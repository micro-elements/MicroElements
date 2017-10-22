using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace MicroComponents.Extensions.Logging.NLog
{
    public class NLogLogging
    {
        /// <summary>
        /// Конфигурирование логирования.
        /// </summary>
        /// <returns>Сконфигурированная фабрика логирования.</returns>
        public static ILoggerFactory ConfigureLogging()
        {
            return new LoggerFactory().AddNLog();
        }
    }
}