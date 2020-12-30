using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GameServiceWarden.Core.Persistence;

namespace GameServiceWarden.Core.Tests.Modules
{
    public class FakePersistentDictionary<V> : IPersistentDictionary<V>, IReadOnlyPersistentDictionary<V>
    {
        private IDictionary<string, V> backing = new Dictionary<string, V>();
        public V this[string key] { get { return backing[key]; } set { backing[key] = value; } }

        public string MapDirectory { get { return "fake/directory/"; } }

        public ICollection<string> Keys { get { return backing.Keys; } }

        public ICollection<V> Values { get { return backing.Values; } }

        public int Count { get { return backing.Count; } }

        public bool IsReadOnly { get { return false; } }

        IEnumerable<string> IReadOnlyDictionary<string, V>.Keys { get { return backing.Keys; } }

        IEnumerable<V> IReadOnlyDictionary<string, V>.Values { get { return backing.Values; } }

        public void Add(string key, V value)
        {
            backing.Add(key, value);
        }

        public void Add(KeyValuePair<string, V> item)
        {
            backing.Add(item);
        }

        public void Clear()
        {
            backing.Clear();
        }

        public bool Contains(KeyValuePair<string, V> item)
        {
            return backing.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return backing.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, V>[] array, int arrayIndex)
        {
            backing.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, V>> GetEnumerator()
        {
            return backing.GetEnumerator();
        }

        public string GetPathForKey(string key)
        {
            return MapDirectory + Path.DirectorySeparatorChar + key;
        }

        public bool Remove(string key)
        {
            return backing.Remove(key);
        }

        public bool Remove(KeyValuePair<string, V> item)
        {
            return backing.Remove(item);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out V value)
        {
            return backing.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return backing.GetEnumerator();
        }
    }
}