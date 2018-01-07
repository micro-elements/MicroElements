// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Configuration
{
    /// <summary>
    /// Аргументы командной строки.
    /// </summary>
    public class CommandLineArgs
    {
        /// <summary>
        /// Null object.
        /// </summary>
        public static readonly CommandLineArgs Null = new CommandLineArgs(new string[0]);

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineArgs"/> class.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        public CommandLineArgs(string[] args)
        {
            Args = args ?? new string[0];
        }

        /// <summary>
        /// Аргументы командной строки.
        /// </summary>
        public string[] Args { get; }
    }
}
