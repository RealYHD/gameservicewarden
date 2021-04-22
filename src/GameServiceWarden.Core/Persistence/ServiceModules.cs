using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GameServiceWarden.Core.Module;
using GameServiceWarden.ModuleFramework;

namespace GameServiceWarden.Core.Persistence
{
    public class ServiceModules : IReadOnlyPersistent<IReadOnlyDictionary<string, IServiceModule>>
    {
        private readonly string mapDirectory;

        private readonly ModuleLoader loader = new ModuleLoader();

        public ServiceModules(string mapDirectory)
        {
            this.mapDirectory = mapDirectory;
        }

        public IReadOnlyDictionary<string, IServiceModule> this[string key]
        {
            get
            {
                if (!ContainsKey(key)) throw new KeyNotFoundException($"Key \"{key}\" not found.");
                Dictionary<string, IServiceModule> res = new Dictionary<string, IServiceModule>();
                IEnumerable<IServiceModule> modules = loader.LoadModules(GetPathForKey(key));
                foreach (IServiceModule module in modules)
                {
                    res.Add(module.Name, module);
                }
                return new ReadOnlyDictionary<string, IServiceModule>(res);
            }
        }

        public string MapDirectory
        {
            get
            {
                return mapDirectory;
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                IEnumerable<string> files = Directory.EnumerateFiles(mapDirectory);
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower().Equals("dll"))
                    {
                        yield return Path.GetFileName(file);
                    }
                }
            }
        }

        public IEnumerable<IReadOnlyDictionary<string, IServiceModule>> Values
        {
            get
            {
                IEnumerable<string> keys = Keys;
                foreach (string key in keys)
                {
                    yield return this[key];
                }
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                IEnumerable<string> files = Directory.EnumerateFiles(mapDirectory);
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower().Equals("dll"))
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public bool ContainsKey(string key)
        {
            string path = GetPathForKey(key);
            return File.Exists(path) && Path.GetExtension(path).ToLower().Equals("dll");
        }

        public IEnumerator<KeyValuePair<string, IReadOnlyDictionary<string, IServiceModule>>> GetEnumerator()
        {
            IEnumerable<string> keys = Keys;
            foreach (string key in keys)
            {
                yield return new KeyValuePair<string, IReadOnlyDictionary<string, IServiceModule>>(key, this[key]);
            }
        }

        public string GetPathForKey(string key)
        {
            return mapDirectory + Path.DirectorySeparatorChar + key;
        }

        public bool TryLoadValue(string key, [MaybeNullWhen(false)] out IReadOnlyDictionary<string, IServiceModule> value)
        {
            try
            {
                value = this[key];
                return true;
            }
            catch (KeyNotFoundException)
            {
                value = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}