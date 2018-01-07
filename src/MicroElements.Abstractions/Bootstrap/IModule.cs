// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Bootstrap
{
    /// <summary>
    /// Registration module.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Configure services.
        /// </summary>
        /// <param name="services">Service collection.</param>
        void ConfigureServices(IServiceCollection services);
    }
}
