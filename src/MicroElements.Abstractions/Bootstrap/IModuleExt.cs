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