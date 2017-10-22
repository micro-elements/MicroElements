using MicroComponents.Bootstrap.Extensions.Configuration.Evaluation;

namespace MicroComponents.Bootstrap.Extensions.Configuration
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