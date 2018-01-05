using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MicroElements.Bootstrap
{
    /// <summary>
    /// Утилиты для облегчения работы с Reflection.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Загрузка сборок в память по маске.
        /// </summary>
        /// <param name="scanDirectory">Директория из которой нужно грузить сборки.</param>
        /// <param name="assemblyScanPatterns">Маска поиска файлов.</param>
        /// <returns>Список найденных сборок.</returns>
        public static IEnumerable<Assembly> LoadAssemblies(string scanDirectory, params string[] assemblyScanPatterns)
        {
            string WildcardToRegex(string pat) => "^" + Regex.Escape(pat).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            bool FileNameMatchesPattern(string filename, string pattern) => Regex.IsMatch(Path.GetFileName(filename), WildcardToRegex(pattern));

            var assemblies = Directory.EnumerateFiles(scanDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                .Concat(Directory.EnumerateFiles(scanDirectory, "*.exe", SearchOption.TopDirectoryOnly))
                .Where(filename => assemblyScanPatterns.Any(pattern => FileNameMatchesPattern(filename, pattern)))
                .Select(Assembly.LoadFrom);

            return assemblies;
        }

        /// <summary>
        /// Получение информации об окружении.
        /// </summary>
        /// <returns>StartupInfo.</returns>
        public static StartupInfo GetStartupInfo()
        {
            var info = new StartupInfo();
            var executingAssembly = Assembly.GetExecutingAssembly();
            info.Version = executingAssembly.GetName().Version.ToString(3);
            info.CurrentDirectory = GetCurrentDirectory();
            info.BaseDirectory = GetBaseDirectory();
            info.StartupApp = Path.GetFileName(executingAssembly.Location);

            return info;
        }

        public static string GetBaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
            return AppContext.BaseDirectory;
        }

        public static string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public static bool IsUserInteractive()
        {
            return Environment.UserInteractive;
        }
    }
}
