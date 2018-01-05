using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroElements.Bootstrap.Extensions
{
    /// <summary>
    /// Extension methods 
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Запуск всех <see cref="IStartable"/>.
        /// При запуске учитывается <see cref="IStartableMetadata"/> в котором есть порядок запуска.
        /// Сервисы группируются по Order и запускаются "волнами".
        /// </summary>
        /// <param name="serviceProvider">Провайдер сервисов.</param>
        /// <param name="logger">Логгер.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task StartRunnablesAsync(this IServiceProvider serviceProvider, ILogger logger)
        {
            //todo: Lazy metadata
            var allRunnables = serviceProvider.GetServices<Lazy<IStartable, IStartableMetadata>>().ToList();

            var runnablesByRunOrder = allRunnables.GroupBy(p => p.Metadata.StartOrder).OrderBy(group => group.Key);
            foreach (var runnables in runnablesByRunOrder)
            {
                logger.LogInformation("Starting runnables: StartOrder={0}, Count={1}", runnables.Key, runnables.Count());
                runnables
                    .Select(lazy => lazy.Value)
                    .Select(runnable => runnable.GetType().Name)
                    .ToList()
                    .ForEach(name => logger.LogInformation("----Starting {0}", name));

                var currentRunnables = runnables.Select(p => p.Value.StartAsync());
                await Task.WhenAll(currentRunnables).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Остановить все сервисы.
        /// </summary>
        /// <param name="serviceProvider">Провайдер сервисов.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task StopAllServices(this IServiceProvider serviceProvider)
        {
            var stoppablesWithRunnable = serviceProvider.GetServices<Lazy<IStoppable, IStartableMetadata>>().ToList();
            var stoppablesByRunOrder = stoppablesWithRunnable.GroupBy(p => p.Metadata.StartOrder).OrderByDescending(group => group.Key);
            var logger = serviceProvider.GetService<ILogger>();

            foreach (var stoppables in stoppablesByRunOrder)
            {
                logger.LogInformation("Stopping runnables: StopOrder={0}, Count={1}", stoppables.Key, stoppables.Count());
                stoppables
                    .Select(lazy => lazy.Value)
                    .Select(stoppable => stoppable.GetType().Name)
                    .ToList()
                    .ForEach(name => logger.LogInformation("Stopping {0}", name));

                var currentStoppables = stoppables.Select(p => p.Value.StopAsync());
                await Task.WhenAll(currentStoppables).ConfigureAwait(false);
            }

            // Диспозим то, что нужно задиспозить.
            foreach (var disposable in serviceProvider.GetServices<IDisposable>())
                disposable.Dispose();
        }
    }
}