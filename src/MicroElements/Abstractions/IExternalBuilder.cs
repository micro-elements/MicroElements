// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Bootstrap
{
    public interface IExternalBuilder
    {
        void AddServices(IEnumerable<ServiceDescriptor> descriptors);
        IServiceProvider ConfigureServices(IBuildContext buildContext);
    }
}