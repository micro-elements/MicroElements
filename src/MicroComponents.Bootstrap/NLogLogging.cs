using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace MicroComponents.Bootstrap
{
    public class NLogLogging
    {
        /// <summary>
        /// Конфигурирование логирования.
        /// </summary>
        /// <returns>Сконфигурированная фабрика логирования.</returns>
        public static ILoggerFactory ConfigureLogging()
        {
            var loggerFactory = new LoggerFactory()
                .AddNLog();

            return loggerFactory;
        }
    }
}