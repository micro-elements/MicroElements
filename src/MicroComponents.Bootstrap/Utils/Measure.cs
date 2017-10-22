using System;

namespace MicroComponents.Bootstrap.Utils
{
    /// <summary>
    /// Измерение.
    /// </summary>
    public class Measure
    {
        /// <summary>
        /// Имя измерения.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Дата время начала.
        /// </summary>
        public DateTime StartTimeUtc { get; private set; }

        /// <summary>
        /// Длительность.
        /// </summary>
        public TimeSpan Duration { get; private set; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="name">Имя измерения.</param>
        /// <param name="startTimeUtc">Время старта.</param>
        /// <param name="duration">Длительность.</param>
        public Measure(string name, DateTime startTimeUtc, TimeSpan duration)
        {
            Name = name;
            StartTimeUtc = startTimeUtc;
            Duration = duration;
        }
    }
}