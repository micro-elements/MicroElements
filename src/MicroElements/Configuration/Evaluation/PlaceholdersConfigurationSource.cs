// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Abstractions;
using Microsoft.Extensions.Configuration;

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для вычисления динамических и подстановочных значений (placeholders).
    /// </summary>
    public class PlaceholdersConfigurationSource : IConfigurationSource
    {
        private readonly BuildContext _buildContext;
        private readonly IConfigurationBuilder _configurationBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceholdersConfigurationSource"/> class.
        /// </summary>
        /// <param name="buildContext">Build context.</param>
        /// <param name="configurationBuilder">ConfigurationBuilder for initial configuration.</param>
        public PlaceholdersConfigurationSource(BuildContext buildContext, IConfigurationBuilder configurationBuilder)
        {
            _buildContext = buildContext;
            _configurationBuilder = configurationBuilder;
        }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new PlaceholdersConfigurationProvider(_buildContext, _configurationBuilder);
        }
    }
}
