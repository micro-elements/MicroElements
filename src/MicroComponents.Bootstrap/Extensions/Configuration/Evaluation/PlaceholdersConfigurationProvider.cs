using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MicroComponents.Bootstrap.Extensions.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для вычисления динамических и подстановочных значений (placeholders).
    /// </summary>
    public class PlaceholdersConfigurationProvider : ConfigurationProvider
    {
        private readonly IConfigurationRoot _configurationRoot;
        private readonly IEnumerable<IValueEvaluator> _evaluators;
        private readonly Dictionary<string,IValueEvaluator> _evaluators2 = new Dictionary<string, IValueEvaluator>();
        private readonly Dictionary<string, string> _propertiesWithPlaceholders;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="configurationRoot">Корень конфигурации.</param>
        /// <param name="evaluators">Список вычислителей.</param>
        public PlaceholdersConfigurationProvider(IConfigurationRoot configurationRoot, IEnumerable<IValueEvaluator> evaluators)
        {
            _configurationRoot = configurationRoot;
            _evaluators = evaluators;
            foreach (var evaluator in _evaluators)
            {
                _evaluators2[evaluator.Name] = evaluator;
            }
            _propertiesWithPlaceholders = GetPropertiesWithPlaceholders(configurationRoot);
        }

        private static string GetEvaluatorTag(IValueEvaluator evaluator)
        {
            return evaluator.Name;
        }

        /// <inheritdoc />
        public override bool TryGet(string key, out string value)
        {
            value = null;
            _propertiesWithPlaceholders.TryGetValue(key, out string valueWithPlaceholderOriginal);
            if (valueWithPlaceholderOriginal != null)
            {
                var valueWithPlaceholder = valueWithPlaceholderOriginal;
                int placeholderValueEndIndex = 0;
                while (placeholderValueEndIndex >= 0 && valueWithPlaceholder.Contains("${") && valueWithPlaceholder.Contains("}"))
                {
                    foreach (var evaluator in _evaluators)
                    {
                        var evaluatorName = evaluator.Name;
                        var placeholderTag = $"${{{evaluatorName}:";
                        if (valueWithPlaceholder.Contains(placeholderTag))
                        {
                            int tagIndex = valueWithPlaceholder.IndexOf(placeholderTag, StringComparison.InvariantCultureIgnoreCase);
                            placeholderValueEndIndex = valueWithPlaceholder.IndexOf('}', tagIndex);
                            if (placeholderValueEndIndex > 0)
                            {
                                var placeholderValueStartIndex = tagIndex + placeholderTag.Length;
                                string expressionValue = valueWithPlaceholder.Substring(placeholderValueStartIndex, placeholderValueEndIndex - placeholderValueStartIndex);
                                if (evaluator.TryEvaluate(expressionValue, out string evaluatedValue))
                                {
                                    if (tagIndex == 0 && placeholderValueEndIndex == valueWithPlaceholderOriginal.Length - 1)
                                    {
                                        value = evaluatedValue;
                                        return true;
                                    }
                                    var placeholder = valueWithPlaceholder.Substring(tagIndex, placeholderValueEndIndex - tagIndex + 1);
                                    value = valueWithPlaceholder.Replace(placeholder, evaluatedValue);
                                    valueWithPlaceholder = value;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return value != null;
        }

        private static bool HasPlaceholder(string value, object tag)
        {
            return value.Contains($"${{{tag}:");
        }

        private static bool HasPlaceholder(string value, IEnumerable<object> tags)
        {
            return tags.Any(tag => HasPlaceholder(value, tag));
        }

        private Dictionary<string, string> GetPropertiesWithPlaceholders(IConfigurationRoot configurationRoot)
        {
            var evaluatorTags = _evaluators.Select(evaluator => evaluator.Name).ToArray();

            return configurationRoot
                .GetAllValues()
                .Where(pair => pair.Value != null && HasPlaceholder(pair.Value, evaluatorTags))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}