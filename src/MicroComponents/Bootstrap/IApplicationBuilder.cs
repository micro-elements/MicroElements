namespace MicroComponents.Bootstrap
{
    public interface IApplicationBuilder
    {
        IBuildContext Build(StartupConfiguration startupConfiguration);
    }
}