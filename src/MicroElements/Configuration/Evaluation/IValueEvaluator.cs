// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Expression evaluator.
    /// </summary>
    public interface IValueEvaluator
    {
        /// <summary>
        /// The name of evaluator.
        /// </summary>
        EvaluatorInfo Info { get; }

        /// <summary>
        /// Expression evaluation.
        /// </summary>
        /// <param name="context">Evaluation context.</param>
        /// <returns>Evaluation result.</returns>
        EvaluationResult Evaluate(EvaluationContext context);
    }

    /// <summary>
    /// Represents evaluator information.
    /// </summary>
    public class EvaluatorInfo
    {
        /// <summary>
        /// The name of evaluator.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Evaluator order.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluatorInfo"/> class.
        /// </summary>
        /// <param name="name">The name of evaluator.</param>
        /// <param name="order">Evaluator order.</param>
        public EvaluatorInfo(string name, int order = 100)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Order = order;
        }
    }

    /// <summary>
    /// Evaluation result.
    /// </summary>
    public class EvaluationResult
    {
        /// <summary>
        /// Input key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Input expression.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Evaluated key or null if key was not changed.
        /// </summary>
        public string EvaluatedKey { get; }

        /// <summary>
        /// Evaluated expression or null if expression was not evaluated.
        /// </summary>
        public string EvaluatedExpression { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationResult"/> class.
        /// </summary>
        /// <param name="key">Input key.</param>
        /// <param name="expression">Input expression.</param>
        /// <param name="evaluatedExpression">Evaluated expression or null if expression was not evaluated.</param>
        /// <param name="evaluatedKey">Evaluated key or null if key was not changed.</param>
        public EvaluationResult(
            string key,
            string expression,
            string evaluatedExpression,
            string evaluatedKey = null)
        {
            Key = key;
            Expression = expression;

            EvaluatedKey = evaluatedKey;
            EvaluatedExpression = evaluatedExpression;
        }

        /// <summary>
        /// Creates <see cref="EvaluationResult"/> from <see cref="EvaluationContext"/>.
        /// </summary>
        /// <param name="context">EvaluationContext.</param>
        /// <param name="evaluatedExpression">Evaluated expression or null if expression was not evaluated.</param>
        /// <returns>EvaluationResult instance.</returns>
        public static EvaluationResult Create(EvaluationContext context, string evaluatedExpression)
        {
            return new EvaluationResult(
                context.Key,
                context.Expression,
                evaluatedExpression: evaluatedExpression,
                evaluatedKey: null);
        }
    }

    /// <summary>
    /// Evaluation Context.
    /// </summary>
    public class EvaluationContext
    {
        /// <summary>
        /// Configuration to use.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Evaluators.
        /// </summary>
        public IReadOnlyCollection<IValueEvaluator> Evaluators { get; }

        /// <summary>
        /// Key to evaluate.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Expression to evaluate.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationContext"/> class.
        /// </summary>
        /// <param name="configuration">Configuration to use.</param>
        /// <param name="evaluators">Evaluators.</param>
        /// <param name="key">Key to evaluate.</param>
        /// <param name="expression">Expression to evaluate.</param>
        public EvaluationContext(IConfiguration configuration, IReadOnlyCollection<IValueEvaluator> evaluators, string key, string expression)
        {
            Configuration = configuration;
            Evaluators = evaluators;
            Key = key;
            Expression = expression;
        }
    }
}
