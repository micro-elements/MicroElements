using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace MicroElements.Bootstrap.Extensions.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для препроцессинга конфигурации.
    /// </summary>
    public class PreprocessConfigurationProvider : FileConfigurationProvider
    {
        private readonly FileConfigurationProvider _configurationProvider;
        private readonly string _rootPath;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="configurationProvider">configurationProvider</param>
        /// <param name="rootPath">rootPath</param>
        public PreprocessConfigurationProvider(FileConfigurationProvider configurationProvider, string rootPath)
            : base(configurationProvider.Source)
        {
            _configurationProvider = configurationProvider;
            _rootPath = rootPath;
        }

        /// <inheritdoc />
        public override void Load(Stream stream)
        {
            _configurationProvider.Load(stream);

            // Получим список ключей
            var keys = _configurationProvider.GetKeys();

            bool IsIncludeKey(string key) => key.EndsWith("${include}");
            bool IsNotIncludeKey(string key) => !IsIncludeKey(key);

            // Ключи с ${include}
            var keysWithIncludes = keys.Where(IsIncludeKey).ToArray();
            if (keysWithIncludes.Length > 0)
            {
                foreach (var include in keysWithIncludes)
                {
                    if (_configurationProvider.TryGet(include, out string includePath))
                    {
                        var path = Path.Combine(_rootPath, includePath);
                        var fullPath = Path.GetFullPath(path);

                        // Создадим провайдер конфигурации и загрузим значения из него
                        var jsonConfigurationProvider = CreateConfigurationProvider(fullPath);
                        jsonConfigurationProvider.Load();

                        // Получим все ключи
                        var keysToInclude = jsonConfigurationProvider.GetKeys();

                        // Добавим все данные из подгруженного файла
                        jsonConfigurationProvider.AddValuesToDictionary(keysToInclude, Data);
                    }
                }

                // Добавим ключи, которые были в базовом провайдере
                var otherKeys = keys.Where(IsNotIncludeKey).ToArray();
                _configurationProvider.AddValuesToDictionary(otherKeys, Data);
            }
            else
            {
                // Добавим ключи, которые были в базовом провайдере
                _configurationProvider.AddValuesToDictionary(keys, Data);
            }
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