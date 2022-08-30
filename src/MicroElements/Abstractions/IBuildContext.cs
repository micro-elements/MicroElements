// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroElements.Bootstrap
{
    /// <summary>
    /// Build context.
    /// </summary>
    public interface IBuildContext : IServiceProvider
    {
        /// <summary>
        /// ServiceCollection. It can be passed from <see cref="StartupConfiguration"/> or created inside build process.
        /// </summary>
        IServiceCollection ServiceCollection { get; }

        /// <summary>
        /// Service provider builded on the end of build process.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Root of loaded configuration.
        /// </summary>
        IConfigurationRoot ConfigurationRoot { get; }

        /// <summary>
        /// Loaded assemblies.
        /// </summary>
        Assembly[] Assemblies { get; }

        /// <summary>
        /// All exported types from loaded assemblies.
        /// </summary>
        Type[] ExportedTypes { get; }

        /// <summary>
        /// Фабрика логирования.
        /// </summary>
        ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Основной логгер для процесса инициализации.
        /// </summary>
        ILogger Logger { get; }
    }
}
