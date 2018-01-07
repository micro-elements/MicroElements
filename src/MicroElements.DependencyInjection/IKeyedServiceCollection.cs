// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroElements.DependencyInjection
{
    //ServiceName //see https://simpleinjector.readthedocs.io/en/latest/howto.html#resolve-instances-by-key
    //https://github.com/yuriy-nelipovich/DependencyInjection.Extensions/tree/master/Neleus.DependencyInjection.Extensions
    //Decorators
    //Metadata

    /// <summary>
    /// Collection of services that can be disambiguated by key
    /// </summary>
    public interface IKeyedServiceCollection<in TKey, out TService>
        where TService : class
    {
        TService GetService(IServiceProvider services, TKey key);
    }

    public static class KeyedServiceCollectionExtensions
    {
        /// <summary>
        /// Acquire a service by key.
        /// </summary>
        public static TService GetServiceByKey<TKey, TService>(this IServiceProvider services, TKey key)
            where TService : class
        {
            IKeyedServiceCollection<TKey, TService> collection = (IKeyedServiceCollection<TKey, TService>)services.GetService(typeof(IKeyedServiceCollection<TKey, TService>));
            return collection?.GetService(services, key);
        }

        /// <summary>
        /// Acquire a service by name.
        /// </summary>
        public static TService GetServiceByName<TService>(this IServiceProvider services, string name)
            where TService : class
        {
            return services.GetServiceByKey<string, TService>(name);
        }
    }

    public interface IKeyedService<TKey, out TService> : IEquatable<TKey>
    {
        TService GetService(IServiceProvider services);
    }

    public class KeyedService<TKey, TService, TInstance> : IKeyedService<TKey, TService>
        where TInstance : TService
    {
        private readonly TKey key;

        public KeyedService(TKey key)
        {
            this.key = key;
        }

        public TService GetService(IServiceProvider services) => services.GetService<TInstance>();

        public bool Equals(TKey other)
        {
            return Equals(key, other);
        }
    }

    public class KeyedServiceCollection<TKey, TService> : IKeyedServiceCollection<TKey, TService>
        where TService : class
    {
        public TService GetService(IServiceProvider services, TKey key)
        {
            IEnumerable<IKeyedService<TKey, TService>> keyedServices = services.GetServices<IKeyedService<TKey, TService>>();
            return keyedServices.FirstOrDefault(s => s.Equals(key))?.GetService(services);
        }
    }

    public static class TransientKeyedServiceExtensions
    {
        /// <summary>
        /// Register a transient keyed service
        /// </summary>
        public static void AddTransientKeyedService<TKey, TService, TInstance>(this IServiceCollection collection, TKey key)
            where TInstance : class, TService
        {
            collection.TryAddTransient<TInstance>();
            collection.AddSingleton<IKeyedService<TKey, TService>>(sp => new KeyedService<TKey, TService, TInstance>(key));
        }

        /// <summary>
        /// Register a transient named service
        /// </summary>
        public static void AddTransientNamedService<TService, TInstance>(this IServiceCollection collection, string key)
            where TInstance : class, TService
        {
            collection.AddTransientKeyedService<string, TService, TInstance>(key);
        }
    }
}
