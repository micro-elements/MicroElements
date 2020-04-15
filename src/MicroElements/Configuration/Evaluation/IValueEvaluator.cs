// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Вычислитель значения.
    /// </summary>
    public interface IValueEvaluator
    {
        /// <summary>
        /// The name of evaluator.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Вычисление значения.
        /// </summary>
        /// <param name="expression">Выражение для вычисления.</param>
        /// <returns>Вычисленное значение.</returns>
        string Evaluate(string expression);
    }
}
