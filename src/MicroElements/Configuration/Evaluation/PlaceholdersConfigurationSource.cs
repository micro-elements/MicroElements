// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для вычисления динамических и подстановочных значений (placeholders).
    /// </summary>
    public class PlaceholdersConfigurationSource : IConfigurationSource
    {
        private readonly IConfigurationRoot _configurationRoot;
        private readonly IEnumerable<IValueEvaluator> _evaluators;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceholdersConfigurationSource"/> class.
        /// </summary>
        /// <param name="configurationRoot">Корень конфигурации.</param>
        /// <param name="evaluators">Вычислители значений.</param>
        public PlaceholdersConfigurationSource(IConfigurationRoot configurationRoot, IEnumerable<IValueEvaluator> evaluators)
        {
            _configurationRoot = configurationRoot;
            _evaluators = evaluators;
        }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new PlaceholdersConfigurationProvider(_configurationRoot, _evaluators);
        }
    }
}
