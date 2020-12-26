using System;
using System.Collections.Generic;
using System.IO;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Host.Modules
{
    public class ServiceManager
    {
        private readonly Dictionary<string, ServiceInfo> services = new Dictionary<string, ServiceInfo>();
        private readonly Dictionary<string, Dictionary<string, IGameServiceModule>> modules = new Dictionary<string, Dictionary<string, IGameServiceModule>>();

        public void AddModule(string assemblyName, IGameServiceModule module)
        {
            if (!modules.ContainsKey(assemblyName)) modules.Add(assemblyName, new Dictionary<string, IGameServiceModule>());
            modules[assemblyName][module.Name] = module;
        }

        public void RemoveModule(string assemblyName, string moduleName)
        {
            if (!modules.ContainsKey(assemblyName) || !modules[assemblyName].ContainsKey(moduleName)) throw new KeyNotFoundException($"No module registered from {assemblyName} named {moduleName}.");
            modules[assemblyName].Remove(moduleName);
            if (modules[assemblyName].Count == 0) modules.Remove(assemblyName);
        }

        public IReadOnlyCollection<string> GetAssemblyNames()
        {
            string[] names = new string[modules.Count];
            modules.Keys.CopyTo(names, 0);
            return names;
        }

        public IReadOnlyCollection<string> GetModuleNames(string assembly)
        {
            if (!modules.ContainsKey(assembly)) throw new KeyNotFoundException($"No loaded assembly named \"{assembly}\".");
            string[] names = new string[modules.Count];
            modules[assembly].Keys.CopyTo(names, 0);
            return names;

        }

        public void CreateService(string serviceName, string assemblyName, string moduleName)
        {
            if (!modules.ContainsKey(assemblyName) || !modules[assemblyName].ContainsKey(moduleName)) throw new KeyNotFoundException($"No module registered from \"{assemblyName}\" named \"{moduleName}\".");
            if (services.ContainsKey(serviceName)) throw new ArgumentException($"Service of Name \"{serviceName}\" already exists.");

            services.Add(serviceName, new ServiceInfo(modules[assemblyName][moduleName].CreateGameService(), moduleName, assemblyName));
        }

        public IReadOnlyCollection<string> GetServiceNames()
        {
            string[] names = new string[services.Count];
            services.Keys.CopyTo(names, 0);
            return names;
        }

        public IEnumerable<string> GetServiceOptions(string serviceName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            ServiceInfo serviceInfo = services[serviceName];
            return serviceInfo.GetConfigurableOptions();
        }

        public bool SetServiceOptionValue(string serviceName, string optionName, string value)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            if (!services[serviceName].GetConfigurableOptions().Contains(optionName)) throw new KeyNotFoundException($"Option \"{optionName}\" for service \"{serviceName}\" not found.");
            return services[serviceName].SetConfigurableValue(optionName, value);
        }

        public string GetServiceOptionValue(string serviceName, string optionName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            if (!services[serviceName].GetConfigurableOptions().Contains(optionName)) throw new KeyNotFoundException($"Option \"{optionName}\" for service \"{serviceName}\" not found.");
            return services[serviceName].GetConfigurableValue(optionName);
        }

        public ServiceState GetServiceState(string serviceName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            return services[serviceName].GetServiceState();
        }

        public void StartService(string serviceName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            services[serviceName].Start();
        }

        public void StopService(string serviceName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            services[serviceName].Stop();
        }

        public void ExecuteCommand(string serviceName, string command)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            services[serviceName].ExecuteCommand(command);
        }

        public Stream GetServiceConsoleStream(string serviceName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            return services[serviceName].ServiceConsoleStream;
        }
    }
}