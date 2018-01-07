// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace MicroElements.Bootstrap
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
