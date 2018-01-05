using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroElements.Bootstrap.Extensions.Logging
{
    /// <summary>
    /// Создание и блокировка файла с pid для логирования.
    /// </summary>
    public class LockFileManager : IStoppable
    {
        private const int MaxFailedAttempts = 10;
        private readonly string _directory;
        private readonly string _profileName;
        private string _currentPidFile;
        private FileStream _lock;
        private int _failedAttempts;

        /// <summary>
        /// Возвращает № текущего запуска соответствующей конфигурации.
        /// </summary>
        public string CurrentInstanceId { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logDirectory">Путь к папке логов</param>
        /// <param name="profileName">Имя профиля</param>
        public LockFileManager(string logDirectory, string profileName)
        {
            _directory = logDirectory;
            _profileName = profileName?.CleanFileName();

            DeletePidFiles();
        }

        /// <summary>
        /// Проверяет наличие уже запущенного процесса с заданным идентификатором.
        /// </summary>
        /// <param name="instanceId">Произвольно заданный идентификатор.</param>
        /// <returns>Возвращает true, если файла с заданным идентификатором нет.</returns>
        public bool CheckInstanceId(string instanceId)
        {
            return !File.Exists(GetLockFileName(instanceId));
        }

        /// <summary>
        /// Создает и блокирует файл
        /// </summary>
        public void CreateAndLockPidFile()
        {
            CreateAndLockPidFile(GetNextInstanceId().ToString());
        }

        /// <summary>
        /// Создает и блокирует файл
        /// </summary>
        /// <param name="instanceId">Заданный пользователем instance id</param>
        public void CreateAndLockPidFile(string instanceId)
        {
            CurrentInstanceId = instanceId;
            _currentPidFile = GetLockFileName(instanceId);

            var succeed = false;

            while (!succeed)
            {
                try
                {
                    _lock = new FileStream(_currentPidFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                    succeed = true;
                }
                catch (IOException)
                {
                    // превышено максимальное количество попыток
                    if (++_failedAttempts >= MaxFailedAttempts)
                        throw;
                    instanceId = GetNextInstanceId().ToString();
                    _currentPidFile = GetLockFileName(instanceId);
                }
            }

            CurrentInstanceId = instanceId;
            var info = new UTF8Encoding(true).GetBytes(Process.GetCurrentProcess().Id.ToString());
            _lock.Write(info, 0, info.Length);
            _lock.Flush();
        }

        private string GetLockFileName(string instanceId)
        {
            return Path.Combine(_directory, $"{_profileName}_{instanceId}.lock");
        }

        /// <summary>
        /// Разблокировать и удалить файл
        /// </summary>
        /// <returns>A <see cref="Task"/>Возвращает асинхронную операцию.</returns>
        public async Task StopAsync()
        {
            _lock?.Dispose();

            var fi = new FileInfo(_currentPidFile);
            if (!IsFileLocked(fi))
                await Task.Run(() => fi.Delete()).ConfigureAwait(false);
        }

        #region Internal methods

        /// <summary>
        /// Проверяет, заблокирован ли файл
        /// </summary>
        /// <param name="file">Проверяемый файл</param>
        /// <returns>true если заблокирован, false в противном случае</returns>
        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
            }

            return false;
        }

        /// <summary>
        /// Возвращает первый свободный runNumber
        /// </summary>
        /// <returns>Первый свободный номер runNumber по порядку с 0.</returns>
        private int GetNextInstanceId()
        {
            var existingIds = new List<int>();

            var files = Directory.GetFiles(_directory, $"*{_profileName}*.lock");
            foreach (var file in files)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                if (fileNameWithoutExtension != null)
                {
                    fileNameWithoutExtension = fileNameWithoutExtension.Replace(_profileName + "_", string.Empty);
                    if (int.TryParse(fileNameWithoutExtension, out int fileInstanceId))
                    {
                        existingIds.Add(fileInstanceId);
                    }
                }
            }

            if (existingIds.Count == 0)
                return 0;

            existingIds.Sort();
            var lastNumber = existingIds.Last();

            var range = Enumerable.Range(0, lastNumber).Except(existingIds).ToArray();

            if (range.Length > 0)
                return range[0];

            return lastNumber + 1;
        }

        /// <summary>
        /// Попытка удалить PID-файлы, если они не заблокированы другими экземплярами.
        /// </summary>
        private void DeletePidFiles()
        {
            var files = Directory.GetFiles(_directory, "*" + _profileName + "*.lock");

            foreach (var file in files)
            {
                if (IsFileLocked(new FileInfo(file)))
                    continue;

                int currentRetry = 0;
                for (; ; )
                {
                    try
                    {
                        File.Delete(file);
                        break;
                    }
                    catch (Exception ex)
                    {
                        currentRetry++;

                        if (currentRetry > MaxFailedAttempts)
                        {
                            //InternalLogger.Error(ex, $"Не удалось удалить PID-файл {file}"); //todo: какой нафиг NLog?
                            break;
                        }
                    }
                }
            }
        }

        #endregion Internal methods
    }
}