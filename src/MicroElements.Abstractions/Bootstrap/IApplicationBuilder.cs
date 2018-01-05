using System;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Bootstrap
{
    public interface IApplicationBuilder
    {
        /// <summary>
        /// Builds application services according <see cref="StartupConfiguration"/>.
        /// </summary>
        /// <param name="startupConfiguration">Startup configuration.</param>
        /// <returns>Build result.</returns>
        IBuildContext Build(StartupConfiguration startupConfiguration);

        /// <summary>
        /// Adds a delegate for configuring additional services for the host or web application. This may be called
        /// multiple times.
        /// </summary>
        /// <param name="configureServices">A delegate for configuring the <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.</param>
        /// <returns>The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" />.</returns>
        IApplicationBuilder ConfigureServices(Action<IServiceCollection> configureServices);
    }
}
