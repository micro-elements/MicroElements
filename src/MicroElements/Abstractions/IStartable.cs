// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace MicroElements.Bootstrap
{
    /// <summary>
    /// Startable service. Defines contract for start service.
    /// </summary>
    public interface IStartable
    {
        /// <summary>
        /// Starts service.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}