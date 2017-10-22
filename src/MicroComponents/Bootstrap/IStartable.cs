using System.Threading.Tasks;

namespace MicroComponents.Bootstrap
{
    /// <summary>
    /// Startable service. Defines contract for start service.
    /// </summary>
    public interface IStartable
    {
        /// <summary>
        /// Start service.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task StartAsync();
    }
}