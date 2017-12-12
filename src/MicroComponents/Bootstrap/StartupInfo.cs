namespace MicroComponents.Bootstrap
{
    /// <summary>
    /// Информация об окружении.
    /// </summary>
    public class StartupInfo
    {
        /// <summary>
        /// Версия приложения.
        /// </summary>
        public string Version;

        /// <summary>
        /// Текущая директория.
        /// </summary>
        public string CurrentDirectory;

        /// <summary>Gets the pathname of the base directory that the assembly resolver uses to probe for assemblies.</summary>
        /// <returns>the pathname of the base directory that the assembly resolver uses to probe for assemblies.</returns>
        public string BaseDirectory;

        /// <summary>
        /// Путь к запускаемому бинарнику.
        /// </summary>
        public string StartupApp;
    }
}
