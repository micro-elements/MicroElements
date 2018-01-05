using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Bootstrap
{
    public interface IExternalBuilder
    {
        void AddServices(IEnumerable<ServiceDescriptor> descriptors);
        IServiceProvider ConfigureServices(IBuildContext buildContext);
    }
}