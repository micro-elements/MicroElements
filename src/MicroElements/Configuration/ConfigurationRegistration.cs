using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroElements.Bootstrap.Extensions.Configuration
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
        /// <param name="profile">Профиль конфигурации.</param>
        /// <returns>ContainerBuilder для поддержки комбинирования вызовов.</returns>
        public static IServiceCollection RegisterConfigurationTypes(this IServiceCollection services, IConfigurationRoot configurationRoot, Type[] allTypes, string profile)
        {
            var configurationTypes = allTypes
                .Where(t => GetConfigurationSuffixes().Any(suffix => t.Name.EndsWith(suffix)))
                .ToList();
            configurationTypes.ForEach(type => RegisterConfigurationType(services, configurationRoot, type, profile));

            return services;
        }

        private static void RegisterConfigurationType(IServiceCollection services, IConfigurationRoot configurationRoot, Type optionType, string profile)
        {
            // Получаем конфигурационную секцию с нужным именем.
            IConfigurationSection configurationSection = GetConfigurationSection(configurationRoot, optionType);
            if (configurationSection == null)
                return;

            // Регистрация строготипизированного конфига.
            services.RegisterConfigurationType(configurationSection, optionType, profile);
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
            // Заменяем реализацию фабрики опций на свою.
            services.Add(ServiceDescriptor.Transient(typeof(IOptionsFactory<>), typeof(OptionsFactory<>)));

            // Добавляем поддержку IOptions, IOptionsSnapshot, IOptionsMonitor
            services.AddOptions();

            // Получим родителя, если есть
            var parent = configurationSection.GetValue<string>("${parent}", null);

            // Имя набора свойств (что-то типа профиля внутри конфигурации)
            var name = parent ?? profile ?? Options.DefaultName;

            // Здесь аналог вызова через Reflection: services.Configure<TOptions>(name, configurationSection);
            services.Configure(configurationSection, optionType, Options.DefaultName);
            if (name != Options.DefaultName)
                services.Configure(configurationSection, optionType, name);

            // Регистрация строготипизированных конфигов, чтобы можно было инжектить без IOptions и т.п.
            services.AddSingleton(optionType, cfg => GetOptionsSnapshotValue(cfg, optionType, name));

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

        private static object GetOptionsSnapshotValue(IServiceProvider cfg, Type configurationType, string name)
        {
            var optionsSnapshotType = typeof(IOptionsSnapshot<>).MakeGenericType(configurationType);
            var optionsSnapshot = cfg.GetService(optionsSnapshotType);
            var optionsSnapshotImplType = optionsSnapshot.GetType();
            var valueProperty = optionsSnapshotImplType.GetMethod(nameof(IOptionsSnapshot<object>.Get));
            var value = valueProperty.Invoke(optionsSnapshot, new object[] { name });
            return value;
        }

        private static IConfigurationSection GetConfigurationSection(IConfigurationRoot configurationRoot, Type configurationType)
        {
            var configurationTypeName = configurationType.Name;

            // Отрезаем общую часть и оставляем чистое имя.
            var suffixes = GetConfigurationSuffixes();
            var nameSuffix = suffixes.FirstOrDefault(suf => configurationTypeName.EndsWith(suf)) ?? string.Empty;
            var name = configurationTypeName.Substring(0, configurationTypeName.Length - nameSuffix.Length);

            Func<IConfigurationSection, bool> hasConfigurationValues = conf => conf.Value != null || conf.GetChildren().Any();

            // Проверяем различные варианты написания секции и расположения
            var configurationSection = configurationRoot.GetSection($"Configuration:{name}");
            if (hasConfigurationValues(configurationSection))
                return configurationSection;

            configurationSection = configurationRoot.GetSection($"Configuration:{name}{nameSuffix}");
            if (hasConfigurationValues(configurationSection))
                return configurationSection;

            configurationSection = configurationRoot.GetSection($"{name}{nameSuffix}");
            if (hasConfigurationValues(configurationSection))
                return configurationSection;

            return null;
        }
    }
}