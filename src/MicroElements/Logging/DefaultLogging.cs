// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroElements.Logging
{
    /// <summary>
    /// Default logging.
    /// </summary>
    public class DefaultLogging
    {
        /// <summary>
        /// Конфигурирование логирования.
        /// </summary>
        /// <returns>Сконфигурированная фабрика логирования.</returns>
        public static ILoggerFactory ConfigureLogging(IServiceCollection services)
        {
            var loggerFactory = NullLoggerFactory.Instance;

            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton<ILogger>(loggerFactory.CreateLogger("Default"));
            services.AddLogging();

            return loggerFactory;
        }
    }
}
