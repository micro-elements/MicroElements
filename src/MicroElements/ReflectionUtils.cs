// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MicroElements.Abstractions;
using MicroElements.Bootstrap;

namespace MicroElements
{
    /// <summary>
    /// Reflection utils.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Loads assemblies according scanPatterns.
        /// </summary>
        /// <param name="scanDirectory">Directory to scan.</param>
        /// <param name="assemblyScanPatterns">Assembly wildcard scan patterns.</param>
        /// <returns>Assemblies.</returns>
        public static IEnumerable<Assembly> LoadAssemblies(string scanDirectory, params string[] assemblyScanPatterns)
        {
            string WildcardToRegex(string pat) => "^" + Regex.Escape(pat).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            bool FileNameMatchesPattern(string filename, string pattern) => Regex.IsMatch(Path.GetFileName(filename) ?? string.Empty, WildcardToRegex(pattern));

            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var resultFromAppDomain = domainAssemblies.Where(asm => assemblyScanPatterns.Any(pattern => FileNameMatchesPattern(asm.FullName, pattern)));

            var assemblies = Directory.EnumerateFiles(scanDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                .Concat(Directory.EnumerateFiles(scanDirectory, "*.exe", SearchOption.TopDirectoryOnly))
                .Where(filename => assemblyScanPatterns.Any(pattern => FileNameMatchesPattern(filename, pattern)))
                .Select(Assembly.LoadFrom)
                .Union(resultFromAppDomain)
                .Distinct();

            return assemblies;
        }

        /// <summary>
        /// Returns some startup info.
        /// </summary>
        /// <returns>StartupInfo.</returns>
        public static StartupInfo GetStartupInfo()
        {
            var info = new StartupInfo();
            var executingAssembly = Assembly.GetExecutingAssembly();
            info.StartupApp = Path.GetFileName(executingAssembly.Location);
            info.Version = executingAssembly.GetName().Version.ToString(3);
            info.BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            info.CurrentDirectory = Directory.GetCurrentDirectory();

            return info;
        }
    }
}
