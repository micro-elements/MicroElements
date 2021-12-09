// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для препроцессинга конфигурации.
    /// </summary>
    public class ProcessIncludesConfigurationProvider : FileConfigurationProvider
    {
        private readonly FileConfigurationProvider _configurationProvider;
        private readonly string _rootPath;
        private readonly IReadOnlyCollection<IValueEvaluator> _valueEvaluators;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessIncludesConfigurationProvider"/> class.
        /// </summary>
        /// <param name="configurationProvider">configurationProvider</param>
        /// <param name="rootPath">rootPath</param>
        /// <param name="valueEvaluators">Evaluator that can be used in include.</param>
        public ProcessIncludesConfigurationProvider(
            FileConfigurationProvider configurationProvider,
            string rootPath,
            IReadOnlyCollection<IValueEvaluator> valueEvaluators = null)
            : base(configurationProvider.Source)
        {
            _configurationProvider = configurationProvider;
            _rootPath = rootPath;
            _valueEvaluators = valueEvaluators;
        }

        /// <inheritdoc />
        public override void Load(Stream stream)
        {
            _configurationProvider.Load(stream);
            LoadRecursive(_configurationProvider, Data);
        }

        private void LoadRecursive(IConfigurationProvider provider, IDictionary<string, string> targetDictionary)
        {
            // Получим список ключей
            var keys = provider.GetKeys();

            // ${include}, ${include}:0, ...
            static bool IsIncludeKey(string key) => key.StartsWith("${include}");

            foreach (string key in keys)
            {
                if (IsIncludeKey(key))
                {
                    if (provider.TryGet(key, out string includePath))
                    {
                        includePath = SimpleExpressionParser.ParseAndRender(includePath, _valueEvaluators) ?? includePath;
                        bool shouldLoad = !string.IsNullOrWhiteSpace(includePath);
                        if (shouldLoad)
                        {
                            var childProvider = LoadIncludedConfiguration(includePath);

                            // Получим все ключи
                            var keysToInclude = childProvider.GetKeys();

                            // Добавим все данные из подгруженного файла
                            childProvider.CopyValuesToDictionary(keysToInclude, targetDictionary);
                        }
                    }
                }
                else
                {
                    provider.CopyValueToDictionary(key, targetDictionary);
                }
            }
        }

        private IConfigurationProvider LoadIncludedConfiguration(string includePath)
        {
            var path = Path.Combine(_rootPath, includePath);
            var fullPath = Path.GetFullPath(path);

            // Создадим провайдер конфигурации и загрузим значения из него
            var jsonConfigurationProvider = CreateConfigurationProvider(fullPath);
            jsonConfigurationProvider.Load();

            return jsonConfigurationProvider;
        }

        private static IConfigurationProvider CreateConfigurationProvider(string fullPath)
        {
            // todo: Можно расширить виды поддерживаемых провайдеров
            var jsonConfigurationSource = new JsonConfigurationSource { Path = fullPath };
            jsonConfigurationSource.ResolveFileProvider();
            var jsonConfigurationProvider = new JsonConfigurationProvider(jsonConfigurationSource);
            return jsonConfigurationProvider;
        }
    }
}
