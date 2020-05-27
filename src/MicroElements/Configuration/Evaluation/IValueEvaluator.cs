// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

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
        /// <param name="key">Expression key.</param>
        /// <param name="expression">Expression.</param>
        /// <returns>Evaluated result.</returns>
        string Evaluate(string key, string expression);
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
}
