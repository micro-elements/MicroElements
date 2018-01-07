// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Bootstrap
{
    /// <summary>
    /// Extended module. Has access to <see cref="IBuildContext"/>.
    /// </summary>
    public interface IModuleExt
    {
        /// <summary>
        /// Configure services.
        /// </summary>
        /// <param name="buildContext">Service collection.</param>
        void ConfigureServices(IBuildContext buildContext);
    }
}