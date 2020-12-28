using System;

namespace GameServiceWarden.Core.Logging
{
    public interface ILogReceiver
    {
        /// <summary>
        /// The severity of the messages this log should receive.
        /// </summary>
        /// <value>The severity of the logs.</value>
        LogLevel Level { get; }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="time">The time at which this message was requested to be logged.</param>
        /// <param name="level">The severity of this message.</param>
        void LogMessage(string message, DateTime time, LogLevel level);

        /// <summary>
        /// Called when this receiver should explicitly flush received messages.
        /// </summary>
        void Flush();
    }
}