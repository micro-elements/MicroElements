// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Bootstrap.Extensions.Configuration.Evaluation;

namespace MicroElements.Bootstrap.Extensions.Configuration
{
    [BuildStep("LoadConfiguration")]
    [DependsOn("Evaluators")]
    public class LoadConfiguration : IBuildStep
    {
        public void Execute(BuildContext buildContext)
        {
            new EvaluatorsModule().Execute(buildContext);
            ConfigurationReader.LoadConfiguration(buildContext);
        }
    }
}