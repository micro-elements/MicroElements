using System;
using System.IO;

namespace MicroComponents.Bootstrap.Extensions.Logging
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Задание переменных среды для использования в NLog.config, создание и блокировка pid-файла
        /// </summary>
        /// <param name="configuration">Параметры запуска.</param>
        /// <returns>Экземпляр менеджера pid-файлов. Он необходим для удаления файла при завершении работы.</returns>
        public static IStoppable SetupLogsPath(StartupConfiguration configuration)
        {
            var logsPath = Path.IsPathRooted(configuration.LogsPath)
                ? configuration.LogsPath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            if (!Directory.Exists(logsPath))
                Directory.CreateDirectory(logsPath);

            var pidFileManager = new PidFileManager(logsPath, configuration.Profile);
            if (configuration.InstanceId != null)
            {
                if (pidFileManager.CheckInstanceId(configuration.InstanceId))
                {
                    pidFileManager.CreateAndLockPidFile(configuration.InstanceId);
                }
                else
                {
                    throw new Exception($"Приложение с InstanceId = {configuration.InstanceId} уже запущено!");//todo: почему-то не хочет кидать ConfigurationErrorsException
                }
            }
            else
            {
                pidFileManager.CreateAndLockPidFile();
            }

            var optionsProfile = configuration.Profile;
            Environment.SetEnvironmentVariable("LogsPath", logsPath, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ProfileName", optionsProfile?.CleanFileName().Replace(Path.DirectorySeparatorChar, '_'), EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("CurrentInstanceId", pidFileManager.CurrentInstanceId, EnvironmentVariableTarget.Process);

            return pidFileManager;
        }
    }
}
