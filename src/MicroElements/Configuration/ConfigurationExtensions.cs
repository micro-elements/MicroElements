// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MicroElements.Configuration
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
        /// Default Func to determine secret key.
        /// </summary>
        /// <param name="key">Configuration key.</param>
        /// <returns>True if key is secret.</returns>
        public static bool IsPassword(string key)
        {
            var lowerKey = key?.ToLowerInvariant() ?? string.Empty;
            return lowerKey.Contains("pass") || lowerKey.Contains("secure") || lowerKey.Contains("secret");
        }

        /// <summary>
        /// Dumps <see cref="IConfiguration"/> to <see cref="ILoggerFactory"/>.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <param name="loggerFactory">LoggerFactory.</param>
        /// <param name="isPassword">Func to determine secret key.</param>
        /// <param name="loggerName">The name of logger.</param>
        public static void DumpConfigurationToLog(this IConfiguration configuration, ILoggerFactory loggerFactory, Func<string, bool> isPassword = null, string loggerName = "Configuration")
        {
            isPassword ??= IsPassword;

            // Note: здесь специально создается именованный логгер, чтобы отфильтровать записи в отдельный файл.
            var configurationLogger = loggerFactory.CreateLogger(loggerName);
            configuration.DumpConfigurationToLog(configurationLogger, isPassword);
        }

        /// <summary>
        /// Дамп в лог всей конфигурации.
        /// </summary>
        public static void DumpConfigurationToLog(this IConfiguration configuration, ILogger logger, Func<string, bool> isPassword = null)
        {
            isPassword ??= IsPassword;

            var keyValuePairs = configuration.GetAllValues();
            foreach (var keyValuePair in keyValuePairs)
            {
                var value = isPassword(keyValuePair.Key) ? "***" : keyValuePair.Value;
                logger.LogInformation("{0}: {1}", keyValuePair.Key, value);
            }
        }
    }
}
