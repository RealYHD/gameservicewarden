using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GameServiceWarden.Core.Games;
using GameServiceWarden.Core.Persistence;

namespace GameServiceWarden.Core.Tests.Modules
{
    public class FakePersistence<V> : IPersistent<V>
    {
        private IDictionary<string, V> backing = new Dictionary<string, V>();
        public V this[string key] { get { return backing[key]; } set { backing[key] = value; } }

        public string MapDirectory { get { return "fake/directory/"; } }

        public ICollection<string> Keys { get { return backing.Keys; } }

        public ICollection<V> Values { get { return backing.Values; } }

        public int Count { get { return backing.Count; } }

        public bool IsReadOnly { get { return false; } }

        IEnumerable<V> IReadOnlyPersistent<V>.Values => backing.Values;

        IEnumerable<string> IReadOnlyPersistent<V>.Keys => backing.Keys;

        public void AddToPersistence(string key, V value)
        {
            backing.Add(key, value);
        }

        public void Add(KeyValuePair<string, V> item)
        {
            backing.Add(item);
        }

        public void Add(string key, GameServiceInfo value)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            backing.Clear();
        }

        public bool ContainsKey(string key)
        {
            return backing.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, V>> GetEnumerator()
        {
            return backing.GetEnumerator();
        }

        public string GetPathForKey(string key)
        {
            return MapDirectory + Path.DirectorySeparatorChar + key;
        }

        public bool Delete(string key)
        {
            return backing.Remove(key);
        }

        public bool Remove(KeyValuePair<string, V> item)
        {
            return backing.Remove(item);
        }

        public bool TryLoadValue(string key, [MaybeNullWhen(false)] out V value)
        {
            return backing.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return backing.GetEnumerator();
        }
    }
}