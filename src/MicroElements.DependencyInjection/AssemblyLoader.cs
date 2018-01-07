// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MicroElements.DependencyInjection
{
    /// <summary>
    /// Утилиты для облегчения работы с Reflection.
    /// </summary>
    public static class AssemblyLoader
    {
        /// <summary>
        /// Загрузка сборок в память по маске.
        /// </summary>
        /// <param name="scanDirectory">Директория из которой нужно грузить сборки.</param>
        /// <param name="assemblyScanPatterns">Маска поиска файлов.</param>
        /// <returns>Список найденных сборок.</returns>
        public static Assembly[] LoadAssemblies(string scanDirectory, params string[] assemblyScanPatterns)
        {
            string WildcardToRegex(string pat) =>
                "^" + Regex.Escape(pat).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

            bool FileNameMatchesPattern(string filename, string pattern) =>
                Regex.IsMatch(Path.GetFileName(filename), WildcardToRegex(pattern));

            var assemblies = Directory.EnumerateFiles(scanDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                .Concat(Directory.EnumerateFiles(scanDirectory, "*.exe", SearchOption.TopDirectoryOnly))
                .Where(filename => assemblyScanPatterns.Any(pattern => FileNameMatchesPattern(filename, pattern)))
                .Select(Assembly.LoadFrom)
                .ToArray();

            return assemblies;
        }
    }
}