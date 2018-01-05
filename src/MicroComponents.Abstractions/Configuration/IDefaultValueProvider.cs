namespace MicroComponents.Configuration
{
    /// <summary>
    /// Провайдер значения по-умолчанию для типа.
    /// </summary>
    /// <typeparam name="T">Тип.</typeparam>
    public interface IDefaultValueProvider<T>
    {
        /// <summary>
        /// Получение значения по-умолчанию.
        /// </summary>
        /// <returns>Значение по-умолчанию.</returns>
        T GetDefault();
    }
}