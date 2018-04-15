// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для препроцессинга конфигурации.
    /// </summary>
    public class PreprocessConfigurationSource : IConfigurationSource
    {
        private readonly JsonConfigurationSource _jsonConfigurationSource;
        private readonly string _rootPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreprocessConfigurationSource"/> class.
        /// </summary>
        /// <param name="jsonConfigurationSource">jsonConfigurationSource</param>
        /// <param name="rootPath">rootPath</param>
        public PreprocessConfigurationSource(JsonConfigurationSource jsonConfigurationSource, string rootPath)
        {
            _jsonConfigurationSource = jsonConfigurationSource;
            _rootPath = rootPath;
        }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new PreprocessConfigurationProvider((FileConfigurationProvider)_jsonConfigurationSource.Build(builder), _rootPath);
        }
    }
}
