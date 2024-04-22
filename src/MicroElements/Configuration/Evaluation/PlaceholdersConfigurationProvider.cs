// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Abstractions;
using MicroElements.Bootstrap;
using MicroElements.Collections.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для вычисления динамических и подстановочных значений (placeholders).
    /// </summary>
    public class PlaceholdersConfigurationProvider : ConfigurationProvider
    {
        private readonly BuildContext _buildContext;
        private readonly IConfigurationBuilder _configurationBuilder;

        private IConfigurationRoot _configurationRoot;
        private IReadOnlyCollection<IValueEvaluator> _evaluators;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceholdersConfigurationProvider"/> class.
        /// </summary>
        /// <param name="buildContext">Build context.</param>
        /// <param name="configurationBuilder">ConfigurationBuilder for initial configuration.</param>
        public PlaceholdersConfigurationProvider(BuildContext buildContext, IConfigurationBuilder configurationBuilder)
        {
            _buildContext = buildContext;
            _configurationBuilder = configurationBuilder;
        }

        /// <inheritdoc />
        public override void Load()
        {
            IValueEvaluator[] CreateValueEvaluators()
            {
                IValueEvaluator[] valueEvaluators = null;

                if (_buildContext.ServiceProvider is { } serviceProvider)
                {
                    valueEvaluators = serviceProvider
                        .GetServices<IValueEvaluator>()
                        .OrderBy(evaluator => evaluator.Info.Order)
                        .ToArray();
                }

                if (valueEvaluators is null || valueEvaluators.Length == 0)
                {
                    valueEvaluators = ValueEvaluator
                        .CreateValueEvaluators(_buildContext, _configurationRoot)
                        .OrderBy(evaluator => evaluator.Info.Order)
                        .ToArray();
                }

                return valueEvaluators;
            }

            _configurationRoot = _configurationBuilder.Build();

            _evaluators = _configurationBuilder.Properties.GetOrAdd(BuilderContext.Key.ValueEvaluators, CreateValueEvaluators);
        }

        /// <inheritdoc />
        public override bool TryGet(string key, out string? value)
        {
            string originalValue = _configurationRoot.GetValue<string>(key);

            if (string.IsNullOrEmpty(originalValue) || !originalValue.HasPlaceholderFor(_evaluators))
            {
                value = null;
            }
            else
            {
                EvaluationResult evaluationResult = SimpleExpressionParser.TryParseAndRender(key, originalValue, _configurationRoot, _evaluators);
                value = evaluationResult.EvaluatedExpression;
            }

            return value != null;
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
        public static bool HasPlaceholderFor(this string value, IReadOnlyCollection<IValueEvaluator> evaluators)
            => value.Contains("$") && evaluators.Any(evaluator => value.Contains(evaluator.PlaceholderTag()));

        public static bool HasPlaceholderFor(this string value, IValueEvaluator evaluator)
            => value.Contains("$") && value.Contains(evaluator.PlaceholderTag());

        public static EvaluationResult TryParseAndRender(
            string key,
            string? valueWithExpression,
            IConfiguration configuration,
            IReadOnlyCollection<IValueEvaluator>? evaluators)
        {
            if (valueWithExpression != null && evaluators != null)
            {
                var valueWithPlaceholder = valueWithExpression;
                string? valueWithPlaceholderPrev = null;
                int placeholderValueEndIndex = 0;
                while (placeholderValueEndIndex >= 0 && valueWithPlaceholderPrev != valueWithPlaceholder)
                {
                    valueWithPlaceholderPrev = valueWithPlaceholder;

                    foreach (var evaluator in evaluators)
                    {
                        var placeholderTag = evaluator.PlaceholderTag();

                        int tagStartIndex = 0;
                        int tagIndex = valueWithPlaceholder.IndexOf(placeholderTag, tagStartIndex, StringComparison.InvariantCultureIgnoreCase);

                        while (tagIndex >= 0)
                        {
                            placeholderValueEndIndex = FindRightBracketIndex(valueWithPlaceholder, tagIndex + placeholderTag.Length);

                            if (placeholderValueEndIndex > 0)
                            {
                                var placeholderValueStartIndex = tagIndex + placeholderTag.Length;
                                string expressionValue = valueWithPlaceholder.Substring(placeholderValueStartIndex, placeholderValueEndIndex - placeholderValueStartIndex);

                                bool hasInnerExpressions = expressionValue.HasPlaceholderFor(evaluators);
                                if (!evaluator.Info.IsUnevaluatedExpressionsAllowed && hasInnerExpressions)
                                {
                                    // will try to find next token with the same tag
                                    tagStartIndex += 1;
                                    tagIndex = valueWithPlaceholder.IndexOf(placeholderTag, tagStartIndex, StringComparison.InvariantCultureIgnoreCase);
                                }
                                else
                                {
                                    // Expression does not have other expressions.
                                    // Evaluate expression.
                                    var context = new EvaluationContext(configuration, evaluators, key, expressionValue, valueWithPlaceholder);
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

                if (valueWithPlaceholder.HasPlaceholderFor(UnwrapEvaluator.Instance))
                {
                    EvaluationResult evaluationResult = TryParseAndRender(key, valueWithPlaceholder, configuration, UnwrapEvaluator.AsCollection);
                    return evaluationResult;
                }

                return new EvaluationResult(key, valueWithExpression, evaluatedExpression: valueWithPlaceholder);
            }

            return new EvaluationResult(key, valueWithExpression, null);
        }

        private static int FindRightBracketIndex(string valueWithPlaceholder, int startIndex)
        {
            int skip = 0;
            int result = -1;
            for (int i = startIndex; i < valueWithPlaceholder.Length; i++)
            {
                char c = valueWithPlaceholder[i];
                if (c == '}' && skip-- == 0)
                {
                    result = i;
                    return result;
                }

                if (c == '{')
                    skip++;
            }

            return result;
        }

        public static string PlaceholderTag(this IValueEvaluator evaluator) => Cache
                .Instance<IValueEvaluator, string>()
                .GetOrAdd(evaluator, ev => $"${{{ev.Info.Name}:");

        public static string? ParseAndRender(string key, string valueWithPlaceholderOriginal, IReadOnlyCollection<IValueEvaluator>? evaluators)
        {
            EvaluationResult tryParseAndRender = TryParseAndRender(key, valueWithPlaceholderOriginal, null, evaluators);
            return tryParseAndRender.EvaluatedExpression;
        }
    }
}
