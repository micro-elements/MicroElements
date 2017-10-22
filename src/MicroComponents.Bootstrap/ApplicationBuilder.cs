using System;
using System.IO;
using System.Linq;
using MicroComponents.Bootstrap.Extensions;
using MicroComponents.Bootstrap.Extensions.Configuration;
using MicroComponents.Bootstrap.Utils;
using MicroComponents.Bootstrap.Extensions.Logging;
using MicroComponents.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroComponents.Bootstrap
{
    /// <summary>
    /// Построитель приложения.
    /// </summary>
    public class ApplicationBuilder : IApplicationBuilder
    {
        private BuildContext _buildContext;

        /// <summary>
        /// Составляем и запускаем приложение.
        /// </summary>
        /// <param name="startupConfiguration">Параметры запуска приложения.</param>
        /// <returns>Провайдер к коллекции зарегистрированных сервисов.</returns>
        public IServiceProvider BuildAndStart(StartupConfiguration startupConfiguration)
        {
            var serviceProvider = Build(startupConfiguration);
            return Start(serviceProvider);
        }

        public IServiceCollection Configure(StartupConfiguration startupConfiguration)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Составляем приложение.
        /// </summary>
        /// <param name="startupConfiguration">Параметры запуска приложения.</param>
        /// <returns>Провайдер к коллекции зарегистрированных сервисов.</returns>
        public IServiceProvider Build(StartupConfiguration startupConfiguration)
        {
            _buildContext = new BuildContext { StartupConfiguration = startupConfiguration, };
            var measureSession = new MeasureSession("Конфигурирование служб");

            // Разбор параметров командной строки
            startupConfiguration.BuildUpFromCommandLineArgs(startupConfiguration.CommandLineArgs.Args);

            _buildContext.ServiceCollection = startupConfiguration.ServiceCollection ?? new ServiceCollection();
            var serviceCollection = _buildContext.ServiceCollection;
            
            using (measureSession.StartTimer("ConfigureLogging"))
            {
                // Установка путей логирования, создание и блокирование pid-файла
                var unlocker = LoggingExtensions.SetupLogsPath(startupConfiguration);
                serviceCollection.AddSingleton(unlocker);

                // Получение сконфигурированной фабрики логирования.
                var configureLogging = startupConfiguration.ConfigureLogging ?? NLogLogging.ConfigureLogging;
                _buildContext.LoggerFactory = configureLogging();
                _buildContext.Logger = _buildContext.LoggerFactory.CreateLogger("Bootstrap");
            }

            using (measureSession.StartTimer("LoadTypes"))
            {
                // Получение информации об окружении.
                _buildContext.StartupInfo = ReflectionUtils.GetStartupInfo();

                // Переключим текущую директорию на директорию запуска.
                Directory.SetCurrentDirectory(_buildContext.StartupInfo.StartupDir);
                _buildContext.StartupInfo.CurrentDirectory = _buildContext.StartupInfo.StartupDir;

                // Загрузка сборок в память
                _buildContext.Assemblies = ReflectionUtils.LoadAssemblies(_buildContext.StartupInfo.StartupDir, _buildContext.StartupConfiguration.AssemblyScanPatterns);

                //todo: diagnostics to many assemblies. set AssemblyScanPatterns

                // Список типов
                _buildContext.ExportedTypes = _buildContext.Assemblies.SelectMany(assembly => assembly.GetDefinedTypesSafe()).ToArray();

                _buildContext.LogHeader();//todo: assemblies loaded? types loaded
            }

            using (measureSession.StartTimer("LoadConfiguration"))
            {
                // Загрузка конфигурации
                new LoadConfiguration().Execute(_buildContext);

                // Dump значений конфигурации в лог
                if (startupConfiguration.DumpConfigurationToLog)
                {
                    _buildContext.ConfigurationRoot.DumpConfigurationToLog(_buildContext.LoggerFactory);
                }
            };

            IServiceProvider serviceProvider;
            using (measureSession.StartTimer("ConfigureServices"))
            {
                try
                {
                    // Конфигурирование сервисов
                    serviceProvider = ConfigureServices(_buildContext);     
                }
                catch (Exception exception)
                {
                    _buildContext.Logger.LogError(new EventId(0), exception, exception.Message);
                    throw;
                }
            }

            var externalBuilder = startupConfiguration.ExternalBuilder;
            if (externalBuilder != null)
            {
                serviceProvider = ConfigureServicesExt(_buildContext);
            }

            measureSession.LogMeasures(_buildContext.Logger);

            return serviceProvider;
        }

        /// <summary>
        /// Запускаем приложение.
        /// </summary>
        /// <param name="serviceProvider">Провайдер сервисов.</param>
        /// <returns>Провайдер к коллекции зарегистрированных сервисов.</returns>
        public IServiceProvider Start(IServiceProvider serviceProvider)
        {
            _buildContext.Logger.LogInformation("Запуск служб");
            var measureSession = new MeasureSession("Запуск служб");
            measureSession.ExecuteWithTimer("StartRunnables", () =>
            {
                // Запуск сервисов.
                serviceProvider.StartRunnablesAsync(_buildContext.Logger).Wait();
            });

            measureSession.LogMeasures(_buildContext.Logger);

            return serviceProvider;
        }

        /// <summary>
        /// Конфигурирование сервисов.
        /// </summary>
        /// <param name="buildContext">Контекст построения приложения.</param>
        /// <returns>Сконфигурированный <see cref="IServiceProvider"/></returns>
        public IServiceProvider ConfigureServices(BuildContext buildContext)
        {
            buildContext.Logger.LogInformation("ConfigureServices started");

            StartupConfiguration startupConfiguration = buildContext.StartupConfiguration;
            ILoggerFactory loggerFactory = buildContext.LoggerFactory;
            var configurationRoot = buildContext.ConfigurationRoot;
            var services = buildContext.ServiceCollection;

            // Logging
            services.RegisterLogging(loggerFactory);

            // Регистрируем строготипизированные конфигурации.
            services.RegisterConfigurationTypes(configurationRoot, buildContext.ExportedTypes, startupConfiguration.Profile);

            // Регистрируем типы по атрибуту [Register]
            services.RegisterWithRegisterAttribute(buildContext.ExportedTypes);

            // Регистрируем модули.
            var moduleTypes = buildContext.ExportedTypes.GetClassTypesAssignableTo<IModule>().ToList();
            if (moduleTypes.Count > 0)
            {
                // Временный контейнер с внедренной конфигурацией и логированием.
                IServiceCollection moduleServices = new ServiceCollection()
                    .RegisterLogging(loggerFactory)
                    .RegisterConfigurationTypes(configurationRoot, buildContext.ExportedTypes, startupConfiguration.Profile)
                    .RegisterWithRegisterAttribute(buildContext.ExportedTypes);

                // Создаем экземпляры модулей.
                var modules = moduleServices.ResolveModules(moduleTypes);

                // Модули регистрируют свои сервисы.
                modules.ForEach(module => module.ConfigureServices(services));
            }

            // Строим провайдер.
            var serviceProvider = services.BuildServiceProvider();

            buildContext.Logger.LogInformation("ConfigureServices finished");

            return serviceProvider;
        }

        /// <summary>
        /// Конфигурирование сервисов.
        /// </summary>
        /// <param name="buildContext">Контекст построения приложения.</param>
        /// <returns>Сконфигурированный <see cref="IServiceProvider"/></returns>
        public IServiceProvider ConfigureServicesExt(BuildContext buildContext)
        {
            var builder = buildContext.StartupConfiguration.ExternalBuilder;

            buildContext.Logger.LogInformation("ConfigureServicesExt started");

            // Регистрируем сервисы, переданные снаружи.
            builder.AddServices(buildContext.ServiceCollection);

            // Запускаем конфигурацию.
            var serviceProvider = builder.ConfigureServices(buildContext);

            buildContext.Logger.LogInformation("ConfigureServicesExt finished");

            return serviceProvider;
        }
    }
}