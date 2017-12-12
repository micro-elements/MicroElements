using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MicroComponents.Tests
{
    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly StringBuilder _log = new StringBuilder();

        public string Log => _log.ToString();

        public void AppendLine(string logLine)
        {
            _log.AppendLine(logLine);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(this);
        }

        public void Dispose() { }
    }

    class TestLogger : ILogger
    {
        private readonly TestLoggerProvider _loggerProvider;

        public TestLogger(TestLoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;
        }

        string Render<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            return state.ToString();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var render = Render(logLevel, eventId, state, exception, formatter);
            _loggerProvider.AppendLine(render);
            Console.WriteLine(render);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}
