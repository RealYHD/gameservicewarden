using System;
using System.Collections.Generic;
using System.IO;
using GameServiceWarden.Core.Persistence;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Core.Games
{
    public class GameServiceManager
    {
        private readonly IDictionary<string, GameServiceInfo> running;
        private readonly IPersistentDictionary<GameServiceInfo> services;
        private readonly IReadOnlyPersistentDictionary<IReadOnlyDictionary<string, IGameServiceModule>> modules;

        public GameServiceManager(IPersistentDictionary<GameServiceInfo> services, IReadOnlyPersistentDictionary<IReadOnlyDictionary<string, IGameServiceModule>> modules)
        {
            this.services = services;
            this.modules = modules;
            this.running = new Dictionary<string, GameServiceInfo>();
        }

        public void CreateService(string serviceName, string assemblyName, string moduleName)
        {
            if (!this.modules.ContainsKey(assemblyName)) throw new KeyNotFoundException($"No file \"{assemblyName}\" found.");
            IReadOnlyDictionary<string, IGameServiceModule> assemblyModules = this.modules[assemblyName];
            if (services.ContainsKey(serviceName)) throw new ArgumentException($"Service of Name \"{serviceName}\" already exists.");

            services.Add(serviceName, new GameServiceInfo(assemblyModules[moduleName].InstantiateGameService(services.GetPathForKey(serviceName), true), moduleName, assemblyName));
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
            if (running.ContainsKey(serviceName)) throw new InvalidOperationException($"Service under name \"{serviceName}\" is already running.");
            running.Add(serviceName, services[serviceName]);
            running[serviceName].Start();
        }

        public void StopService(string serviceName)
        {
            if (!running.ContainsKey(serviceName)) throw new InvalidOperationException($"Service under name \"{serviceName}\" is not running.");
            running[serviceName].Stop();
            services[serviceName] = running[serviceName];
            running.Remove(serviceName);
        }

        public void ExecuteCommand(string serviceName, string command)
        {
            if (!running.ContainsKey(serviceName)) throw new InvalidOperationException($"Service under name \"{serviceName}\" is not running.");
            running[serviceName].ExecuteCommand(command);
        }

        public Stream GetServiceConsoleStream(string serviceName)
        {
            if (!running.ContainsKey(serviceName)) throw new InvalidOperationException($"Service under name \"{serviceName}\" is not running.");
            return running[serviceName].ServiceConsoleStream;
        }
    }
}