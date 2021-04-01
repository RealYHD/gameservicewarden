using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GameServiceWarden.Core.Games.Modules;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Core.Persistence
{
    public class GameServiceModules : IReadOnlyPersistent<IReadOnlyDictionary<string, IGameServiceModule>>
    {
        private readonly string mapDirectory;

        private readonly GameServiceModuleLoader loader = new GameServiceModuleLoader();

        public GameServiceModules(string mapDirectory)
        {
            this.mapDirectory = mapDirectory;
        }

        public IReadOnlyDictionary<string, IGameServiceModule> this[string key]
        {
            get
            {
                if (!ContainsKey(key)) throw new KeyNotFoundException($"Key \"{key}\" not found.");
                Dictionary<string, IGameServiceModule> res = new Dictionary<string, IGameServiceModule>();
                IEnumerable<IGameServiceModule> modules = loader.LoadModules(GetPathForKey(key));
                foreach (IGameServiceModule module in modules)
                {
                    res.Add(module.Name, module);
                }
                return new ReadOnlyDictionary<string, IGameServiceModule>(res);
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

        public IEnumerable<IReadOnlyDictionary<string, IGameServiceModule>> Values
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

        public IEnumerator<KeyValuePair<string, IReadOnlyDictionary<string, IGameServiceModule>>> GetEnumerator()
        {
            IEnumerable<string> keys = Keys;
            foreach (string key in keys)
            {
                yield return new KeyValuePair<string, IReadOnlyDictionary<string, IGameServiceModule>>(key, this[key]);
            }
        }

        public string GetPathForKey(string key)
        {
            return mapDirectory + Path.DirectorySeparatorChar + key;
        }

        public bool TryLoadValue(string key, [MaybeNullWhen(false)] out IReadOnlyDictionary<string, IGameServiceModule> value)
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