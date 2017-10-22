using System;
using Microsoft.Extensions.DependencyInjection;

namespace MicroComponents.Bootstrap
{
    public interface IApplicationBuilder
    {
        IServiceCollection Configure(StartupConfiguration startupConfiguration);
        IServiceProvider Build(StartupConfiguration startupConfiguration);
    }
}