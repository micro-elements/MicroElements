using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MicroComponents.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MicroComponents.Autofac
{
    public class AutofacContainer : IExternalBuilder
    {
        private readonly ContainerBuilder _containerBuilder;
        public AutofacContainer(ContainerBuilder containerBuilder)
        {
            _containerBuilder = containerBuilder;
        }

        public void AddServices(IEnumerable<ServiceDescriptor> descriptors)
        {
            _containerBuilder.Populate(descriptors);
            //descriptors.AddAutofac(builder => builder.Populate());
        }

        public IServiceProvider ConfigureServices(IBuildContext buildContext)
        {
            //todo: autofac modules
            //todo: named services
            //todo: metadata
            throw new NotImplementedException();
        }
    }
}