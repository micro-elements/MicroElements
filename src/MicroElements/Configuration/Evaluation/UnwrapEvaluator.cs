using System;
using System.Collections.Generic;
using System.Text;

namespace MicroElements.Configuration.Evaluation;

public class UnwrapEvaluator : IValueEvaluator
{
    public static IValueEvaluator Instance { get; } = new UnwrapEvaluator();

    public static IReadOnlyCollection<IValueEvaluator> AsCollection { get; } = new [] { Instance };

    private UnwrapEvaluator() { }

    public static string Wrap(string expression)
    {
        string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(expression));
        string wrapped = $"${{wrapped:{base64String}}}";
        return wrapped;
    }

    /// <inheritdoc />
    public EvaluatorInfo Info { get; } = new EvaluatorInfo("wrapped", 1000);

    /// <inheritdoc />
    EvaluationResult IValueEvaluator.Evaluate(EvaluationContext context)
    {
        string value = Encoding.UTF8.GetString(Convert.FromBase64String(context.Expression));
        return EvaluationResult.Create(context, value);
    }
}
