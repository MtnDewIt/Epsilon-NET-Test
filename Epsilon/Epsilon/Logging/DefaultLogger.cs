using EpsilonLib.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace Epsilon.Logging
{
    class DefaultLogger : ILogHandler
    {
        private StreamWriter _writer;

        public DefaultLogger()
        {
            OpenLogForWriting();
        }

        public void Log(LogMessageType type, string message)
        {
            var line = $"[{type.ToString().ToUpper()}]: {message}";
            Debug.WriteLine(line);
            _writer.WriteLine(line);
            _writer.Flush();
        }

        private void OpenLogForWriting()
        {
            string logsDir = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "logs")).FullName;
            string suffix = "";
            int attempt = 0;
            const int maxAttempts = 10;

            while (true)
            {
                try
                {

                    string path = Path.Combine(logsDir, $"epsilon{suffix}.log");
                    _writer = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read));

                    break;
                }
                catch (IOException ex) when ((uint)ex.HResult == 0x80070020) // ERROR_SHARING_VIOLATION 
                {
                    if (++attempt > maxAttempts)
                        throw new IOException($"Failed to open log file after {maxAttempts} attempts", ex);

                    suffix = $"({attempt})";
                }
            }
        }
    }
}
