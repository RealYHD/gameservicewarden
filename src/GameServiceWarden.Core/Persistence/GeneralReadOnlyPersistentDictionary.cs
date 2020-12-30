using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace GameServiceWarden.Core.Persistence
{
    public class GeneralReadOnlyPersistentDictionary<V> : IReadOnlyPersistentDictionary<V>, IReadOnlyCollection<KeyValuePair<string, V>>
    {
        private readonly string mapDirectory;
        public string MapDirectory { get { return mapDirectory; } }
        private IPersistentDictionaryReader<V> reader;

        public GeneralReadOnlyPersistentDictionary(string mapDirectory, IPersistentDictionaryReader<V> reader)
        {
            this.mapDirectory = mapDirectory;
            this.reader = reader;
        }

        public V this[string key]
        {
            get
            {
                return reader.Read(GetPathForKey(key));
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                IEnumerable<string> keys = Directory.EnumerateFileSystemEntries(mapDirectory);
                foreach (string key in keys)
                {
                    string path = GetPathForKey(key);
                    if ((File.Exists(path) || Directory.Exists(path)) && reader.IsValidValue(path)) yield return key;
                }
            }
        }

        public IEnumerable<V> Values
        {
            get
            {
                IEnumerable<string> keys = Directory.EnumerateFileSystemEntries(mapDirectory);
                foreach (string key in keys)
                {
                    yield return reader.Read(GetPathForKey(key));
                }
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                IEnumerable<string> keys = Directory.EnumerateFileSystemEntries(mapDirectory);
                foreach (string key in keys)
                {
                    string path = GetPathForKey(key);
                    if ((File.Exists(path) || Directory.Exists(path)) && reader.IsValidValue(path)) count++;
                }
                return count;
            }
        }

        public bool ContainsKey(string key)
        {
            if (key == null) throw new ArgumentNullException(key);
            string path = GetPathForKey(key);
            return (File.Exists(path) || Directory.Exists(path)) && reader.IsValidValue(path);
        }

        public IEnumerator<KeyValuePair<string, V>> GetEnumerator()
        {
            IEnumerable<string> keys = Keys;
            List<KeyValuePair<string, V>> pairs = new List<KeyValuePair<string, V>>();
            foreach (string key in keys)
            {
                pairs.Add(new KeyValuePair<string, V>(key, this[key]));
            }
            return pairs.GetEnumerator();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out V value)
        {
            string path = GetPathForKey(key);
            if (!File.Exists(path) || !Directory.Exists(path) || !reader.IsValidValue(path))
            {
                value = default(V);
                return false;
            }
            value = reader.Read(path);
            return true;
        }

        public string GetPathForKey(string key)
        {
            return mapDirectory + Path.DirectorySeparatorChar + key;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}