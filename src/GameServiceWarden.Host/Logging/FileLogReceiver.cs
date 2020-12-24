using System;

namespace GameServiceWarden.Host.Logging
{
    public class FileLogReceiver : ILogReceiver
    {
        public LogLevel Level => LogLevel.INFO;

        public void LogMessage(string message, DateTime time, LogLevel level)
        {
            
        }
    }
}