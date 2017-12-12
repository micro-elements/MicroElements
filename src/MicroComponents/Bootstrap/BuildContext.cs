using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroComponents.Bootstrap
{
    /// <summary>
    /// Контекст построения.
    /// </summary>
    public class BuildContext : IBuildContext
    {
        /// <summary>
        /// Параметры запуска приложения.
        /// </summary>
        public StartupConfiguration StartupConfiguration;

        /// <summary>
        /// ServiceCollection. It can be passed from <see cref="StartupConfiguration"/> or created inside build process.
        /// </summary>
        public IServiceCollection ServiceCollection { get; set; }

        /// <summary>
        /// Service provider builded on the end of build process.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Root of loaded configuration.
        /// </summary>
        public IConfigurationRoot ConfigurationRoot { get; set; }

        /// <summary>
        /// Фабрика логирования.
        /// </summary>
        public ILoggerFactory LoggerFactory = NullLoggerFactory.Instance;

        /// <summary>
        /// Основной логгер для процесса инициализации.
        /// </summary>
        public ILogger Logger = NullLogger.Instance;

        /// <summary>
        /// Информация об окружении.
        /// </summary>
        public StartupInfo StartupInfo;

        /// <summary>
        /// Loaded assemblies.
        /// </summary>
        public Assembly[] Assemblies { get; set; }

        /// <summary>
        /// All exported types from loaded assemblies.
        /// </summary>
        public Type[] ExportedTypes { get; set; }

        /// <summary>
        /// Usefull information collected on build process. This information can be logged after build process finished.
        /// </summary>
        public List<KeyValuePair<string, string>> BuildInfo = new List<KeyValuePair<string, string>>();

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        /// <summary>
        /// Вывод в лог заголовочной информации.
        /// </summary>
        public void LogHeader()
        {
            Logger.LogInformation("*************************************");
            Logger.LogInformation("StartTime: {0}", DateTime.Now);
            Logger.LogInformation("Version  : {0}", StartupInfo.Version);
            Logger.LogInformation("Profile  : {0}", StartupConfiguration.Profile);
            Logger.LogInformation("LogsPath : {0}", StartupConfiguration.LogsPath);
            Logger.LogInformation("Instance : {0}", StartupConfiguration.InstanceId);
            Logger.LogInformation("WorkMode : {0}", Environment.UserInteractive ? "Console" : "Service");
            Logger.LogInformation("*************************************");

            Logger.LogInformation("StartupApp      : {0}", StartupInfo.StartupApp);
            Logger.LogInformation("BaseDirectory   : {0}", StartupInfo.BaseDirectory);
            Logger.LogInformation("CurrentDir      : {0}", StartupInfo.CurrentDirectory);

            foreach (var pair in BuildInfo)
            {
                Logger.LogInformation("{0}      : {0}", pair.Key, pair.Key);
            }
            Logger.LogInformation("*************************************");
        }
    }
}
