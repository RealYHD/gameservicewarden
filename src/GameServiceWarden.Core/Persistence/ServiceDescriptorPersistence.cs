using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GameServiceWarden.Core.Module;
using GameServiceWarden.ModuleAPI;
using System.Collections.ObjectModel;

namespace GameServiceWarden.Core.Persistence
{
    public class ServiceDescriptorPersistence : IPersistent<IReadOnlyDictionary<string, string>>
    {
        private readonly string mapDirectory;
        private const string EXTENSION = ".sin";


        public ServiceDescriptorPersistence(string mapDirectory)
        {
            this.mapDirectory = mapDirectory;
        }

        public IReadOnlyDictionary<string, string> this[string key]
        {
            set
            {
                SaveService(key, value[ServiceDescriptor.ASSEMBLY_PROPERTY], value[ServiceDescriptor.MODULE_PROPERTY]);
            }
            get
            {
                if (!ContainsKey(key)) throw new KeyNotFoundException();
                Dictionary<string, string> info = new Dictionary<string, string>();
                info[ServiceDescriptor.ASSEMBLY_PROPERTY] = GetServiceInfoValue(key, ServiceDescriptor.ASSEMBLY_PROPERTY);
                info[ServiceDescriptor.MODULE_PROPERTY] = GetServiceInfoValue(key, ServiceDescriptor.MODULE_PROPERTY);
                return new ReadOnlyDictionary<string, string>(info);
            }
        }

        public string MapDirectory { get { return mapDirectory; } }



        public int Count
        {
            get
            {
                return Directory.GetFiles(mapDirectory).Length;
            }
        }

        public IEnumerable<IReadOnlyDictionary<string,string>> Values {
            get {
                IEnumerable<string> keys = Keys;
                List<IReadOnlyDictionary<string,string>> res = new List<IReadOnlyDictionary<string,string>>();
                foreach (string key in keys)
                {
                    res.Add(this[key]);
                }
                return res;
            }
        }

        public IEnumerable<string> Keys {
            get {
                IEnumerable<string> keys = Directory.EnumerateDirectories(mapDirectory);
                List<string> res = new List<string>();
                foreach (string key in keys)
                {
                    res.Add(Path.GetDirectoryName(key));
                }
                return res;
            }
        }

        public void AddToPersistence(string key, IReadOnlyDictionary<string,string> value)
        {
            if (key == null) throw new ArgumentNullException();
            if (ContainsKey(key)) throw new ArgumentException();
            this[key] = value;
        }

        public void Clear()
        {
            IEnumerable<string> directories = Keys;
            foreach (string directory in directories)
            {
                Directory.Delete(directory);
            }
        }

        public bool ContainsKey(string key)
        {
            return Directory.Exists(GetPathForKey(key));
        }

        public IEnumerator<KeyValuePair<string, IReadOnlyDictionary<string,string>>> GetEnumerator()
        {
            IEnumerable<string> keys = Keys;
            List<KeyValuePair<string, IReadOnlyDictionary<string,string>>> result = new List<KeyValuePair<string, IReadOnlyDictionary<string,string>>>();
            foreach (string key in keys)
            {
                result.Add(new KeyValuePair<string, IReadOnlyDictionary<string,string>>(key, this[key]));
            }
            return result.GetEnumerator();
        }

        public string GetPathForKey(string key)
        {
            return Path.Combine(mapDirectory, key);
        }

        public bool Delete(string key)
        {
            if (!ContainsKey(key)) return false;
            try
            {
                Directory.Delete(GetPathForKey(key));
            }
            catch (System.Exception)
            {
                return false;
                throw;
            }
            return true;
        }

        public bool TryLoadValue(string key, [MaybeNullWhen(false)] out IReadOnlyDictionary<string,string> value)
        {
            try {
                value = this[key];
            } catch (KeyNotFoundException) {
                value = null;
                return false;
            }
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SaveService(string key, string assemblyName, string moduleName)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (assemblyName == null) throw new ArgumentNullException("assemblyName");
            if (moduleName == null) throw new ArgumentNullException("moduleName");
            string serviceInfoPath = GetPathForKey(key);
            Directory.CreateDirectory(serviceInfoPath);
            using (StreamWriter writer = File.CreateText(Path.Combine(serviceInfoPath, key + EXTENSION)))
            {
                writer.WriteLine($"{ServiceDescriptor.ASSEMBLY_PROPERTY}: {assemblyName}");
                writer.WriteLine($"{ServiceDescriptor.MODULE_PROPERTY}: {moduleName}");
            }
        }

        private string GetServiceInfoValue(string key, string value)
        {
            string path = GetPathForKey(key);
            IEnumerable<string> lines = File.ReadAllLines(Path.Combine(path, key + EXTENSION));
            foreach (string line in lines)
            {
                if (line.StartsWith($"{value}: "))
                {
                    return line.Substring(value.Length + 2);
                }
            }

            throw new FormatException($"\"{path}\" is corrupted. Could not find value for: {value}.");
        }
    }
}