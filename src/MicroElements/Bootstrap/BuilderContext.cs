// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Configuration.Evaluation;

namespace MicroElements.Bootstrap
{
    public static class BuilderContext
    {
        public static class Key
        {
            public static readonly ContextKey<IStartupConfiguration> StartupConfiguration = new("MicroElements.StartupConfiguration");
            public static readonly ContextKey<IReadOnlyCollection<IValueEvaluator>> StatelessEvaluators = new("MicroElements.StatelessEvaluators");
            public static readonly ContextKey<IReadOnlyCollection<IValueEvaluator>> ValueEvaluators = new("MicroElements.ValueEvaluators");
        }

        public static T? GetValue<T>(this IDictionary<string, object> context, ContextKey<T> key)
        {
            if (context.TryGetValue(key.Key, out object value))
            {
                return (T)value;
            }

            return default;
        }

        public static void SetValue<T>(this IDictionary<string, object> context, ContextKey<T> key, T value)
        {
            context.Add(key.Key, value);
        }

        public static T GetOrAdd<T>(this IDictionary<string, object> context, ContextKey<T> key, Func<ContextKey<T>, T> factory)
        {
            var value = context.GetValue(key);
            if (value == null)
            {
                value = factory(key) ?? throw new InvalidOperationException("factory returned null");
                context.SetValue(key, value);
            }

            return value;
        }

        public static T GetOrAdd<T>(this IDictionary<string, object> context, ContextKey<T> key, Func<T> factory)
            => context.GetOrAdd(key, _ => factory());

        public static void AddIfNotExists<T>(this IDictionary<string, object> context, ContextKey<T> key, Func<ContextKey<T>, T> factory)
            => context.GetOrAdd(key, factory);

        public static void AddIfNotExists<T>(this IDictionary<string, object> context, ContextKey<T> key, Func<T> factory)
            => context.GetOrAdd(key, _ => factory());
    }

    public readonly record struct ContextKey<T>(string Key);
}
