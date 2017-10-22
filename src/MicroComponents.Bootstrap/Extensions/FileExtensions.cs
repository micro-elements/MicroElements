using System;
using System.IO;
using System.Linq;

namespace MicroComponents.Bootstrap.Extensions
{
    /// <summary>
    /// Расширения для работы с файлами и путями.
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Очистка файлового имени. Заменяет невалидные символы на заданный.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="replaceSymbol">Символ замены для невалидных символов.</param>
        /// <returns>Валидное имя файла.</returns>
        public static string CleanFileName(this string fileName, char replaceSymbol = '_')
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (Path.GetInvalidFileNameChars().Contains(replaceSymbol))
                throw new ArgumentException($"replaceSymbol '{replaceSymbol}' is invalid file name char", nameof(replaceSymbol));

            return string.Join(replaceSymbol.ToString(), fileName.Split(Path.GetInvalidFileNameChars()));
        }

        /// <summary>
        /// Нормализация слешей в файловом пути.
        /// </summary>
        /// <param name="path">Файловый путь.</param>
        /// <returns>Нормализованный путь.</returns>
        public static string PathNormalize(this string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Добавление слеша в конец имени, если его там нет.
        /// </summary>
        /// <param name="path">Файловый путь.</param>
        /// <returns>Путь со слешем в конце.</returns>
        public static string AppendSlashInPath(this string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            return path.EndsWith(@"\") || path.EndsWith(@"/") ? path : path + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Получение относительного пути по имени файла и директории относительно которой вычисляется относительный путь.
        /// </summary>
        /// <param name="fileName">Имя файла. Может быть абсолютныи или относительным.</param>
        /// <param name="basePath">Базовый путь, относительно которого вычисляется относительный путь.</param>
        /// <returns>Относительный путь.</returns>
        public static string RelativeTo(this string fileName, string basePath)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (basePath == null)
                throw new ArgumentNullException(nameof(basePath));

            return Path.GetFullPath(fileName.PathNormalize()).Replace(Path.GetFullPath(basePath.PathNormalize().AppendSlashInPath()), string.Empty);
        }
    }
}