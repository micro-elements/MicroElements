using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using MicroComponents.DependencyInjection;
using Microsoft.Extensions.Logging;
using IModule = Autofac.Core.IModule;
using Module = Autofac.Module;

namespace MicroComponents.Autofac
{
    /// <summary>
    /// Расширения для <see cref="Autofac"/>.
    /// </summary>
    public static class AutofacExtensions
    {
        ///// <summary>
        ///// Регистрация строготипизированных конфигураций по конфигурации.
        ///// </summary>
        ///// <param name="builder">ContainerBuilder.</param>
        ///// <param name="configurationRoot">Конфигурация.</param>
        ///// <param name="allTypes">Список всех загруженных типов.</param>
        ///// <param name="profile">Профиль конфигурации.</param>
        ///// <returns>ContainerBuilder для поддержки комбинирования вызовов.</returns>
        //public static ContainerBuilder RegisterConfigurationTypes(this ContainerBuilder builder, IConfigurationRoot configurationRoot, Type[] allTypes, string profile)
        //{
        //    var configurationTypes = allTypes
        //        .Where(t => GetConfigurationSuffixes().Any(suffix => t.Name.EndsWith(suffix)))
        //        .ToList();
        //    configurationTypes.ForEach(type => RegisterConfigurationType(builder, configurationRoot, type, profile));

        //    return builder;
        //}

        //private static void RegisterConfigurationType(ContainerBuilder builder, IConfigurationRoot configurationRoot, Type optionType, string profile)
        //{
        //    // Получаем конфигурационную секцию с нужным именем.
        //    IConfigurationSection configurationSection = GetConfigurationSection(configurationRoot, optionType);
        //    if (configurationSection == null)
        //        return;

        //    IServiceCollection services = new ServiceCollection();

        //    // Регистрация строготипизированного конфига.
        //    services.RegisterConfigurationType(configurationSection, optionType, profile);

        //    builder.AddServices(services);
        //}

        /// <summary>
        /// Регистрация необходимых для логирования интерфейсов в контейнер.
        /// </summary>
        /// <param name="builder">ContainerBuilder.</param>
        /// <param name="loggerFactory">Фабрика логирования.</param>
        /// <returns>ContainerBuilder для поддержки комбинирования вызовов.</returns>
        public static ContainerBuilder RegisterLogging(this ContainerBuilder builder, ILoggerFactory loggerFactory)
        {
            builder.RegisterInstance(loggerFactory).As<ILoggerFactory>();
            builder.RegisterInstance(loggerFactory.CreateLogger("Default")).As<ILogger>();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            return builder;
        }

        /// <summary>
        /// Регистрация модулей.
        /// </summary>
        /// <param name="builder">services</param>
        /// <param name="moduleBuilder">moduleBuilder</param>
        /// <param name="assemblies">assemblies</param>
        /// <returns>ContainerBuilder для поддержки комбинирования вызовов.</returns>
        public static ContainerBuilder RegisterModules(this ContainerBuilder builder, ContainerBuilder moduleBuilder, params Assembly[] assemblies)
        {
            // Все типы модулей.
            var moduleTypes = assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(type => typeof(Module).IsAssignableFrom(type));

            // Зарегистрируем в промежуточный контейнер
            moduleTypes
                .ToList()
                .ForEach(moduleType => moduleBuilder.RegisterType(moduleType).As<IModule>());

            // Получим модули с внедренными значениями
            var container = moduleBuilder.Build();
            var modules = container.Resolve<IEnumerable<IModule>>().ToList();

            // Добавим модули в основной билдер.
            modules.ForEach(module => builder.RegisterModule(module));

            return builder;
        }

        /// <summary>
        /// Регистрация компонентов по атрибуту.
        /// </summary>
        /// <param name="builder">ContainerBuilder.</param>
        /// <param name="allTypes">Список всех загруженных типов.</param>
        /// <returns>ContainerBuilder для поддержки комбинирования вызовов.</returns>
        public static ContainerBuilder RegisterWithRegisterAttribute(this ContainerBuilder builder, Type[] allTypes)
        {
            allTypes
                .Where(type => type.GetCustomAttribute<RegisterAttribute>() != null)
                .ToList()
                .ForEach(componentType =>
                {
                    var registerAttribute = componentType.GetCustomAttribute<RegisterAttribute>();
                    var registrationBuilder = builder.RegisterType(componentType);

                    if (registerAttribute.Name != null)
                    {
                        var services = registerAttribute.Services;
                        if (services == null || services.Length != 1)
                            throw new SystemException("For named instances property Services in RegisterAttribute must be exactly one");

                        var serviceType = services.Single();
                        registrationBuilder = registrationBuilder.Named(registerAttribute.Name, serviceType);
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
                });

            return builder;
        }
    }
}