using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace GameServiceWarden.Core.Persistence
{
    public class GeneralPersistentDictionary<V> : IPersistentDictionary<V>, ICollection<KeyValuePair<string, V>>
    {
        private readonly string mapDirectory;
        private readonly IPersistentDictionaryWriter<V> writer;
        private readonly IPersistentDictionaryReader<V> reader;
        private readonly GeneralReadOnlyPersistentDictionary<V> readDictionary;

        public GeneralPersistentDictionary(string mapDirectory, IPersistentDictionaryReader<V> reader, IPersistentDictionaryWriter<V> writer)
        {
            this.mapDirectory = mapDirectory ?? throw new ArgumentNullException("mapDirectory");
            this.reader = reader ?? throw new ArgumentNullException("reader");
            this.readDictionary = new GeneralReadOnlyPersistentDictionary<V>(this.mapDirectory, reader);
            this.writer = writer ?? throw new ArgumentNullException("writer");
        }

        public V this[string key] { get { return readDictionary[key]; } set { writer.Write(GetPathForKey(key), value); } }

        public ICollection<string> Keys
        {
            get
            {
                IEnumerable<string> keys = Directory.EnumerateFileSystemEntries(mapDirectory);
                List<string> res = new List<string>();
                foreach (string key in keys)
                {
                    if (reader.IsValidValue(GetPathForKey(key))) res.Add(key);
                }
                return res;
            }
        }

        public ICollection<V> Values
        {
            get
            {
                IEnumerable<string> keys = Keys;
                List<V> res = new List<V>();
                foreach (string key in keys)
                {
                    res.Add(reader.Read(GetPathForKey(key)));
                }
                return res;
            }
        }

        public int Count {get { return readDictionary.Count; } }

        public bool IsReadOnly {get { return false; } }

        public string MapDirectory { get { return mapDirectory; } }

        public void Add(string key, V value)
        {
            string path = GetPathForKey(key);
            if (key == null) throw new ArgumentNullException("key");
            if (File.Exists(path) || Directory.Exists(path))
            {
                if (reader.IsValidValue(path)) throw new ArgumentException($"Key \"{key}\" already exists.");
                throw new ArgumentException($"Corrupt item at \"{path}\" associated with key \"{key}\".");
            }
            writer.Write(GetPathForKey(key), value);
        }

        void ICollection<KeyValuePair<string,V>>.Add(KeyValuePair<string, V> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            IEnumerable<string> keys = Keys;
            foreach (string key in keys)
            {
                string path = GetPathForKey(key);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else
                {
                    Directory.Delete(path);
                }
            }
        }

        bool ICollection<KeyValuePair<string, V>>.Contains(KeyValuePair<string, V> item)
        {
            string path = GetPathForKey(item.Key);
            return ContainsKey(item.Key) && readDictionary[item.Key].Equals(item.Value);
        }

        public bool ContainsKey(string key)
        {
            return readDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, V>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("arrayIndex");
            if (array.Length - arrayIndex < Count) throw new ArgumentException($"The number of elements in this dictionary ({Count}) is greater than the space from arrayIndex to the end of the array ({array.Length - arrayIndex}).");
            IEnumerator<KeyValuePair<string, V>> pairs = GetEnumerator();
            for (int i = arrayIndex; i < array.Length; i++)
            {
                if (!pairs.MoveNext()) return;
                array[i] = pairs.Current;
            }
        }

        public IEnumerator<KeyValuePair<string, V>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, V>>) readDictionary).GetEnumerator();
        }

        public bool Remove(string key)
        {
            if (key == null) throw new ArgumentNullException("key");

            if (!ContainsKey(key)) return false;
            string path = GetPathForKey(key);
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
                return true;
            }
            File.Delete(path);
            return true;
        }

        public bool Remove(KeyValuePair<string, V> item)
        {
            return Remove(item.Key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out V value)
        {
            return readDictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) readDictionary).GetEnumerator();
        }

        public string GetPathForKey(string key)
        {
            return mapDirectory + Path.DirectorySeparatorChar + key;
        }
    }
}