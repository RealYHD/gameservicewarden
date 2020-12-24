using System;
using System.Collections.Generic;
using System.IO;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Host.Modules
{
    public class ServiceManager
    {
        private Dictionary<string, ServiceInfo> services = new Dictionary<string, ServiceInfo>();
        private Dictionary<string, Dictionary<string, IGameServiceModule>> modules = new Dictionary<string, Dictionary<string, IGameServiceModule>>();

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

        public void CreateService(string serviceName, string assemblyName, string moduleName)
        {
            if (!modules.ContainsKey(assemblyName) || modules[assemblyName].ContainsKey(moduleName)) throw new KeyNotFoundException($"No module registered from \"{assemblyName}\" named \"{moduleName}\".");
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
            return serviceInfo.GetConfigurables().Keys;
        }

        public bool SetServiceOptionValue(string serviceName, string optionName, string value)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            if (!services[serviceName].GetConfigurables().ContainsKey(optionName)) throw new KeyNotFoundException($"Option \"{optionName}\" for service \"{serviceName}\" not found.");
            IConfigurable configurable = services[serviceName].GetConfigurables()[optionName];
            return configurable.SetValue(value);
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