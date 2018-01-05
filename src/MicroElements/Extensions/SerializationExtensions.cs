using Newtonsoft.Json;

namespace MicroElements.Bootstrap.Extensions
{
    /// <summary>
    /// Расширения для сериализации.
    /// </summary>
    internal static class SerializationExtensions
    {
        /// <summary>
        /// Клонирование объекта с использованием сериализации и десериализации.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="obj">Объект, который нужно склонировать.</param>
        /// <returns>Клон исходного объекта</returns>
        internal static T CloneWithJson<T>(this T obj)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
        }
    }
}