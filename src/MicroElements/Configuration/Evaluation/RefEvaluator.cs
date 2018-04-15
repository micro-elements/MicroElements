// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace MicroElements.Configuration.Evaluation
{
    internal class RefEvaluatorDoNotUse //: IValueEvaluator
    {
        private readonly IConfigurationRoot _configurationRoot;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefEvaluatorDoNotUse"/> class.
        /// </summary>
        /// <param name="configurationRoot">Корень конфигурации.</param>
        public RefEvaluatorDoNotUse(IConfigurationRoot configurationRoot, IServiceProvider serviceProvider)
        {
            _configurationRoot = configurationRoot;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public string Name => "ref";

        /// <inheritdoc />
        public bool TryEvaluate(string expression, out string value)
        {
            var type = Type.GetType("MicroElements.Tests.Model.InnerObject, MicroElements.Tests");
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(type);
            var service = _serviceProvider.GetService(enumerableType);
            // Часто используют '.' вместо ':', поэтому автоматом исправим
            var configurationKey = expression.Replace('.', ':');
            var sec = _configurationRoot.GetSection(configurationKey);
            value = null;
            return false;
        }
    }
}
