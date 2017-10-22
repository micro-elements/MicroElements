using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace MicroComponents.Bootstrap.Extensions.Configuration
{
    public static class CommandLineExtensions
    {
        /// <summary>
        /// Polulates properties of <see cref="targetObject"/> from command line arguments.
        /// </summary>
        /// <param name="targetObject">Target.</param>
        /// <param name="args">Command Line Args.</param>
        /// <returns>Corrected object.</returns>
        public static void BuildUpFromCommandLineArgs<T>([NotNull] this T targetObject, string[] args)
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
    }
}
