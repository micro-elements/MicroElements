// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        string Name { get; }

        /// <summary>
        /// Expression evaluation.
        /// </summary>
        /// <param name="key">Expression key.</param>
        /// <param name="expression">Expression.</param>
        /// <returns>Evaluated result.</returns>
        string Evaluate(string key, string expression);
    }
}
