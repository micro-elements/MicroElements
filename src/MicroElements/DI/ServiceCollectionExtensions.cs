// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.Collections.Extensions.Iterate;
using MicroElements.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroElements.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IEnumerable<(Type ImplType, Type[] ServiceTypes)> GetTypesToRegister(this Assembly assembly, Func<Type, Type[]> typeToServiceTypes)
        {
            var typesToRegister = assembly
                .GetDefinedTypesSafe()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .Select(type => (ImplType: type, ServiceTypes: typeToServiceTypes(type)))
                .Where(a => a.ServiceTypes != null);
            return typesToRegister;
        }

        public static IEnumerable<ServiceDescriptor> GetServicesToRegister(this Assembly assembly, Func<Type, Type[]> typeToServiceTypes, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            return assembly
                .GetTypesToRegister(typeToServiceTypes)
                .SelectMany(a => a.ServiceTypes.Select(serviceType => (ServiceType: serviceType, a.ImplType)))
                .Select(a => new ServiceDescriptor(a.ServiceType, a.ImplType, lifetime));
        }

        public static IServiceCollection ExploreAndAddServices(
            this IServiceCollection services,
            Assembly assembly,
            Func<Type, Type[]> typeToServiceTypes,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            assembly
                .GetServicesToRegister(typeToServiceTypes, lifetime)
                .Iterate(services.TryAdd);

            return services;
        }
    }
}
