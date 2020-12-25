using System;
using System.Collections.Generic;
using System.Reflection;
using GameServiceWarden.Host.Modules.Exceptions;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Host.Modules
{
    public class ModuleLoader //Gateway
    {
        /// <summary>
        /// Loads an extension module.
        /// </summary>
        /// <param name="path">The path to the module.</param>
        /// <returns>An </<see cref="IEnumerable{IGameServiceModule}"/> from the given module.</returns>
        /// <exception cref="NoServiceableFoundException">When the module requested to be loaded does not contain any public <see cref="IGameServiceable"/> classes.</exception>
        public IEnumerable<IGameServiceModule> LoadModules(string path)
        {
            return instantiateServiceable(loadAssembly(path));
        }

        /// <summary>
        /// Loads all module for each given path to modules file.
        /// </summary>
        /// <param name="paths">The paths to load modules for.</param>
        /// <returns>A <see cref="Dictionary{string, IEnumerable{IGameServiceModule}}"/> where the key is a <see cref="string"/> that is the associated path.</returns>
        public Dictionary<string, IEnumerable<IGameServiceModule>> LoadAllModules(IEnumerable<string> paths)
        {
            Dictionary<string, IEnumerable<IGameServiceModule>> res = new Dictionary<string, IEnumerable<IGameServiceModule>>();
            foreach (string path in paths)
            {
                res.Add(path, LoadModules(path));
            }
            return res;
        }

        private Assembly loadAssembly(string path)
        {
            ModuleLoadContext moduleLoadContext = new ModuleLoadContext(path);
            return moduleLoadContext.LoadFromAssemblyPath(path);
        }

        private IEnumerable<IGameServiceModule> instantiateServiceable(Assembly assembly)
        {
            int serviceableCount = 0;
            foreach (Type type in assembly.GetExportedTypes())
            {
                if (typeof(IGameServiceModule).IsAssignableFrom(type))
                {
                    IGameServiceModule res = Activator.CreateInstance(type) as IGameServiceModule;
                    if (res != null)
                    {
                        serviceableCount++;
                        yield return res;
                    }
                }
            }

            if (serviceableCount == 0)
            {
                List<string> typeNames = new List<string>();
                foreach (Type type in assembly.GetExportedTypes())
                {
                    typeNames.Add(type.FullName);
                }
                string types = String.Join(',', typeNames);

                throw new NoServiceableFoundException(
                    $"No public classes in {assembly} from {assembly.Location} implemented {typeof(IGameService).FullName}." +
                    $"Detected types: {types}");
            }
        }
    }
}