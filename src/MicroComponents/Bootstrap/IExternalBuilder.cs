using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MicroComponents.Bootstrap
{
    public interface IExternalBuilder
    {
        void AddServices(IEnumerable<ServiceDescriptor> descriptors);
        IServiceProvider ConfigureServices(IBuildContext buildContext);
    }
}