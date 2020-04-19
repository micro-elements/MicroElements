// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для препроцессинга конфигурации.
    /// </summary>
    public class ProcessIncludesConfigurationSource : IConfigurationSource
    {
        private readonly JsonConfigurationSource _jsonConfigurationSource;
        private readonly string _rootPath;
        private readonly IReadOnlyCollection<IValueEvaluator> _valueEvaluators;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessIncludesConfigurationSource"/> class.
        /// </summary>
        /// <param name="jsonConfigurationSource">jsonConfigurationSource</param>
        /// <param name="rootPath">rootPath</param>
        /// <param name="valueEvaluators">Evaluator that can be used in include.</param>
        public ProcessIncludesConfigurationSource(
            JsonConfigurationSource jsonConfigurationSource,
            string rootPath,
            IReadOnlyCollection<IValueEvaluator> valueEvaluators = null)
        {
            _jsonConfigurationSource = jsonConfigurationSource;
            _rootPath = rootPath;
            _valueEvaluators = valueEvaluators;
        }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileConfigurationProvider fileConfigurationProvider = (FileConfigurationProvider)_jsonConfigurationSource.Build(builder);
            return new ProcessIncludesConfigurationProvider(fileConfigurationProvider, _rootPath, _valueEvaluators);
        }
    }
}
