// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Configuration.Evaluation
{
    public class ResolvableConfigurationSource<T> : IConfigurationSource where T : IConfigurationSource
    {
        private readonly IServiceProvider _serviceProvider;

        public ResolvableConfigurationSource(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var service = _serviceProvider.GetService<T>();
            return service.Build(builder);
        }
    }
}
