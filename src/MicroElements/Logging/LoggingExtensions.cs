using System;
using System.IO;
using MicroElements.Bootstrap;
using MicroElements.Bootstrap.Extensions;

namespace MicroElements.Logging
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
            var fullLogsPath = Path.IsPathRooted(configuration.LogsPath)
                ? configuration.LogsPath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration.LogsPath);

            if (!Directory.Exists(fullLogsPath))
                Directory.CreateDirectory(fullLogsPath);

            var lockFileManager = new LockFileManager(fullLogsPath, configuration.Profile);
            if (configuration.InstanceId != null)
            {
                if (lockFileManager.CheckInstanceId(configuration.InstanceId))
                {
                    lockFileManager.CreateAndLockPidFile(configuration.InstanceId);
                }
                else
                {
                    throw new Exception($"Приложение с InstanceId = {configuration.InstanceId} уже запущено!");
                }
            }
            else
            {
                lockFileManager.CreateAndLockPidFile();
            }

            var flatProfileName = configuration.Profile?.CleanFileName().Replace(Path.DirectorySeparatorChar, '_');
            // todo: задокументировать
            // todo: использовать для автоматической конфигурации общего сбора логов
            Environment.SetEnvironmentVariable("LogsPath", fullLogsPath, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ProfileName", flatProfileName, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("CurrentInstanceId", configuration.InstanceId, EnvironmentVariableTarget.Process);

            return lockFileManager;
        }
    }
}
