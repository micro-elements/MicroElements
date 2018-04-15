using MicroElements.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Tests.Model
{
    public class TestModule1 : IModule
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<TestModule1Service>();
        }
    }
}