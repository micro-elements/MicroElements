// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.DependencyInjection;
using MicroElements.Reflection;
using MicroElements.Reflection.TypeExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace MicroElements.Bootstrap.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрация необходимых для логирования интерфейсов в контейнер.
        /// </summary>
        /// <param name="services">ContainerBuilder.</param>
        /// <param name="loggerFactory">Фабрика логирования.</param>
        /// <returns>ContainerBuilder для поддержки комбинирования вызовов.</returns>
        public static IServiceCollection RegisterLogging(this IServiceCollection services, ILoggerFactory loggerFactory)
        {
            services.TryAddSingleton<ILoggerFactory>(loggerFactory);
            services.TryAddSingleton<ILogger>(loggerFactory.CreateLogger("Default"));
            services.AddLogging();

            return services;
        }

        /// <summary>
        /// Регистрация компонентов по атрибуту.
        /// </summary>
        /// <param name="builder">ContainerBuilder.</param>
        /// <param name="allTypes">Список всех загруженных типов.</param>
        /// <returns>ContainerBuilder для поддержки комбинирования вызовов.</returns>
        public static IServiceCollection RegisterWithRegisterAttribute(this IServiceCollection builder, Type[] allTypes)
        {
            allTypes
                .Where(type => type.GetCustomAttribute<RegisterAttribute>() != null)
                .ToList()
                .ForEach(componentType =>
                {
                    var registerAttribute = componentType.GetCustomAttribute<RegisterAttribute>();
                    var registrationBuilder = builder.RegisterType(componentType);//todo: freezable decorator //todo: IConfiguration

                    if (registerAttribute.Name != null)
                    {
                        var services = registerAttribute.Services;
                        if (services == null || services.Length != 1)
                            throw new SystemException("For named instances property Services in RegisterAttribute must be exactly one");

                        registrationBuilder = registrationBuilder.As(services);
                        registrationBuilder = registrationBuilder.Named(registerAttribute.Name);
                    }
                    else
                    {
                        if (registerAttribute.Services != null)
                            registrationBuilder = registrationBuilder.As(registerAttribute.Services);
                        else
                            registrationBuilder = registrationBuilder.AsImplementedInterfaces();
                    }

                    if (registerAttribute.MetadataName != null && registerAttribute.MetadataValue != null)
                        registrationBuilder = registrationBuilder.WithMetadata(registerAttribute.MetadataName, registerAttribute.MetadataValue);

                    if (registerAttribute.Singleton)
                    {
                        registrationBuilder.SingleInstance();
                    }

                    registrationBuilder.TryAdd();
                });

            return builder;
        }

        public static IEnumerable<Type> GetClassTypesAssignableTo<T>(this IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetDefinedTypesSafe())
                .GetClassTypesAssignableTo<T>();
        }

        public static IEnumerable<Type> GetClassTypesAssignableTo<T>(this IEnumerable<Type> types)
        {
            return types.Where(type => type.IsConcreteAndAssignableTo<T>());
        }

        /// <summary>
        /// Makes a copy of <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">Input <see cref="IServiceCollection"/></param>
        /// <returns>Copy of input <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection Copy(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            IServiceCollection serviceCollectionCopy = new ServiceCollection();
            foreach (var serviceDescriptor in services)
            {
                serviceCollectionCopy.Add(serviceDescriptor);
            }

            return serviceCollectionCopy;
        }
    }
}
