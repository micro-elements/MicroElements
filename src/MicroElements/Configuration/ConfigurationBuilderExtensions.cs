using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace MicroElements.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        private static string EnvironmentInfoProviderKey => "MicroElements.EnvironmentInfoProvider";

        /// <summary>
        /// Gets the <see cref="IEnvironmentInfoProvider"/> to be used for store environment information.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>The <see cref="IEnvironmentInfoProvider"/>.</returns>
        public static IEnvironmentInfoProvider GetEnvironmentInfoProvider(this IConfigurationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            IEnvironmentInfoProvider? environmentInfoProvider = null;

            if (builder.Properties.TryGetValue(EnvironmentInfoProviderKey, out object provider))
            {
                environmentInfoProvider = provider as IEnvironmentInfoProvider;
            }

            if (environmentInfoProvider == null)
            {
                environmentInfoProvider = new EnvironmentInfoProvider();
                builder.Properties[EnvironmentInfoProviderKey] = environmentInfoProvider;
            }

            return environmentInfoProvider;
        }

        public static IConfigurationBuilder AddEnvInfo(this IConfigurationBuilder builder, string name, string value)
        {
            builder.GetEnvironmentInfoProvider().SetValue(name, value);
            return builder;
        }
    }

    public interface IEnvironmentInfoProvider
    {
        void SetValue(string name, string value);

        IEnumerable<KeyValuePair<string, string>> GetValues();
    }

    public class EnvironmentInfoProvider : IEnvironmentInfoProvider
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>(comparer: StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public void SetValue(string name, string value) => _values[name] = value;

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> GetValues() => _values;
    }
}
