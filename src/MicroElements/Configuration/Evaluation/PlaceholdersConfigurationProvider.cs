// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для вычисления динамических и подстановочных значений (placeholders).
    /// </summary>
    public class PlaceholdersConfigurationProvider : ConfigurationProvider
    {
        private readonly IConfigurationRoot _configurationRoot;
        private readonly IReadOnlyCollection<IValueEvaluator> _evaluators;
        private readonly Dictionary<string, string> _propertiesWithPlaceholders;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceholdersConfigurationProvider"/> class.
        /// </summary>
        /// <param name="configurationRoot">Корень конфигурации.</param>
        /// <param name="evaluators">Список вычислителей.</param>
        public PlaceholdersConfigurationProvider(IConfigurationRoot configurationRoot, IEnumerable<IValueEvaluator> evaluators)
        {
            _configurationRoot = configurationRoot;
            _evaluators = evaluators.OrderBy(evaluator => evaluator.Info.Order).ToArray();

            _propertiesWithPlaceholders = GetPropertiesWithPlaceholders(configurationRoot);
        }

        /// <inheritdoc />
        public override bool TryGet(string key, out string value)
        {
            _propertiesWithPlaceholders.TryGetValue(key, out string valueWithPlaceholder);

            return SimpleExpressionParser.TryParseAndRender(key, valueWithPlaceholder, _evaluators, out value);
        }

        private Dictionary<string, string> GetPropertiesWithPlaceholders(IConfigurationRoot configurationRoot)
        {
            return configurationRoot
                .GetAllValues()
                .Where(pair => pair.Value != null && pair.Value.HasPlaceholderFor(_evaluators))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }

    public static class SimpleExpressionParser
    {
        public static bool HasPlaceholderFor(this string value, IReadOnlyCollection<IValueEvaluator> evaluators) => evaluators.Any(evaluator => value.Contains(evaluator.PlaceholderTag()));

        public static bool TryParseAndRender(string key, string valueWithPlaceholderOriginal, IReadOnlyCollection<IValueEvaluator> evaluators, out string value)
        {
            if (valueWithPlaceholderOriginal != null && evaluators != null)
            {
                var valueWithPlaceholder = valueWithPlaceholderOriginal;
                string valueWithPlaceholderPrev = null;
                int placeholderValueEndIndex = 0;
                while (placeholderValueEndIndex >= 0 && valueWithPlaceholderPrev != valueWithPlaceholder)
                {
                    valueWithPlaceholderPrev = valueWithPlaceholder;

                    foreach (var evaluator in evaluators)
                    {
                        var placeholderTag = evaluator.PlaceholderTag();

                        int startIndex = 0;
                        int tagIndex = valueWithPlaceholder.IndexOf(placeholderTag, startIndex, StringComparison.InvariantCultureIgnoreCase);

                        while (tagIndex >= 0)
                        {
                            placeholderValueEndIndex = valueWithPlaceholder.IndexOf('}', tagIndex);

                            if (placeholderValueEndIndex > 0)
                            {
                                var placeholderValueStartIndex = tagIndex + placeholderTag.Length;
                                string expressionValue = valueWithPlaceholder.Substring(placeholderValueStartIndex, placeholderValueEndIndex - placeholderValueStartIndex);

                                if (!expressionValue.HasPlaceholderFor(evaluators))
                                {
                                    string evaluatedValue = evaluator.Evaluate(key, expressionValue);
                                    evaluatedValue ??= string.Empty;

                                    if (evaluatedValue == expressionValue)
                                    {
                                        // value was unchanged - try next evaluator
                                        break;
                                    }

                                    if (tagIndex == 0 && placeholderValueEndIndex == valueWithPlaceholderOriginal.Length - 1)
                                    {
                                        value = evaluatedValue;
                                        return true;
                                    }

                                    var placeholder = valueWithPlaceholder.Substring(tagIndex, placeholderValueEndIndex - tagIndex + 1);
                                    valueWithPlaceholder = valueWithPlaceholder.Replace(placeholder, evaluatedValue);
                                    break;
                                }
                                else
                                {
                                    // will try to find next token with the same tag
                                    startIndex = startIndex + 1;
                                    tagIndex = valueWithPlaceholder.IndexOf(placeholderTag, startIndex, StringComparison.InvariantCultureIgnoreCase);
                                }
                            }
                            else if (placeholderValueEndIndex == -1)
                            {
                                // no close bracket.
                                break;
                            }
                        }
                    }
                }

                value = valueWithPlaceholder;
                return true;
            }

            value = null;
            return false;
        }

        public static string PlaceholderTag(this IValueEvaluator evaluator) => $"${{{evaluator.Info.Name}:";

        public static string ParseAndRender(string key, string valueWithPlaceholderOriginal, IReadOnlyCollection<IValueEvaluator> evaluators)
        {
            TryParseAndRender(key, valueWithPlaceholderOriginal, evaluators, out string value);
            return value;
        }

        public static string ParseAndRender(string valueWithPlaceholderOriginal, IReadOnlyCollection<IValueEvaluator> evaluators)
        {
            TryParseAndRender(string.Empty, valueWithPlaceholderOriginal, evaluators, out string value);
            return value;
        }
    }
}
