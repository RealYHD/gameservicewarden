using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GameServiceWarden.ModuleFramework;

namespace GameServiceWarden.Core.Persistence
{
    public interface IReadOnlyPersistent<V> : IEnumerable<KeyValuePair<string, V>>
    {
        V this[string key] { get; }

        /// <summary>
        /// The directory for this dictionary to use.
        /// </summary>
        string MapDirectory { get; }
        int Count { get; }
        IEnumerable<V> Values { get; }
        IEnumerable<string> Keys { get; }

        bool ContainsKey(string key);

        /// <summary>
        /// The path to the data representing a specific key.
        /// </summary>
        /// <param name="key">The key to get the path for.</param>
        /// <returns>A <see cref="string"/> representing the key.</returns>
        string GetPathForKey(string key);
        bool TryLoadValue(string key, [MaybeNullWhen(false)] out V value);
    }
}