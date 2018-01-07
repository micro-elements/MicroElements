// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Configuration
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