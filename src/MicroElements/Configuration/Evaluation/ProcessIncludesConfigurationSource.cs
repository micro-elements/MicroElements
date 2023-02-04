// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для препроцессинга конфигурации.
    /// </summary>
    public class ProcessIncludesConfigurationSource : IConfigurationSource
    {
        private readonly IConfigurationSource _configurationSource;

        public string RootPath { get; }
        public bool ReloadOnChange { get; }
        public IReadOnlyCollection<IValueEvaluator>? ValueEvaluators { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessIncludesConfigurationSource"/> class.
        /// </summary>
        /// <param name="configurationSource">The original <see cref="IConfigurationSource"/>.</param>
        /// <param name="rootPath">rootPath</param>
        /// <param name="valueEvaluators">Evaluator that can be used in include.</param>
        public ProcessIncludesConfigurationSource(
            IConfigurationSource configurationSource,
            string rootPath,
            bool reloadOnChange,
            IReadOnlyCollection<IValueEvaluator>? valueEvaluators)
        {
            _configurationSource = configurationSource;
            RootPath = rootPath;
            ValueEvaluators = valueEvaluators;
            ReloadOnChange = reloadOnChange;
        }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            IConfigurationProvider fileConfigurationProvider = _configurationSource.Build(builder);
            return new ProcessIncludesConfigurationProvider(fileConfigurationProvider, this);
        }
    }
}
