using System;
using System.Collections.Generic;
using MicroComponents.Configuration;
using MicroComponents.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroComponents.Bootstrap.Extensions.Configuration
{
    /// <summary>
    /// Фабрика опций с поддержкой <see cref="IDefaultValueProvider{T}"/>
    /// Провайдеры значений по умолчанию регистрируются по имени.
    /// </summary>
    /// <typeparam name="TOptions">TOptions.</typeparam>
    public class OptionsFactory<TOptions> : IOptionsFactory<TOptions>
        where TOptions : class, new()
    {
        private readonly IEnumerable<IConfigureOptions<TOptions>> _setups;
        private readonly IKeyedServiceCollection<string, IDefaultValueProvider<TOptions>> _defaultValueProviders;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="setups">setups.</param>
        /// <param name="defaultValueProviders">Провайдер значения по-умолчанию для типа.</param>
        public OptionsFactory(
            IEnumerable<IConfigureOptions<TOptions>> setups,
            IKeyedServiceCollection<string, IDefaultValueProvider<TOptions>> defaultValueProviders = null)
        {
            _setups = setups;
            _defaultValueProviders = defaultValueProviders;
        }

        /// <inheritdoc />
        public TOptions Create(string name)
        {
            IDefaultValueProvider<TOptions> defaultValueProvider = null; //_defaultValueProviders.GetService(null, name);//todo: IDefaultValueProvider
            TOptions instance = defaultValueProvider != null ? defaultValueProvider.GetDefault() : Activator.CreateInstance<TOptions>();
            foreach (IConfigureOptions<TOptions> setup in _setups)
            {
                IConfigureNamedOptions<TOptions> configureNamedOptions;
                if ((configureNamedOptions = setup as IConfigureNamedOptions<TOptions>) != null)
                    configureNamedOptions.Configure(name, instance);
                else
                    setup.Configure(instance);
            }

            return instance;
        }
    }
}