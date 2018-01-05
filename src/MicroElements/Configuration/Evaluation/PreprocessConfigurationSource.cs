using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace MicroElements.Bootstrap.Extensions.Configuration.Evaluation
{
    /// <summary>
    /// Провайдер конфигурации для препроцессинга конфигурации.
    /// </summary>
    public class PreprocessConfigurationSource : IConfigurationSource
    {
        private readonly JsonConfigurationSource _jsonConfigurationSource;
        private readonly string _rootPath;

        /// <summary>
        /// Конструктор.
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