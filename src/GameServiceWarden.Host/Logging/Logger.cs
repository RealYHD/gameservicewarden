using System;
using System.Collections.Generic;

namespace GameServiceWarden.Host.Logging
{
    public class Logger {
        private HashSet<ILogReceiver> listeners = new HashSet<ILogReceiver>();

        /// <summary>
        /// Logs the message to listeners that are listening to the set severity of the message or greater.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The level of severity, by default, info.</param>
        public void Log(string message, LogLevel level = LogLevel.INFO) {
            foreach (ILogReceiver listener in listeners)
            {
                if (level <= listener.Level) {
                    listener.LogMessage(message, DateTime.Now, level);
                }
            }
        }

        /// <summary>
        /// Adds a log listener.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddLogListener(ILogReceiver listener) {
            listeners.Add(listener);
        }

        /// <summary>
        /// Removes a log listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public void RemoveLogListener(ILogReceiver listener) {
            listeners.Remove(listener);
        }

        /// <summary>
        /// Called when all listeners should perform any flushing they need.
        /// </summary>
        public static void FlushListeners() {

        }
    }
}