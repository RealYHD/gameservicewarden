using System.Collections.Generic;

namespace GameServiceWarden.Core.Persistence
{
    public interface IPersistentDictionary<V> : IDictionary<string, V>
    {
        /// <summary>
        /// The directory for this dictionary to use.
        /// </summary>
        string MapDirectory { get; }

        /// <summary>
        /// The path to the data representing a specific key.
        /// </summary>
        /// <param name="key">The key to get the path for.</param>
        /// <returns>A <see cref="string"/> representing the key.</returns>
        string GetPathForKey(string key);
    }
}