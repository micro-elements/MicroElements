// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MicroElements.Bootstrap.Extensions.Configuration.Evaluation
{
    /// <summary>
    /// Расширения для работы с <see cref="IConfigurationProvider"/>.
    /// </summary>
    public static class ConfigurationProviderExtensions
    {
        /// <summary>
        /// Получение списка ключей для <see cref="IConfigurationProvider"/>
        /// </summary>
        /// <param name="configurationProvider">Провайдер конфигурации.</param>
        /// <returns>Список ключей.</returns>
        public static string[] GetKeys(this IConfigurationProvider configurationProvider)
        {
            var keys = new List<string>();
            var childKeys = configurationProvider.GetChildKeys(Enumerable.Empty<string>(), null).Distinct().ToArray();
            keys.AddRange(childKeys);
            AddKeys(keys, configurationProvider, childKeys);
            return keys.ToArray();
        }

        private static void AddKeys(List<string> keys, IConfigurationProvider configurationProvider, IEnumerable<string> parentKeys)
        {
            foreach (var parentPath in parentKeys)
            {
                var childKeys = configurationProvider.GetChildKeys(Enumerable.Empty<string>(), parentPath).Distinct();
                var fullChildKeys = childKeys.Select(s => ConfigurationPath.Combine(parentPath, s)).ToArray();
                if (fullChildKeys.Length > 0)
                {
                    keys.AddRange(fullChildKeys);
                    AddKeys(keys, configurationProvider, fullChildKeys);
                }
            }
        }

        /// <summary>
        /// Добавление всех KeyValue в targetDictionary />
        /// </summary>
        /// <param name="configurationProvider">Провайдер конфигурации.</param>
        /// <param name="keys">Список ключей.</param>
        /// <param name="targetDictionary">Целевой словарь.</param>
        public static void AddValuesToDictionary(this IConfigurationProvider configurationProvider, IEnumerable<string> keys, IDictionary<string, string> targetDictionary)
        {
            foreach (var keyToInclude in keys)
            {
                if (configurationProvider.TryGet(keyToInclude, out string valueToInclude))
                {
                    targetDictionary[keyToInclude] = valueToInclude;
                }
            }
        }
    }
}