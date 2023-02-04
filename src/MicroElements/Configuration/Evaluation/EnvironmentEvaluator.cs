// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Вычисление выражений вида ${environment:envVarName}.
    /// Выражение заменяется на Environment Variable.
    /// </summary>
    public class EnvironmentEvaluator : IValueEvaluator
    {
        /// <inheritdoc />
        public EvaluatorInfo Info { get; } = new EvaluatorInfo("environment", 10);

        /// <inheritdoc />
        EvaluationResult IValueEvaluator.Evaluate(EvaluationContext context)
        {
            string value = Environment.GetEnvironmentVariable(context.Expression);
            return EvaluationResult.Create(context, value);
        }
    }
}
