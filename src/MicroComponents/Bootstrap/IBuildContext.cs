using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MicroComponents.Bootstrap
{
    /// <summary>
    /// Build context.
    /// </summary>
    public interface IBuildContext
    {
        /// <summary>
        /// ServiceCollection. It can be passed from <see cref="StartupConfiguration"/> or created inside build process.
        /// </summary>
        IServiceCollection ServiceCollection { get; }

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