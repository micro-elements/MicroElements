using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton<ILogger>(loggerFactory.CreateLogger("Default"));
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

        /// <summary>
        /// Регистрация модулей.
        /// </summary>
        /// <param name="services">services</param>
        /// <param name="moduleServices">moduleServices</param>
        /// <param name="assemblies">assemblies</param>
        /// <returns>IServiceCollection для поддержки комбинирования вызовов.</returns>
        public static IServiceCollection RegisterModules(this IServiceCollection services, IServiceCollection moduleServices, params Assembly[] assemblies)
        {
            var moduleTypes = GetModuleTypes(assemblies);

            if (moduleTypes.Count > 0)
            {
                // Зарегистрируем в промежуточный контейнер
                moduleTypes.ForEach(moduleType => moduleServices.RegisterType(moduleType).As<IModule>().Add());

                // Получим модули с внедренными значениями
                var moduleServiceProvider = moduleServices.BuildServiceProvider();
                var modules = moduleServiceProvider.GetServices<IModule>().ToList();

                // Добавим модули в основной билдер.
                modules.ForEach(module => module.ConfigureServices(services));
            }

            return services;
        }

        public static List<Type> GetModuleTypes(this Assembly[] assemblies)
        {
            return assemblies.GetClassTypesAssignableTo<IModule>().ToList();
        }

        public static IEnumerable<Type> GetClassTypesAssignableTo<T>(this IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetDefinedTypesSafe())
                .GetClassTypesAssignableTo<T>();
        }

        public static IEnumerable<Type> GetClassTypesAssignableTo<T>(this IEnumerable<Type> types)
        {
            return types.Where(type => type.IsClassAssignableTo<T>());
        }

        public static List<IModule> ResolveModules(this IServiceCollection moduleServices, List<Type> moduleTypes)
        {
            // Зарегистрируем в промежуточный контейнер
            moduleTypes.ForEach(moduleType => moduleServices.RegisterType(moduleType).As<IModule>().Add());

            // Получим модули с внедренными значениями
            var moduleServiceProvider = moduleServices.BuildServiceProvider();
            var modules = moduleServiceProvider.GetServices<IModule>().ToList();

            return modules;
        }

        public static IServiceCollection RegisterModules(this IServiceCollection services, List<Type> moduleTypes)
        {
            if (moduleTypes.Count > 0)
            {
                // Copy of service collection
                IServiceCollection moduleServices = services.Copy();

                // Зарегистрируем в промежуточный контейнер
                //moduleTypes.ForEach(moduleType => moduleServices.RegisterType(moduleType).As<IModule>().Add());
                moduleTypes.ForEach(moduleType => moduleServices.RegisterType(moduleType).AsSelf().Add());

                // Получим модули с внедренными значениями
                var moduleServiceProvider = moduleServices.BuildServiceProvider();

                foreach (var moduleType in moduleTypes)
                {
                    var module = moduleServiceProvider.GetService(moduleType) as IModule;

                    //todo: можно сделать duck typing по наличию метода ConfigureServices
                    module.ConfigureServices(services);
                }

                ////todo: можно сделать duck typing по наличию метода ConfigureServices
                //var modules = moduleServiceProvider.GetServices<IModule>().ToList();

                //// Добавим модули в основной билдер.
                //modules.ForEach(module => module.ConfigureServices(services));
            }

            return services;
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