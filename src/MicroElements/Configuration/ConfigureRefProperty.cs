// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroElements.Configuration
{
    public class ConfigureRefProperty<T> : IConfigureNamedOptions<T> where T : class
    {
        private IServiceProvider _serviceProvider;
        private string _optionPropertyName;
        private Type _refObjectType;
        private string _refObjectName;

        /// <inheritdoc />
        public ConfigureRefProperty(IServiceProvider serviceProvider, string optionPropertyName, Type refObjectType, string refObjectName)
        {
            _serviceProvider = serviceProvider;
            _optionPropertyName = optionPropertyName;
            _refObjectType = refObjectType;
            _refObjectName = refObjectName;
        }

        public void Configure(T options)
        {
            Configure(Options.DefaultName, options);
        }

        /// <inheritdoc />
        public void Configure(string name, T options)
        {
            var services = _serviceProvider.GetServices(_refObjectType);
            var servByName = services.FirstOrDefault(service => Equals(service.GetType().GetProperty("Name").GetValue(service), _refObjectName));

            if (servByName != null)
            {
                options.GetType().GetProperty(_optionPropertyName).SetValue(options, servByName);
            }
        }
    }
}
