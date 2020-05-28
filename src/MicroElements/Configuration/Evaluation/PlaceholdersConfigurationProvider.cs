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

            var evaluationResult = SimpleExpressionParser.TryParseAndRender(key, valueWithPlaceholder, _configurationRoot, _evaluators);
            value = evaluationResult.EvaluatedExpression;
            return evaluationResult.EvaluatedExpression != null;
        }

        private Dictionary<string, string> GetPropertiesWithPlaceholders(IConfigurationRoot configurationRoot)
        {
            return configurationRoot
                .GetAllValues()
                .Where(pair => pair.Value != null && pair.Value.HasPlaceholderFor(_evaluators))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }

    //TODO: support UsedKeys for secure with ***
    internal class ConfigurationValueWithExpression
    {
        public string Key { get; }
        public string Expression { get; }
        public string EvaluatedValue { get; }
        public string[] UsedKeys { get; }
    }

    public static class SimpleExpressionParser
    {
        public static bool HasPlaceholderFor(this string value, IReadOnlyCollection<IValueEvaluator> evaluators) => evaluators.Any(evaluator => value.Contains(evaluator.PlaceholderTag()));

        public static EvaluationResult TryParseAndRender(
            string key,
            string valueWithExpression,
            IConfiguration configuration,
            IReadOnlyCollection<IValueEvaluator> evaluators)
        {
            if (valueWithExpression != null && evaluators != null)
            {
                var valueWithPlaceholder = valueWithExpression;
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

                                bool hasInnerExpressions = expressionValue.HasPlaceholderFor(evaluators);
                                if (hasInnerExpressions)
                                {
                                    // will try to find next token with the same tag
                                    startIndex = startIndex + 1;
                                    tagIndex = valueWithPlaceholder.IndexOf(placeholderTag, startIndex, StringComparison.InvariantCultureIgnoreCase);
                                }
                                else
                                {
                                    // Expression does not have other expressions.
                                    // Evaluate expression.
                                    var context = new EvaluationContext(configuration, evaluators, key, expressionValue);
                                    EvaluationResult evaluationResult = evaluator.Evaluate(context);
                                    string evaluatedValue = evaluationResult.EvaluatedExpression ?? string.Empty;

                                    bool evaluatedValueHasInnerExpressions = evaluatedValue.HasPlaceholderFor(evaluators);
                                    if (evaluatedValueHasInnerExpressions && evaluationResult.EvaluatedKey != null)
                                    {
                                        var deepResult = TryParseAndRender(
                                            evaluationResult.EvaluatedKey,
                                            evaluationResult.EvaluatedExpression,
                                            configuration,
                                            evaluators);

                                        evaluatedValue = deepResult.EvaluatedExpression;
                                    }

                                    //if (tagIndex == 0 && placeholderValueEndIndex == valueWithPlaceholderOriginal.Length - 1)
                                    //{
                                    //    value = evaluatedValue;
                                    //    return true;
                                    //}

                                    var placeholder = valueWithPlaceholder.Substring(tagIndex, placeholderValueEndIndex - tagIndex + 1);
                                    valueWithPlaceholder = valueWithPlaceholder.Replace(placeholder, evaluatedValue);
                                    break;
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

                return new EvaluationResult(key, valueWithExpression, evaluatedExpression: valueWithPlaceholder);
            }

            return new EvaluationResult(key, valueWithExpression, null);
        }

        public static string PlaceholderTag(this IValueEvaluator evaluator) => $"${{{evaluator.Info.Name}:";

        public static string ParseAndRender(string key, string valueWithPlaceholderOriginal, IReadOnlyCollection<IValueEvaluator> evaluators)
        {
            return TryParseAndRender(key, valueWithPlaceholderOriginal, null, evaluators).EvaluatedExpression;
        }

        public static string ParseAndRender(string valueWithPlaceholderOriginal, IReadOnlyCollection<IValueEvaluator> evaluators)
        {
            return TryParseAndRender(string.Empty, valueWithPlaceholderOriginal, null, evaluators).EvaluatedExpression;
        }
    }
}
