using System;

namespace MicroElements.Bootstrap.Extensions.Configuration.Evaluation
{
    /// <summary>
    /// Вычисление выражений вида ${environment:envVarName}.
    /// Выражение заменяется на Environment Variable.
    /// </summary>
    public class EnvironmentEvaluator : IValueEvaluator
    {
        /// <inheritdoc />
        public string Name => "environment";

        /// <inheritdoc />
        public bool TryEvaluate(string expression, out string value)
        {
            value = Environment.GetEnvironmentVariable(expression);
            return value != null;
        }
    }
}