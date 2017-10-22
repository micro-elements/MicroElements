using System.Threading.Tasks;

namespace MicroComponents.Bootstrap
{
    /// <summary>
    /// Stoppable service. Defines contract for service stop.
    /// </summary>
    public interface IStoppable
    {
        /// <summary>
        /// Stop service.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task StopAsync();
    }
}
