// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MicroElements.Abstractions;
using MicroElements.Bootstrap;
using MicroElements.Bootstrap.Extensions;
using MicroElements.Configuration.Evaluation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace MicroElements.Configuration
{
    /// <summary>
    /// Extensions for <see cref="Microsoft.Extensions.Configuration"/>.
    /// </summary>
    public static class ConfigurationReader
    {
        /// <summary>
        /// Загрузка конфигурации.
        /// </summary>
        /// <param name="buildContext">Контекст построения приложения.</param>
        public static void LoadConfiguration(BuildContext buildContext, bool reloadOnChange = true)
        {
            var startupConfiguration = buildContext.StartupConfiguration;

            // Настройка чтения конфигураций
            IConfigurationBuilder builder = startupConfiguration.ConfigurationBuilder ?? new ConfigurationBuilder();

            // Добавляем начальное пользовательское конфигурирование.
            if (startupConfiguration.BeginConfiguration != null)
            {
                builder = startupConfiguration.BeginConfiguration(builder);
            }

            // Вычислители без контекста
            builder.Properties.AddIfNotExists(BuilderContext.Key.StatelessEvaluators, _ => ValueEvaluator.CreateValueEvaluators(buildContext, null, statelessEvaluators: true).ToArray());

            // Добавляем стандартное конфигурирование из файловой директории.
            builder.AddFileConfiguration(buildContext, reloadOnChange: reloadOnChange);

            // Параметры командной строки перекрывают все.
            builder = builder.AddCommandLine(startupConfiguration.CommandLineArgs?.Args ?? new string[0]);

            // Добавляем конечное пользовательское конфигурирование.
            if (startupConfiguration.EndConfiguration != null)
            {
                builder = startupConfiguration.EndConfiguration(builder);
            }

            // (Step1) Построение конфигурации
            IConfigurationBuilder builderCopy = builder.CopyConfigurationBuilder();

            // Для вычисления Placeholders.
            builder.Add(new PlaceholdersConfigurationSource(buildContext, builderCopy));

            // (Step2) Повторное построение, чтобы рассчитать вычисляемые значения.
            buildContext.ConfigurationRoot = builder.Build();
        }

        public static IConfigurationBuilder CopyConfigurationBuilder(this IConfigurationBuilder builder)
        {
            IConfigurationBuilder builderCopy = new ConfigurationBuilder();
            foreach (var pair in builder.Properties)
            {
                builderCopy.Properties.Add(pair.Key, pair.Value);
            }

            foreach (IConfigurationSource source in builder.Sources)
            {
                builderCopy.Add(source);
            }

            return builderCopy;
        }

        /// <summary>
        /// Добавление всех конфигурационных файлов в конфигурацию.
        /// Файлы конфигурации определяются параметром <paramref name="searchPatterns"/>
        /// </summary>
        /// <param name="builder">ConfigurationBuilder в который добавляются конфигурации.</param>
        /// <param name="configurationPath">Директория для поиска файлов конфигурации.</param>
        /// <param name="searchPatterns">Паттерны для поиска файлов.</param>
        /// <param name="reloadOnChange">Determines whether the source will be loaded if the underlying file changes.</param>
        /// <returns>Итоговый ConfigurationBuilder.</returns>
        public static IConfigurationBuilder AddConfigurationFiles(
            this IConfigurationBuilder builder,
            string configurationPath,
            string[]? searchPatterns = null,
            bool reloadOnChange = true)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            searchPatterns ??= new[] { "*.json" };

            IReadOnlyCollection<IValueEvaluator> statelessEvaluators = builder.Properties.GetValue(BuilderContext.Key.StatelessEvaluators) ?? Array.Empty<IValueEvaluator>();

            foreach (var searchPattern in searchPatterns)
            {
                foreach (var file in Directory.EnumerateFiles(configurationPath, searchPattern, SearchOption.TopDirectoryOnly))
                {
                    var extension = Path.GetExtension(file)?.ToLower();

                    if (extension == ".json")
                    {
                        // JsonConfigurationSource with include support.
                        var jsonConfigurationSource = new JsonConfigurationSource
                        {
                            Path = file.PathNormalize(),
                            Optional = false,
                            ReloadOnChange = reloadOnChange,
                        };

                        jsonConfigurationSource.ResolveFileProvider();
                        if (jsonConfigurationSource.FileProvider is null)
                        {
                            jsonConfigurationSource.FileProvider = builder.GetFileProvider();
                        }

                        builder.Add(new ProcessIncludesConfigurationSource(jsonConfigurationSource, configurationPath.PathNormalize(), reloadOnChange, statelessEvaluators));
                    }
                }
            }

            return builder;
        }

        public static void AddFileConfiguration(
            this IConfigurationBuilder builder,
            BuildContext buildContext,
            bool reloadOnChange = true)
        {
            var startupConfiguration = buildContext.StartupConfiguration;

            var configurationPath = startupConfiguration.ConfigurationPath;
            var configurationProfile = startupConfiguration.Profile;

            if (configurationPath != null)
            {
                // Base path (full directory name)
                var configurationBasePath = Path.IsPathRooted(configurationPath)
                    ? configurationPath
                    : Path.Combine(AppContext.BaseDirectory, configurationPath);

                if (!Directory.Exists(configurationBasePath))
                    throw new Exception($"ConfigurationBasePath ${configurationBasePath} doesn't exists");

                builder.AddEnvInfo("ConfigurationBasePath", configurationBasePath);

                // Add files from base path.
                builder = builder.AddConfigurationFiles(configurationBasePath, reloadOnChange: reloadOnChange);

                if (!string.IsNullOrEmpty(configurationProfile))
                {
                    var dirs = configurationProfile.PathNormalize().Split(Path.DirectorySeparatorChar, '.');

                    var cumulativePath = configurationBasePath.PathNormalize();
                    string profileDirectory = string.Empty;
                    foreach (var dir in dirs)
                    {
                        // Путь к профильной конфигурации
                        var subProfileDirectory = Path.Combine(cumulativePath, dir);
                        cumulativePath = subProfileDirectory;

                        if (Directory.Exists(subProfileDirectory))
                        {
                            profileDirectory = subProfileDirectory;

                            // Переопределяем конфигурацию профильными конфигами
                            builder = builder.AddConfigurationFiles(subProfileDirectory);
                        }
                    }

                    builder.AddEnvInfo("ConfigurationProfileDirectory", profileDirectory);
                }
            }
        }
    }
}
