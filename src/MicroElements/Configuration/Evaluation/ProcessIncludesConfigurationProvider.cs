// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Primitives;

namespace MicroElements.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для препроцессинга конфигурации.
    /// </summary>
    public class ProcessIncludesConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private readonly string _rootPath;
        private readonly IReadOnlyCollection<IValueEvaluator> _valueEvaluators;

        private readonly ProviderHandler _rootHandler;
        private IReadOnlyCollection<ProviderHandler> _childHandlers; 

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessIncludesConfigurationProvider"/> class.
        /// </summary>
        /// <param name="configurationProvider">configurationProvider</param>
        /// <param name="rootPath">rootPath</param>
        /// <param name="valueEvaluators">Evaluator that can be used in include.</param>
        public ProcessIncludesConfigurationProvider(
            IConfigurationProvider configurationProvider,
            string rootPath,
            IReadOnlyCollection<IValueEvaluator> valueEvaluators = null)
        {
            _rootPath = rootPath;
            _valueEvaluators = valueEvaluators;
            _rootHandler = BuildHandler(configurationProvider);
        }

        /// <inheritdoc />
        public override void Load() 
        {
            _rootHandler.Provider.Load();
            _childHandlers ??= InitializeHandlers(_rootHandler.Provider, new Dictionary<string, ProviderHandler>());

            RebuildData();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _rootHandler.ReloadHandler.Dispose();

            foreach ((IConfigurationProvider provider, IDisposable handler) in _childHandlers ?? Enumerable.Empty<ProviderHandler>())
            {
                handler.Dispose();
                (provider as IDisposable)?.Dispose();
            }
        }

        private IReadOnlyCollection<ProviderHandler> InitializeHandlers(IConfigurationProvider provider, Dictionary<string, ProviderHandler> includedFiles)
        {
            foreach (string key in provider.GetKeys())
            {
                if (IsIncludeKey(key) && provider.TryGet(key, out string includePath))
                {
                    includePath = SimpleExpressionParser.ParseAndRender(includePath, _valueEvaluators) ?? includePath;

                    if (!string.IsNullOrWhiteSpace(includePath) && !includedFiles.ContainsKey(includePath))
                    {
                        var childProvider = LoadIncludedConfiguration(includePath);

                        includedFiles.Add(includePath, BuildHandler(childProvider));
                        InitializeHandlers(childProvider, includedFiles);
                    }
                }
            }

            return includedFiles.Values;
        }

        private ProviderHandler BuildHandler(IConfigurationProvider childProvider) 
            => new
            (
                childProvider,
                ChangeToken.OnChange(() => childProvider.GetReloadToken(), () => RebuildData())
            );

        private IConfigurationProvider LoadIncludedConfiguration(string includePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(_rootPath, includePath));
            var jsonConfigurationProvider = CreateConfigurationProvider(fullPath);

            jsonConfigurationProvider.Load();
            return jsonConfigurationProvider;

            static IConfigurationProvider CreateConfigurationProvider(string fullPath)
            {
                // todo: Можно расширить виды поддерживаемых провайдеров
                var jsonConfigurationSource = new JsonConfigurationSource { Path = fullPath, ReloadOnChange = true };
                jsonConfigurationSource.ResolveFileProvider();
                var jsonConfigurationProvider = new JsonConfigurationProvider(jsonConfigurationSource);
                return jsonConfigurationProvider;
            }
        }

        private void RebuildData()
        {
            Data = new Dictionary<string, string>();
            CopyValues(_rootHandler);

            foreach (ProviderHandler handler in _childHandlers ?? Enumerable.Empty<ProviderHandler>())
            {
                CopyValues(handler);
            }

            void CopyValues(ProviderHandler handler)
            {
                foreach (string key in handler.Provider.GetKeys())
                {
                    if (!IsIncludeKey(key) && handler.Provider.TryGet(key, out string value))
                    {
                        Data.TryAdd(key, value);
                    }
                }
            }

            OnReload();
        }

        private static bool IsIncludeKey(string key) => key.StartsWith("${include}");

        private sealed record ProviderHandler(IConfigurationProvider Provider, IDisposable ReloadHandler);
    }
}
