namespace MicroElements.Tests.Model
{
    public class TestModule2Service
    {
        public string ConfigurationValue { get; }

        public TestModule2Service(string configurationValue)
        {
            ConfigurationValue = configurationValue;
        }
    }
}