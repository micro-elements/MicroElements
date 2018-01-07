// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.DependencyInjection;

namespace MicroElements.Bootstrap.Extensions.Configuration.Evaluation
{
    [BuildStep("Evaluators")]
    public class EvaluatorsModule : IBuildStep
    {
        public void Execute(BuildContext buildContext)
        {
            buildContext.ServiceCollection.AddSingletons<IValueEvaluator>(buildContext.ExportedTypes);
        }
    }
}