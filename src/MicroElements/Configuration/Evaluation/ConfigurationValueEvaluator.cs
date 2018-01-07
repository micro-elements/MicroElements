// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace MicroElements.Bootstrap.Extensions.Configuration.Evaluation
{
    /// <summary>
    /// Вычисление выражений вида ${configurationValue:configurationValueFullName}.
    /// Выражение вычисляется как получение значения из <see cref="IConfigurationRoot"/>.
    /// </summary>
    public class ConfigurationValueEvaluator : IValueEvaluator
    {
        private readonly IConfigurationRoot _configurationRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationValueEvaluator"/> class.
        /// </summary>
        /// <param name="configurationRoot">Корень конфигурации.</param>
        public ConfigurationValueEvaluator(IConfigurationRoot configurationRoot)
        {
            _configurationRoot = configurationRoot;
        }

        /// <inheritdoc />
        public string Name => "configurationValue";

        /// <inheritdoc />
        public bool TryEvaluate(string expression, out string value)
        {
            // Часто используют '.' вместо ':', поэтому автоматом исправим
            var configurationKey = expression.Replace('.', ':');
            value = _configurationRoot.GetValue<string>(configurationKey);
            return value != null;
        }
    }
}
