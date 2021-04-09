using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServiceWarden.Core.Logging
{
    public static class Logger {
        private static readonly ConcurrentDictionary<string, ILogReceiver> listeners = new ConcurrentDictionary<string, ILogReceiver>();

        /// <summary>
        /// Logs the message to listeners that are listening to the set severity of the message or greater.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The level of severity, by default, info.</param>
        public static void Log(string message, LogLevel level = LogLevel.INFO) {
            foreach (ILogReceiver listener in listeners.Values)
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
        public static void AddLogListener(ILogReceiver listener) {
            listeners[listener.Identifier] = listener;
        }

        /// <summary>
        /// Removes a log listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public static void RemoveLogListener(ILogReceiver listener) {
            ILogReceiver receiver;
            listeners.TryRemove(listener.Identifier, out receiver);
        }

        /// <summary>
        /// Called when all listeners should perform any flushing they need.
        /// </summary>
        public static void FlushListeners()
        {
            foreach (ILogReceiver listener in listeners.Values)
            {
                listener.Flush();
            }
        }
    }
}