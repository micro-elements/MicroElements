using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSingletons(this IServiceCollection services, Assembly assembly, Func<Type, bool> typeToRegister)
        {
            assembly
                .GetDefinedTypesSafe()
                .Where(typeToRegister)
                .Iter(type => services.AddSingleton(type));

            return services;
        }

        public static IServiceCollection AddSingletons2(this IServiceCollection services, Assembly assembly, Func<Type, Type> typeToServiceType)
        {
            assembly
                .GetDefinedTypesSafe()
                .Select(type => new {Type = type, ServiceType = typeToServiceType(type)})
                .Where(a => a.ServiceType != null)
                .Iter(a => services.AddSingleton(a.ServiceType, a.Type));

            return services;
        }

        public static IServiceCollection AddSingletons3(this IServiceCollection services, Assembly assembly, Func<Type, Type[]> typeToServiceTypes)
        {
            assembly
                .GetDefinedTypesSafe()
                .Select(type => new {Type = type, ServiceTypes = typeToServiceTypes(type)})
                .Where(a => a.ServiceTypes != null)
                .Iter(a =>
                {
                    foreach (var serviceType in a.ServiceTypes)
                        services.AddSingleton(serviceType, a.Type);
                });

            return services;
        }

        public static IServiceCollection AddSingletons<TService>(this IServiceCollection services, IEnumerable<Type> types)
        {
            types
                .Where(type => type.IsClassAssignableTo<TService>())
                .Iter(t => services.AddSingleton(typeof(TService), t));

            return services;
        }

        public static IEnumerable<ServiceDescriptor> GetServiceDescriptors(this Assembly assembly, Func<Type, Type[]> typeToServiceTypes, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            return assembly
                .GetDefinedTypesSafe()
                .Select(type => new {ImplementationType = type, ServiceTypes = typeToServiceTypes(type)})
                .Where(a => a.ServiceTypes != null)
                .SelectMany(a => a.ServiceTypes.Select(serviceType => new ServiceDescriptor(serviceType, a.ImplementationType, lifetime)));
        }

        public static IEnumerable<ServiceDescriptor> GetServiceDescriptors(this Assembly assembly, Func<Type, bool> typeFilter,
            Type serviceType, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            return assembly
                .GetDefinedTypesSafe()
                .Where(typeFilter)
                .Select(type => new ServiceDescriptor(serviceType, type, lifetime));
        }
    }
}
