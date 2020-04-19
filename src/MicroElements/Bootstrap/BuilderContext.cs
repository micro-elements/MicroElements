// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Configuration.Evaluation;

namespace MicroElements.Bootstrap
{
    public static class BuilderContext
    {
        public static class Key
        {
            public static readonly ContextKey<IStartupConfiguration> StartupConfiguration = new ContextKey<IStartupConfiguration>("MicroElements.StartupConfiguration");
            public static readonly ContextKey<IReadOnlyCollection<IValueEvaluator>> StatelessEvaluators = new ContextKey<IReadOnlyCollection<IValueEvaluator>>("MicroElements.Evaluators");
        }

        public static T GetValue<T>(this IDictionary<object, object> context, ContextKey<T> key)
        {
            if (context.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            return default;
        }

        public static T GetValue<T>(this IDictionary<string, object> context, ContextKey<T> key)
        {
            if (context.TryGetValue(key.AsText, out object value))
            {
                return (T)value;
            }

            return default;
        }

        public static void SetValue<T>(this IDictionary<object, object> context, ContextKey<T> key, T value)
        {
            context.Add(key, value);
        }

        public static void SetValue<T>(this IDictionary<string, object> context, ContextKey<T> key, T value)
        {
            context.Add(key.AsText, value);
        }
    }

    public readonly struct ContextKey<T> : IEquatable<ContextKey<T>>
    {
        public ContextKey(string key)
        {
            Key = key;
        }

        public string Key { get; }

        public Type Type => typeof(T);

        public string AsText => $"({Key}, {Type})";

        /// <inheritdoc />
        public override string ToString() => AsText;

        /// <inheritdoc />
        public bool Equals(ContextKey<T> other) => Key == other.Key && Type == other.Type;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is ContextKey<T> other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ (Type != null ? Type.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ContextKey<T> left, ContextKey<T> right) => left.Equals(right);

        public static bool operator !=(ContextKey<T> left, ContextKey<T> right) => !left.Equals(right);
    }
}
