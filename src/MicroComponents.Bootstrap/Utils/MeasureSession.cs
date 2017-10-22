using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace MicroComponents.Bootstrap.Utils
{
    /// <summary>
    /// Сессия записи измерений.
    /// </summary>
    public class MeasureSession
    {
        private readonly string _sessionName;
        private readonly List<Measure> _measures = new List<Measure>();

        /// <summary>
        /// Создание сессии измерения.
        /// </summary>
        /// <param name="sessionName">Имя сессии.</param>
        public MeasureSession(string sessionName)
        {
            _sessionName = sessionName;
        }

        /// <summary>
        /// Имя сессии измерения.
        /// </summary>
        public string SessionName => _sessionName;

        /// <summary>
        /// Список измерений.
        /// </summary>
        public Measure[] Measures => _measures.ToArray();

        class MeasureDisp : IDisposable
        {
            private string _name;
            private DateTime _startedTime;
            private Stopwatch _stopwatch;
            private MeasureSession _session;

            public MeasureDisp(string name, MeasureSession session)
            {
                _name = name;
                _session = session;
                _startedTime = DateTime.UtcNow;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _session.Add(new Measure(_name, _startedTime, _stopwatch.Elapsed));
            }
        }

        private void Add(Measure measure)
        {
            _measures.Add(measure);
        }

        public IDisposable StartTimer(string timerName)
        {
            return new MeasureDisp(timerName, this);
        }

        /// <summary>
        /// Запуск выполнения с подстчетом времени.
        /// Все временные мерки сохраняются во внутреннем списке.
        /// </summary>
        /// <param name="timerName">Имя измерения.</param>
        /// <param name="action">Действие.</param>
        public void ExecuteWithTimer(string timerName, Action action)
        {
            var duration = Stopwatch.StartNew();
            var startedAt = DateTime.UtcNow;
            try
            {
                action();
            }
            finally
            {
                _measures.Add(new Measure(timerName, startedAt, duration.Elapsed));
            }
        }

        /// <summary>
        /// Вывод в лог измерений в читаемом виде.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        public void LogMeasures(ILogger logger)
        {
            var total = new TimeSpan(_measures.Sum(tuple => tuple.Duration.Ticks));
            var filler1 = "└──";
            var filler2 = "   ├──";
            var filler3 = "   └──";
            int maxLen = Math.Max(SessionName.Length + filler1.Length, _measures.Max(measure => measure.Name.Length) + filler2.Length);

            logger.LogInformation($"{filler1}{SessionName.PadRight(maxLen - filler1.Length)} : {total}");
            for (var index = 0; index < _measures.Count; index++)
            {
                var measure = _measures[index];
                var filler = index < _measures.Count - 1 ? filler2 : filler3;
                logger.LogInformation($"{filler}{measure.Name.PadRight(maxLen - filler2.Length)} : {measure.Duration}");
            }
        }
    }
}