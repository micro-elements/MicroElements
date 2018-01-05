using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace MicroElements.Bootstrap.Extensions.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для вычисления динамических и подстановочных значений (placeholders).
    /// </summary>
    public class PlaceholdersConfigurationSource : IConfigurationSource
    {
        private readonly IConfigurationRoot _configurationRoot;
        private readonly IEnumerable<IValueEvaluator> _evaluators;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="configurationRoot">Корень конфигурации.</param>
        /// <param name="evaluators"></param>
        public PlaceholdersConfigurationSource(IConfigurationRoot configurationRoot, IEnumerable<IValueEvaluator> evaluators)
        {
            _configurationRoot = configurationRoot;
            _evaluators = evaluators;
        }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new PlaceholdersConfigurationProvider(_configurationRoot, _evaluators);
        }
    }
}