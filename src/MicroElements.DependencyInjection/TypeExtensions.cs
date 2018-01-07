// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace MicroElements.DependencyInjection
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Determines whether this type is assignable to <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type to test assignability to.</typeparam>
        /// <param name="this">The type to test.</param>
        /// <returns>True if this type is assignable to references of type
        /// <typeparamref name="T" />; otherwise, False.</returns>
        public static bool IsAssignableTo<T>(this Type @this)
        {
            if ((object)@this == null)
                throw new ArgumentNullException(nameof(@this));
            return typeof(T).GetTypeInfo().IsAssignableFrom(@this.GetTypeInfo());
        }

        public static bool IsClassAssignableTo<T>(this Type @this)
        {
            if ((object)@this == null)
                throw new ArgumentNullException(nameof(@this));
            return IsAssignableTo<T>(@this) && !@this.IsInterface && !@this.IsAbstract;
        }
    }
}