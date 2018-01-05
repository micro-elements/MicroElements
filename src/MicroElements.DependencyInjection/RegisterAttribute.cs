using System;

namespace MicroElements.DependencyInjection
{
    /// <summary>
    /// Атрибут регистрации какого-либо класса в контейнере.
    /// </summary>
    public class RegisterAttribute : Attribute
    {
        /// <summary>
        /// Типы, в качестве которых необходимо зарегистрировать зависимость.
        /// </summary>
        public Type[] Services { get; set; }

        /// <summary>
        /// Регистрируется именованная сущность.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Компонент регистрируется как Singleton.
        /// </summary>
        public bool Singleton { get; set; } = true;

        /// <summary>
        /// Метаданные для регистрации.
        /// </summary>
        public string MetadataName { get; set; }

        /// <summary>
        /// Метаданные для регистрации.
        /// </summary>
        public object MetadataValue { get; set; }

        /// <summary>
        /// Создает новый атрибут регистрации зависимости.
        /// </summary>
        public RegisterAttribute()
        {
        }

        /// <summary>
        /// Создает новый атрибут регистрации зависимости.
        /// </summary>
        /// <param name="services">Типы, в качестве которых необходимо зарегистрировать зависимость.
        ///  Если типы не указаны, то компонент будет зарегистрирован как AsImplementedInterfaces.</param>
        public RegisterAttribute(params Type[] services)
        {
            Services = services ?? new Type[0];
        }
    }
}