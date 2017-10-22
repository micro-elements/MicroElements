using System;

namespace MicroComponents.Bootstrap
{
    public interface IBuildStep
    {
        void Execute(BuildContext buildContext);
    }

    public class BuildStepAttribute : Attribute
    {
        private string _name;

        public BuildStepAttribute(string name)
        {
            _name = name;
        }
    }

    public class DependsOnAttribute : Attribute
    {
        private string _buildStepName;

        public DependsOnAttribute(string buildStepName)
        {
            _buildStepName = buildStepName;
        }
    }
}