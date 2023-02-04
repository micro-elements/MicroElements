// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Bootstrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MicroElements.Configuration
{
    /// <summary>
    /// Методы для регистриции конфигурации.
    /// </summary>
    public static class ConfigurationRegistration
    {
        private static string[] GetConfigurationSuffixes() => new[] { "Configuration", "Options" };

        /// <summary>
        /// Регистрация строготипизированных конфигураций по конфигурации.
        /// </summary>
        /// <param name="services">IServiceCollection.</param>
        /// <param name="configurationRoot">Конфигурация.</param>
        /// <param name="allTypes">Список всех загруженных типов.</param>
        /// <param name="startupConfiguration">StartupConfiguration.</param>
        /// <returns>IServiceCollection для поддержки комбинирования вызовов.</returns>
        public static IServiceCollection RegisterConfigurationTypes(this IServiceCollection services, IConfigurationRoot configurationRoot, Type[] allTypes, StartupConfiguration startupConfiguration)
        {
            var configurationTypes = GetConfigurationTypes(allTypes, startupConfiguration);
            configurationTypes.ForEach(type => RegisterConfigurationType(services, configurationRoot, type, startupConfiguration.Profile));

            return services;
        }

        public static List<Type> GetConfigurationTypes(Type[] allTypes, StartupConfiguration startupConfiguration)
        {
            var configurationTypes = allTypes
                .Where(t => GetConfigurationSuffixes().Any(suffix => t.Name.EndsWith(suffix)))
                .Where(type => !type.IsAbstract)
                .Concat(startupConfiguration.ConfigurationTypes ?? Array.Empty<Type>())
                .ToList();
            return configurationTypes;
        }

        private static void RegisterConfigurationType(IServiceCollection services, IConfigurationRoot configurationRoot, Type optionType, string profile)
        {
            // Получаем конфигурационную секцию с нужным именем.
            IConfigurationSection[] configurationSections = GetConfigurationSection(configurationRoot, optionType);
            if (configurationSections != null)
            {
                foreach (var configurationSection in configurationSections)
                {
                    // Регистрация строготипизированного конфига.
                    services.RegisterConfigurationType(configurationSection, optionType, profile);
                }
            }
        }

        /// <summary>
        /// Регистрация строготипизированного конфига.
        /// </summary>
        /// <remarks>
        /// IOptionsSnapshot не работает для перезагрузки значений на лету, хотя по документации должен.
        /// Причина в том, что IOptionsSnapshot нужно получать в режиме Scoped (один раз на запрос или контекст)
        /// Для отслеживания изменения конфигурации нужно использовать IOptionsMonitor
        /// </remarks>
        /// <param name="services">Экземпляр IServiceCollection.</param>
        /// <param name="configurationSection">Конфигурационная секция.</param>
        /// <param name="optionType">Тип конфигурационного объекта.</param>
        /// <param name="profile">Профиль конфигурации.</param>
        /// <returns>IServiceCollection.</returns>
        private static IServiceCollection RegisterConfigurationType(this IServiceCollection services, IConfigurationSection configurationSection, Type optionType, string profile)
        {
            // Получим родителя, если есть
            var parent = configurationSection.GetValue<string>("${parent}", null);

            // Имя набора свойств (что-то типа профиля внутри конфигурации)
            var name = parent ?? profile ?? Options.DefaultName;
            if (configurationSection.Path.StartsWith("$objects"))
            {
                name = configurationSection.Key;
            }

            // Здесь аналог вызова через Reflection: services.Configure<TOptions>(name, configurationSection);
            services.Configure(configurationSection, optionType, Options.DefaultName);
            if (name != Options.DefaultName)
                services.Configure(configurationSection, optionType, name);

            // Регистрация строготипизированных конфигов, чтобы можно было инжектить без IOptions и т.п.
            services.AddSingleton(optionType, provider => GetOptionsSnapshotValue(provider, optionType, name));

            var interfaces = optionType.GetInterfaces();
            if (interfaces?.Length > 0)
            {
                foreach (var configurationInterface in interfaces)
                {
                    services.AddSingleton(configurationInterface, provider => GetOptionsSnapshotValue(provider, optionType, name));
                }
            }

            return services;
        }

        /// <summary>
        /// Non generic version of: services.Configure{TOptions}(name, configurationSection);
        /// </summary>
        /// <param name="services">IServiceCollection.</param>
        /// <param name="configurationSection">Конфигурационная секция.</param>
        /// <param name="configurationType">Тип конфигурационного объекта.</param>
        /// <param name="name">Имя набора опций.</param>
        private static void Configure(this IServiceCollection services, IConfigurationSection configurationSection, Type configurationType, string name)
        {
            var configureMethodInfo = typeof(OptionsConfigurationServiceCollectionExtensions).GetMethod(
                nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
                new[] { typeof(IServiceCollection), typeof(string), typeof(IConfiguration) });
            configureMethodInfo = configureMethodInfo.MakeGenericMethod(configurationType);
            configureMethodInfo.Invoke(null, new object[] { services, name, configurationSection });
        }

        private static object GetOptionsSnapshotValue(IServiceProvider provider, Type configurationType, string name)
        {
            // todo: expression
            var optionsSnapshotType = typeof(IOptionsSnapshot<>).MakeGenericType(configurationType);
            var optionsSnapshot = provider.GetService(optionsSnapshotType);
            var optionsSnapshotImplType = optionsSnapshot.GetType();
            var valueProperty = optionsSnapshotImplType.GetMethod(nameof(IOptionsSnapshot<object>.Get));
            var value = valueProperty.Invoke(optionsSnapshot, new object[] { name });
            return value;
        }

        private static IConfigurationSection[] GetConfigurationSection(IConfigurationRoot configurationRoot, Type configurationType)
        {
            var configurationTypeName = configurationType.Name;

            // Отрезаем общую часть и оставляем чистое имя.
            var suffixes = GetConfigurationSuffixes();
            var nameSuffix = suffixes.FirstOrDefault(suf => configurationTypeName.EndsWith(suf)) ?? string.Empty;
            var name = configurationTypeName.Substring(0, configurationTypeName.Length - nameSuffix.Length);

            // Проверяем различные варианты написания секции и расположения
            IConfigurationSection configurationSection;

            configurationSection = configurationRoot.GetSection($"{name}{nameSuffix}");
            if (HasConfigurationValues(configurationSection))
                return new[] { configurationSection };

            configurationSection = configurationRoot.GetSection($"{name}");
            if (HasConfigurationValues(configurationSection))
                return new[] { configurationSection };

            configurationSection = configurationRoot.GetSection($"Configuration:{name}{nameSuffix}");
            if (HasConfigurationValues(configurationSection))
                return new[] { configurationSection };

            configurationSection = configurationRoot.GetSection($"Configuration:{name}");
            if (HasConfigurationValues(configurationSection))
                return new[] { configurationSection };

            return null;
        }

        private static bool HasConfigurationValues(IConfigurationSection section)
        {
            var children = section.GetChildren();
            return section.Value != null || children.Any();
        }
    }
}
