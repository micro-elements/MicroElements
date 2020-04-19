// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Abstractions;
using MicroElements.Bootstrap.Extensions;
using MicroElements.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Configuration.Evaluation
{
    public static class ValueEvaluator
    {
        public static IReadOnlyCollection<IValueEvaluator> PredefinedEvaluators { get; } = new List<IValueEvaluator>()
        {
            new EnvironmentEvaluator()
        };

        public static IEnumerable<IValueEvaluator> CreateValueEvaluators(BuildContext buildContext, IConfigurationRoot configurationRoot, bool statelessEvaluators = false)
        {
            // Делаем копию ServiceCollection, чтобы не портить временной регистрацией.
            IServiceCollection serviceCollectionCopy = buildContext.ServiceCollection.Copy();

            if (configurationRoot != null)
                serviceCollectionCopy.AddSingleton(configurationRoot);

            var evaluatorTypes = buildContext
                .ExportedTypes
                .Where(type => type.IsClassAssignableTo<IValueEvaluator>())
                .Where(type => !statelessEvaluators || type.GetConstructor(Type.EmptyTypes) != null)
                .ToArray();

            foreach (Type evaluatorType in evaluatorTypes)
            {
                serviceCollectionCopy.AddSingleton(typeof(IValueEvaluator), evaluatorType);
            }

            var serviceProvider = serviceCollectionCopy.BuildServiceProvider();
            var valueEvaluators = serviceProvider.GetServices<IValueEvaluator>();
            return valueEvaluators;
        }
    }
}
