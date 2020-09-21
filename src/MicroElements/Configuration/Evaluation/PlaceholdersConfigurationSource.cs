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
        /// <param name="buildContext">Корень конфигурации.</param>
        /// <param name="configurationBuilder">Вычислители значений.</param>
        public PlaceholdersConfigurationSource(BuildContext buildContext, IConfigurationBuilder configurationBuilder)
        {
            _buildContext = buildContext;
            _configurationBuilder = configurationBuilder;
        }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            IConfigurationRoot firstIteration = _configurationBuilder.Build();
            var evaluators = ValueEvaluator.CreateValueEvaluators(_buildContext, firstIteration);
            return new PlaceholdersConfigurationProvider(firstIteration, evaluators);
        }
    }
}
