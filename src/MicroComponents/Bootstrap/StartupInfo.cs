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

        /// <summary>
        /// Директория из которой запущено приложение.
        /// </summary>
        public string StartupDir;

        /// <summary>
        /// Путь к запускаемому бинарнику.
        /// </summary>
        public string StartupApp;
    }
}