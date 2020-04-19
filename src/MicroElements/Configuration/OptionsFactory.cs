// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroElements.Configuration
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
        private readonly IServiceProvider _serviceProvider;
        private readonly IKeyedServiceCollection<string, IDefaultValueProvider<TOptions>> _defaultValueProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsFactory{TOptions}"/> class.
        /// </summary>
        /// <param name="setups">setups.</param>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> to create default values for options if needed.</param>
        /// <param name="defaultValueProviders">Провайдер значения по-умолчанию для типа.</param>
        public OptionsFactory(
            IEnumerable<IConfigureOptions<TOptions>> setups,
            IServiceProvider serviceProvider,
            IKeyedServiceCollection<string, IDefaultValueProvider<TOptions>> defaultValueProviders = null)
        {
            _setups = setups;
            _serviceProvider = serviceProvider;
            _defaultValueProviders = defaultValueProviders;
        }

        /// <inheritdoc />
        public TOptions Create(string name)
        {
            TOptions instance;
            if (_serviceProvider != null && _defaultValueProviders != null)
            {
                IDefaultValueProvider<TOptions> defaultValueProvider = _defaultValueProviders.GetService(_serviceProvider, name);
                instance = defaultValueProvider != null ? defaultValueProvider.GetDefault() : ActivatorUtilities.CreateInstance<TOptions>(_serviceProvider);
            }
            else
            {
                instance = Activator.CreateInstance<TOptions>();
            }

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
