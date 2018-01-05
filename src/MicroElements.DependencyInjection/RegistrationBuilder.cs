using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroElements.DependencyInjection
{
    public class RegistrationBuilder
    {
        private readonly IServiceCollection _services;

        private ServiceLifetime _lifetime = ServiceLifetime.Singleton;
        private Type[] _serviceTypes;
        private readonly Type _implementationType;
        private object _implementationInstance;
        private Func<IServiceProvider, object> _implementationFactory;


        public RegistrationBuilder(IServiceCollection services, Type implementationType)
        {
            _services = services;
            _implementationType = implementationType;
            _serviceTypes = new[] {implementationType};
        }

        public RegistrationBuilder(IServiceCollection services, object implementationInstance)
        {
            _services = services;
            _implementationType = implementationInstance.GetType();
            _serviceTypes = new[] { _implementationType };
            _implementationInstance = implementationInstance;
        }

        public RegistrationBuilder(IServiceCollection services, Type implementationType, Func<IServiceProvider, object> implementationFactory)
        {
            _services = services;
            _implementationType = implementationType;
            _serviceTypes = new[] { _implementationType };
            _implementationFactory = implementationFactory;
        }

        public RegistrationBuilder As<T>()
        {
            _serviceTypes = new []{ typeof(T) };
            return this;
        }

        public RegistrationBuilder AsSelf()
        {
            _serviceTypes = new[] { _implementationType };
            return this;
        }

        public RegistrationBuilder SingleInstance()
        {
            _lifetime = ServiceLifetime.Singleton;
            return this;
        }

        public RegistrationBuilder Scoped()
        {
            _lifetime = ServiceLifetime.Scoped;
            return this;
        }

        public RegistrationBuilder Transient()
        {
            _lifetime = ServiceLifetime.Transient;
            return this;
        }

        public RegistrationBuilder AsImplementedInterfaces()
        {
            _serviceTypes = GetImplementedInterfaces(_implementationType);
            return this;
        }

        public RegistrationBuilder As(Type[] services)
        {
            _serviceTypes = services;
            return this;
        }

        private static Type[] GetImplementedInterfaces(Type type)
        {
            var types = type.GetTypeInfo().ImplementedInterfaces.Where(t => t != typeof(IDisposable));
            if (!type.GetTypeInfo().IsInterface)
                return types.ToArray();
            return types.Concat(new []{ type }).ToArray();
        }

        public RegistrationBuilder WithMetadata(string registerAttributeMetadataName, object registerAttributeMetadataValue)
        {
            return this;
        }

        public RegistrationBuilder Named(string name)
        {

            return this;
        }

        public void Add()
        {
            foreach (var serviceType in _serviceTypes)
            {
                _services.Add(new ServiceDescriptor(serviceType, _implementationType, _lifetime));
            }
        }

        public void TryAdd()
        {
            foreach (var serviceType in _serviceTypes)
            {
                _services.TryAdd(new ServiceDescriptor(serviceType, _implementationType, _lifetime));
            }
        }
    }

    public static class RegistrationBuilderExt
    {
        public static RegistrationBuilder RegisterType(this IServiceCollection services, Type implementationType)
        {
            return new RegistrationBuilder(services, implementationType);
        }

        public static RegistrationBuilder RegisterInstance(this IServiceCollection services, object implementationInstance)
        {
            return new RegistrationBuilder(services, implementationInstance);
        }

        public static RegistrationBuilder RegisterFactory(this IServiceCollection services, Type implementationType, Func<IServiceProvider, object> implementationFactory)
        {
            return new RegistrationBuilder(services, implementationType, implementationFactory);
        }
    }
}