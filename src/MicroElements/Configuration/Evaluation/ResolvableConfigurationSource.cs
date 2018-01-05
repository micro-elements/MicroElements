using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Bootstrap.Extensions.Configuration.Evaluation
{
    public class ResolvableConfigurationSource<T> : IConfigurationSource where T : IConfigurationSource
    {
        private IServiceProvider _serviceProvider;

        public ResolvableConfigurationSource(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var service = _serviceProvider.GetService<T>();
            return service.Build(builder);
        }
    }
}