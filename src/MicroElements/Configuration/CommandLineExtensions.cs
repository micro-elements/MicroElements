// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MicroElements.Configuration
{
    public static class CommandLineExtensions
    {
        /// <summary>
        /// Polulates properties of <see cref="targetObject"/> from command line arguments.
        /// </summary>
        /// <param name="targetObject">Target.</param>
        /// <param name="args">Command Line Args.</param>
        /// <returns>Corrected object.</returns>
        public static void BuildUpFromCommandLineArgs<T>(this T targetObject, string[] args)
        {
            if (targetObject == null)
                throw new ArgumentNullException(nameof(targetObject));

            if (args != null && args.Length > 0)
            {
                var configuration = new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build();

                configuration.Bind(targetObject);
            }
        }

        /// <summary>
        /// Получение параметров командной строки.
        /// </summary>
        /// <returns>Параметры командной строки или пустой массив.</returns>
        public static string[] GetCommandLine()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            return commandLineArgs.Length > 1 ? commandLineArgs.Skip(1).ToArray() : Array.Empty<string>();
        }
    }
}
