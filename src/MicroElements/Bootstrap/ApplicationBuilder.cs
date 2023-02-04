// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MicroElements.Abstractions;
using MicroElements.Bootstrap.Extensions;
using MicroElements.Bootstrap.Utils;
using MicroElements.Configuration;
using MicroElements.Logging;
using MicroElements.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MicroElements.Bootstrap
{
    /// <summary>
    /// Построитель приложения.
    /// </summary>
    public class ApplicationBuilder : IApplicationBuilder
    {
        private BuildContext _buildContext;

        /// <summary>
        /// Составляем приложение.
        /// </summary>
        /// <param name="startupConfiguration">Параметры запуска приложения.</param>
        /// <returns>Провайдер к коллекции зарегистрированных сервисов.</returns>
        public IBuildContext Build(StartupConfiguration startupConfiguration)
        {
            _buildContext = new BuildContext { StartupConfiguration = startupConfiguration, };
            var measureSession = new MeasureSession("Application startup");

            // Разбор параметров командной строки
            if (startupConfiguration.SetPropertiesFromCommandLineArgs)
            {
                startupConfiguration.BuildUpFromCommandLineArgs(startupConfiguration.CommandLineArgs.Args);
            }

            SetEnvVariables(startupConfiguration);

            // Can use defined service collection or create new
            _buildContext.ServiceCollection = startupConfiguration.ServiceCollection ?? new ServiceCollection();
            var serviceCollection = _buildContext.ServiceCollection;

            using (measureSession.StartTimer("ConfigureLogging"))
            {
                // Получение сконфигурированной фабрики логирования.
                var configureLogging = startupConfiguration.ConfigureLogging ?? DefaultLogging.ConfigureLogging;
                _buildContext.LoggerFactory = configureLogging(_buildContext.ServiceCollection);
                _buildContext.Logger = _buildContext.LoggerFactory.CreateLogger("Bootstrap");
            }

            using (measureSession.StartTimer("LoadTypes"))
            {
                // Получение информации об окружении.
                _buildContext.StartupInfo = GetStartupInfo();

                // Переключим текущую директорию на директорию запуска.
                Directory.SetCurrentDirectory(_buildContext.StartupInfo.BaseDirectory);
                _buildContext.StartupInfo.CurrentDirectory = _buildContext.StartupInfo.BaseDirectory;

                // Loading assemblies and types
                AssemblySource assemblySource = new(
                    loadFromDomain: true,
                    loadFromDirectory: _buildContext.StartupInfo.BaseDirectory,
                    searchPatterns: _buildContext.StartupConfiguration.AssemblyScanPatterns);
                TypeFilters typeFilters = TypeFilters.AllPublicTypes;

                _buildContext.Assemblies = assemblySource
                    .LoadAssemblies()
                    .ToArray();

                _buildContext.ExportedTypes = _buildContext
                    .Assemblies
                    .GetTypes(typeFilters)
                    .ToArray();

                _buildContext.Logger.LogDebug($"Loaded {_buildContext.Assemblies.Length} assemblies");

                if (_buildContext.Assemblies.Length > 20)
                {
                    var assemblyScanPatterns = _buildContext.StartupConfiguration.AssemblyScanPatterns;
                    var assemblyScanPatternsText = string.Join(",", assemblyScanPatterns);
                    _buildContext.Logger.LogWarning($"Diagnostic: too many assemblies found. Specify AssemblyScanPatterns. Loaded: {_buildContext.Assemblies.Length} assemblies, AssemblyScanPatterns: {assemblyScanPatternsText}");
                }

                _buildContext.LogHeader();//todo: assemblies loaded? types loaded
            }

            using (measureSession.StartTimer("LoadConfiguration"))
            {
                // Загрузка конфигурации
                ConfigurationReader.LoadConfiguration(_buildContext, reloadOnChange: startupConfiguration.ReloadOnChange);

                // Регистрируем конфигурацию в виде IConfiguration и IConfigurationRoot
                serviceCollection.Replace(ServiceDescriptor.Singleton<IConfiguration>(_buildContext.ConfigurationRoot));
                serviceCollection.Replace(ServiceDescriptor.Singleton<IConfigurationRoot>(_buildContext.ConfigurationRoot));
            }

            using (measureSession.StartTimer("ConfigureServices"))
            {
                try
                {
                    // Конфигурирование сервисов
                    ConfigureServices(_buildContext);

                    // Строим провайдер.
                    _buildContext.ServiceProvider = _buildContext.ServiceCollection.BuildServiceProvider();
                }
                catch (Exception exception)
                {
                    _buildContext.Logger.LogError(new EventId(0), exception, exception.Message);
                    throw;
                }
            }

            // Dump значений конфигурации в лог
            if (startupConfiguration.DumpConfigurationToLog)
            {
                _buildContext.ConfigurationRoot.DumpConfigurationToLog(_buildContext.LoggerFactory, startupConfiguration.IsSecretConfigurationKey, "Configuration");
            }

            measureSession.LogMeasures(_buildContext.Logger);

            return _buildContext;
        }

        /// <inheritdoc />
        public IApplicationBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            throw new NotImplementedException();
        }

        public static void SetEnvVariables(StartupConfiguration configuration)
        {
            var fullLogsPath = Path.IsPathRooted(configuration.LogsPath)
                ? configuration.LogsPath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration.LogsPath);

            var flatProfileName = configuration.Profile?.CleanFileName().Replace(Path.DirectorySeparatorChar, '_');

            Environment.SetEnvironmentVariable("LogsPath", fullLogsPath, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ProfileName", flatProfileName, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("Profile", configuration.Profile, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ConfigurationPath", configuration.ConfigurationPath, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("InstanceId", configuration.InstanceId, EnvironmentVariableTarget.Process);
        }

        /// <summary>
        /// Конфигурирование сервисов.
        /// </summary>
        /// <param name="buildContext">Контекст построения приложения.</param>
        public void ConfigureServices(BuildContext buildContext)
        {
            var logger = buildContext.Logger;

            StartupConfiguration startupConfiguration = buildContext.StartupConfiguration;
            var configurationRoot = buildContext.ConfigurationRoot;
            var services = buildContext.ServiceCollection;

            // Заменяем реализацию фабрики опций на свою.
            services.TryAdd(ServiceDescriptor.Transient(typeof(IOptionsFactory<>), typeof(Configuration.OptionsFactory<>)));

            // Добавляем поддержку IOptions, IOptionsSnapshot, IOptionsMonitor
            services.AddOptions();

            // Logging
            services.RegisterLogging(buildContext.LoggerFactory);

            // Регистрируем строготипизированные конфигурации.
            services.RegisterConfigurationTypes(configurationRoot, buildContext.ExportedTypes, startupConfiguration);

            // Регистрируем типы по атрибуту [Register]
            services.RegisterWithRegisterAttribute(buildContext.ExportedTypes);

            // todo: зарегистрировать не исходные типы, а результирующий
            services.AddSingleton(buildContext.StartupConfiguration);
            services.AddSingleton(buildContext.StartupInfo);
        }

        /// <summary>
        /// Returns some startup info.
        /// </summary>
        /// <returns>StartupInfo.</returns>
        public static StartupInfo GetStartupInfo()
        {
            var info = new StartupInfo();
            var executingAssembly = Assembly.GetExecutingAssembly();
            info.StartupApp = Path.GetFileName(executingAssembly.Location);
            info.Version = executingAssembly.GetName().Version.ToString(3);
            info.BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            info.CurrentDirectory = Directory.GetCurrentDirectory();

            return info;
        }
    }
}
