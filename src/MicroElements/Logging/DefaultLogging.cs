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
        public static ILoggerFactory ConfigureLogging()
        {
            var loggerFactory = NullLoggerFactory.Instance;
            return loggerFactory;
        }
    }
}
