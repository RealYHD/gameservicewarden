using System;
using System.Collections.Generic;
using System.IO;
using GameServiceWarden.Core.Persistence;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Core.Games
{
    public class GameServiceManager
    {
        private readonly IPersistentDictionary<GameServiceInfo> services;
        private readonly IReadOnlyPersistentDictionary<IGameServiceModule> modules;

        public GameServiceManager(IPersistentDictionary<GameServiceInfo> services, IReadOnlyPersistentDictionary<IGameServiceModule> modules)
        {
            this.services = services;
            this.modules = modules;
        }

        public void CreateService(string serviceName, string assemblyName, string moduleName)
        {
            string moduleID = ToModuleID(assemblyName, moduleName);
            if (!modules.ContainsKey(moduleID)) throw new KeyNotFoundException($"No module registered from \"{assemblyName}\" named \"{moduleName}\".");
            if (services.ContainsKey(serviceName)) throw new ArgumentException($"Service of Name \"{serviceName}\" already exists.");

            services.Add(serviceName, new GameServiceInfo(modules[moduleID].InstantiateGameService(services.GetPathForKey(serviceName), true), moduleName, assemblyName));
        }

        public void DeleteService(string serviceName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            if (services[serviceName].GetServiceState() == ServiceState.Running) services[serviceName].Stop();
            services.Remove(serviceName);
        }

        public IEnumerable<string> GetModuleNames()
        {
            return modules.Keys;
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
            GameServiceInfo serviceInfo = services[serviceName];
            return serviceInfo.GetConfigurableOptions();
        }

        public string GetServiceOptionValue(string serviceName, string optionName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            if (!services[serviceName].GetConfigurableOptions().Contains(optionName)) throw new KeyNotFoundException($"Option \"{optionName}\" for service \"{serviceName}\" not found.");
            return services[serviceName].GetConfigurableValue(optionName);
        }

        public bool SetServiceOptionValue(string serviceName, string optionName, string value)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            if (!services[serviceName].GetConfigurableOptions().Contains(optionName)) throw new KeyNotFoundException($"Option \"{optionName}\" for service \"{serviceName}\" not found.");
            return services[serviceName].SetConfigurableValue(optionName, value);
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

        private string ToModuleID(string assemblyName, string moduleName)
        {
            return assemblyName + Path.DirectorySeparatorChar + moduleName;
        }
    }
}