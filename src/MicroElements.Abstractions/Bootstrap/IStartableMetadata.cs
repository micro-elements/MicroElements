// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace MicroElements.Bootstrap
{
    /// <summary>
    /// Metadata for startable component.
    /// </summary>
    public interface IStartableMetadata
    {
        /// <summary>
        /// Order of start. Can be used when you need deterministic ordered start.
        /// </summary>
        [DefaultValue(0)]
        int StartOrder { get; }
    }
}