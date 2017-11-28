using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroComponents.Bootstrap
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
    }
}