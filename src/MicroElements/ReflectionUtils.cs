// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MicroElements.Abstractions;
using MicroElements.Collections.Extensions;
using MicroElements.Reflection;

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
            List<string> messagesList = new List<string>();

            AssemblySource assemblySource = new AssemblySource(
                loadFromDomain: true,
                loadFromDirectory: scanDirectory,
                searchPatterns: assemblyScanPatterns,
                assemblyFilters: new AssemblyFilters(assemblyScanPatterns));

            IEnumerable<Assembly> assemblies = TypeLoader
                .LoadAssemblies(assemblySource, messagesList)
                .ToArrayDebug();

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
