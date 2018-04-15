using MicroElements.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Tests.Model
{
    public class TestModule2 : IModule
    {
        private readonly TestModule2Configuration _configuration;

        public TestModule2(TestModule2Configuration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new TestModule2Service(_configuration.ConfigurationValue));
        }
    }
}