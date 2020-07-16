using System.Collections.Generic;

namespace EpsilonLib.Logging
{
    public static class Logger
    {
        private volatile static IList<ILogHandler> _loggers = new List<ILogHandler>();

        public static void Trace(string message) => Log(LogMessageType.Trace, message);
        public static void Info(string message) => Log(LogMessageType.Info, message);
        public static void Warn(string message) => Log(LogMessageType.Warning, message);
        public static void Error(string message) => Log(LogMessageType.Error, message);

        public static void RegisterLogger(ILogHandler logger)
        {
            lock (_loggers)
            {
                if (_loggers.Contains(logger))
                    return;

                _loggers.Add(logger);
            }
        }

        private static void Log(LogMessageType type, string message)
        {
            lock(_loggers) 
            {
                foreach (var logger in _loggers)
                    logger.Log(type, message);
            }
        }
    }
}
