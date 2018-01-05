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
