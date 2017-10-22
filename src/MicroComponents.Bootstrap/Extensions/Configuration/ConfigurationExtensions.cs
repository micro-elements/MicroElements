using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MicroComponents.Bootstrap.Extensions.Configuration
{
    /// <summary>
    /// Extensions for <see cref="IConfiguration"/>.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Получение всех значений конфигурации в виде KeyValuePair.
        /// </summary>
        /// <param name="configuration">Конфигурация.</param>
        /// <returns>Список всех значений.</returns>
        public static KeyValuePair<string, string>[] GetAllValues(this IConfiguration configuration)
        {
            return configuration.AsEnumerable().ToArray();
        }

        /// <summary>
        /// Dumps <see cref="IConfiguration"/> to <see cref="ILoggerFactory"/>.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <param name="loggerFactory">LoggerFactory.</param>
        /// <param name="loggerName">The name of logger.</param>
        public static void DumpConfigurationToLog(this IConfiguration configuration, ILoggerFactory loggerFactory, string loggerName = "Configuration")
        {
            // Note: здесь специально создается именованный логгер, чтобы отфильтровать записи в отдельный файл.
            var configurationLogger = loggerFactory.CreateLogger(loggerName);
            configuration.DumpConfigurationToLog(configurationLogger);
        }

        /// <summary>
        /// Дамп в лог всей конфигурации.
        /// </summary>
        public static void DumpConfigurationToLog(this IConfiguration configuration, ILogger logger)
        {
            var keyValuePairs = configuration.GetAllValues();
            bool IsPassword(string key) => key.Contains("Password");

            foreach (var keyValuePair in keyValuePairs)
            {
                var value = IsPassword(keyValuePair.Key) ? "***" : keyValuePair.Value;
                logger.LogInformation("{0}: {1}", keyValuePair.Key, value);
            }
        }
    }
}