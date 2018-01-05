namespace MicroComponents.Bootstrap.Extensions.Configuration.Evaluation
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
        /// <param name="value">Вычисленное значение.</param>
        /// <returns>true, если выражение успешно вычислено.</returns>
        bool TryEvaluate(string expression, out string value);
    }
}