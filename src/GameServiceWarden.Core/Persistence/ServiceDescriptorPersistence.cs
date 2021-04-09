using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GameServiceWarden.Core.Games;
using GameServiceWarden.API.Module;

namespace GameServiceWarden.Core.Persistence
{
    public class ServiceDescriptorPersistence : IPersistent<ServiceDescriptor>
    {
        private readonly IReadOnlyPersistent<IReadOnlyDictionary<string, IServiceModule>> modules;
        private readonly string mapDirectory;
        private const string ASSEMBLY_NAME = "Assembly Name";
        private const string MODULE_NAME = "Module Name";
        private const string EXTENSION = ".sin";


        public ServiceDescriptorPersistence(string mapDirectory, IReadOnlyPersistent<IReadOnlyDictionary<string, IServiceModule>> modules)
        {
            this.mapDirectory = mapDirectory;
            this.modules = modules;
        }

        public ServiceDescriptor this[string key]
        {
            set
            {
                SaveService(key, value.GetAssemblyName(), value.GetModuleName());
            }
            get
            {
                if (!ContainsKey(key)) throw new KeyNotFoundException();
                string assemblyName = GetServiceInfoValue(key, ASSEMBLY_NAME);
                string moduleName = GetServiceInfoValue(key, MODULE_NAME);
                IService service = modules[assemblyName][moduleName].InstantiateService(GetPathForKey(key), false);
                return new ServiceDescriptor(service, key, moduleName, assemblyName);
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

        public IEnumerable<ServiceDescriptor> Values {
            get {
                IEnumerable<string> keys = Keys;
                List<ServiceDescriptor> res = new List<ServiceDescriptor>();
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

        public void AddToPersistence(string key, ServiceDescriptor value)
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

        public IEnumerator<KeyValuePair<string, ServiceDescriptor>> GetEnumerator()
        {
            IEnumerable<string> keys = Keys;
            List<KeyValuePair<string, ServiceDescriptor>> result = new List<KeyValuePair<string, ServiceDescriptor>>();
            foreach (string key in keys)
            {
                result.Add(new KeyValuePair<string, ServiceDescriptor>(key, this[key]));
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

        public bool TryLoadValue(string key, [MaybeNullWhen(false)] out ServiceDescriptor value)
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
                writer.WriteLine($"{ASSEMBLY_NAME}: {assemblyName}");
                writer.WriteLine($"{MODULE_NAME}: {moduleName}");
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