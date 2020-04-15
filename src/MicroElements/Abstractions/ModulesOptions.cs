// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Bootstrap
{
    public class ModulesOptions
    {
        /// <summary>
        /// Автоматический поиск модулей среди загруженных типов.
        /// </summary>
        public bool AutoDiscoverModules { get; set; } = false;

        /// <summary>
        /// Типы модулей, заданные вручную.
        /// </summary>
        public Type[] ModuleTypes { get; set; } = new Type[0];
    }
}
