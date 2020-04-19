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
    /// Расширения для <see cref="Microsoft.Extensions.Configuration"/>.
    /// </summary>
    public static class ConfigurationReader
    {
        /// <summary>
        /// Загрузка конфигурации.
        /// </summary>
        /// <param name="buildContext">Контекст построения приложения.</param>
        public static void LoadConfiguration(BuildContext buildContext)
        {
            var startupConfiguration = buildContext.StartupConfiguration;

            // Настройка чтения конфигураций
            IConfigurationBuilder builder = buildContext.StartupConfiguration.ConfigurationBuilder ?? new ConfigurationBuilder();

            // Добавляем начальное пользовательское конфигурирование.
            if (startupConfiguration.BeginConfiguration != null)
            {
                builder = startupConfiguration.BeginConfiguration(builder);
            }

            // Вычислители без контекста
            var evaluators = ValueEvaluator.CreateValueEvaluators(buildContext, null, statelessEvaluators: true);
            builder.Properties.SetValue(BuilderContext.Key.StatelessEvaluators, evaluators.ToArray());

            // Добавляем стандартное конфигурирование из файловой директории.
            AddFileConfiguration(buildContext, builder);

            // Параметры командной строки перекрывают все.
            builder = builder.AddCommandLine(startupConfiguration.CommandLineArgs?.Args ?? new string[0]);

            // Добавляем конечное пользовательское конфигурирование.
            if (startupConfiguration.EndConfiguration != null)
            {
                builder = startupConfiguration.EndConfiguration(builder);
            }

            // (Step1) Построение конфигурации
            var configurationRoot = builder.Build();

            // Для вычисления Placeholders.
            IEnumerable<IValueEvaluator> valueEvaluators = ValueEvaluator.CreateValueEvaluators(buildContext, configurationRoot);
            builder.Add(new PlaceholdersConfigurationSource(configurationRoot, valueEvaluators));

            // (Step2) Повторное построение, чтобы рассчитать вычисляемые значения.
            buildContext.ConfigurationRoot = builder.Build();
        }

        /// <summary>
        /// Добавление всех конфигурационных файлов в конфигурацию.
        /// Файлы конфигурации определяются параметром <see cref="searchPatterns"/>
        /// </summary>
        /// <param name="builder">ConfigurationBuilder в который добавляются конфигурации.</param>
        /// <param name="configurationPath">Директория для поиска файлов конфигурации.</param>
        /// <param name="searchPatterns">Паттерны для поиска файлов.</param>
        /// <returns>Итоговый ConfigurationBuilder.</returns>
        public static IConfigurationBuilder AddConfigurationFiles(this IConfigurationBuilder builder, string configurationPath, string[] searchPatterns)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            foreach (var searchPattern in searchPatterns)
            {
                foreach (var file in Directory.EnumerateFiles(configurationPath, searchPattern, SearchOption.TopDirectoryOnly))
                {
                    var extension = Path.GetExtension(file)?.ToLower();
                    if (extension == ".xml")
                    {
                        builder = builder.AddXmlFile(file, optional: false, reloadOnChange: true);
                    }

                    if (extension == ".json")
                    {
                        // Создадим провайдер Json и обернем его в свой декоратор.
                        var jsonConfigurationSource = new JsonConfigurationSource { Path = file.PathNormalize(), Optional = false, ReloadOnChange = true };
                        jsonConfigurationSource.ResolveFileProvider();
                        var statelessEvaluators = builder.Properties.GetValue(BuilderContext.Key.StatelessEvaluators) ?? Array.Empty<IValueEvaluator>();
                        builder.Add(new ProcessIncludesConfigurationSource(jsonConfigurationSource, configurationPath.PathNormalize(), statelessEvaluators));
                    }
                }
            }

            return builder;
        }

        /// <summary>
        /// Добавление всех конфигурационных файлов в конфигурацию.
        /// В выборку попадают все xml и json файлы.
        /// </summary>
        /// <param name="builder">ConfigurationBuilder в который добавляются конфигурации.</param>
        /// <param name="configurationPath">Директория для поиска файлов конфигурации.</param>
        /// <returns>Итоговый ConfigurationBuilder.</returns>
        public static IConfigurationBuilder AddConfigurationFiles(this IConfigurationBuilder builder, string configurationPath)
        {
            return builder.AddConfigurationFiles(configurationPath, new[] { "*.json", "*.xml" });
        }

        private static void AddFileConfiguration(BuildContext buildContext, IConfigurationBuilder builder)
        {
            var startupConfiguration = buildContext.StartupConfiguration;

            var configurationPath = startupConfiguration.ConfigurationPath;
            var configurationProfile = startupConfiguration.Profile;

            if (configurationPath != null)
            {
                // Базовый путь для чтения конфигураций
                var configurationBasePath = Path.IsPathRooted(configurationPath)
                    ? configurationPath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configurationPath);

                if (!Directory.Exists(configurationBasePath))
                    throw new Exception($"ConfigurationBasePath ${configurationBasePath} doesn't exists");

                buildContext.AddBuildInfo("ConfigurationBasePath", configurationBasePath);

                // Добавляем файлы из корня конфигурации, переопределяем конфигурацию профильными конфигами
                builder = builder.AddConfigurationFiles(configurationBasePath);

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

                    buildContext.AddBuildInfo("ConfigurationProfileDirectory", profileDirectory);
                }
            }
        }
    }
}
