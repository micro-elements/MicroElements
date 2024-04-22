// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.Abstractions;
using MicroElements.Bootstrap.Extensions;
using MicroElements.DependencyInjection;
using MicroElements.Reflection.TypeExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroElements.Configuration.Evaluation
{
    public static class ValueEvaluator
    {
        public static IReadOnlyCollection<IValueEvaluator> PredefinedEvaluators { get; } = new List<IValueEvaluator>()
        {
            new EnvironmentEvaluator()
        };

        public static IEnumerable<IValueEvaluator> CreateValueEvaluators(
            BuildContext buildContext,
            IConfigurationRoot? configurationRoot,
            bool statelessEvaluators = false)
        {
            // Делаем копию ServiceCollection, чтобы не портить временной регистрацией.
            IServiceCollection serviceCollectionCopy = buildContext.ServiceCollection.Copy();

            if (configurationRoot != null)
                serviceCollectionCopy.AddSingleton(configurationRoot);

            //TODO:
            var evaluatorTypes = buildContext
                .ExportedTypes
                .Where(type => type.IsConcreteAndAssignableTo<IValueEvaluator>());

            if (statelessEvaluators)
            {
                evaluatorTypes = evaluatorTypes.Where(type => type.GetConstructors().Any(info => info.IsPublic && info.GetParameters().Length == 0));
            }
            else
            {
                evaluatorTypes = evaluatorTypes.Where(type => type.GetConstructors().Any(info => info.IsPublic));
            }

            foreach (Type evaluatorType in evaluatorTypes)
            {
                serviceCollectionCopy.TryAddEnumerable(new ServiceDescriptor(typeof(IValueEvaluator), evaluatorType, ServiceLifetime.Singleton));
            }

            var serviceProvider = serviceCollectionCopy.BuildServiceProvider();
            var valueEvaluators = serviceProvider.GetServices<IValueEvaluator>() ?? Array.Empty<IValueEvaluator>();
            return valueEvaluators;
        }
    }
}
