using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using GameServiceWarden.ClientAPI.Module;
using GameServiceWarden.Core.Persistence;
using GameServiceWarden.ModuleAPI;
using GameServiceWarden.Core.Collection;

namespace GameServiceWarden.Core.Module
{
    public class ServiceManager : IServiceManagerActionExecuter
    {
        private IServiceManagerMonitor managerMonitor;
        private readonly ConcurrentDictionary<string, ServiceDescriptor> running;
        private readonly IPersistent<IReadOnlyDictionary<string, string>> services;
        private readonly LRUCache<string, ServiceDescriptor> descriptorCache;
        private readonly IReadOnlyPersistent<IReadOnlyDictionary<string, IServiceModule>> modules;

        public ServiceManager(IServiceManagerMonitor actionMonitor, IPersistent<IReadOnlyDictionary<string, string>> services, IReadOnlyPersistent<IReadOnlyDictionary<string, IServiceModule>> modules)
        {
            this.services = services ?? throw new ArgumentNullException("services");
            this.modules = modules ?? throw new ArgumentNullException("modules");
            this.managerMonitor = actionMonitor ?? throw new ArgumentNullException("actionMonitor");
            this.running = new ConcurrentDictionary<string, ServiceDescriptor>();
            this.descriptorCache = new LRUCache<string, ServiceDescriptor>(100);
        }

        public void CreateService(string serviceName, string assemblyName, string moduleName)
        {
            if (!this.modules.ContainsKey(assemblyName)) throw new KeyNotFoundException($"No file \"{assemblyName}\" found.");
            IReadOnlyDictionary<string, IServiceModule> assemblyModules = this.modules[assemblyName];
            if (services.ContainsKey(serviceName)) throw new ArgumentException($"Service of Name \"{serviceName}\" already exists.");
            Dictionary<string, string> data = new Dictionary<string, string>();
            data[ServiceDescriptor.ASSEMBLY_PROPERTY] = assemblyName;
            data[ServiceDescriptor.MODULE_PROPERTY] = moduleName;
            services.AddToPersistence(serviceName, data);
            ServiceManagerState managerState = new ServiceManagerState();
            managerState.delta = true;
            managerState.subtract = false;
            managerState.services = new List<string>();
            managerState.services.Add(serviceName);
            managerMonitor.Present(managerState);
        }

        public void DeleteService(string serviceName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            if (running.ContainsKey(serviceName)) running[serviceName].Stop();
            services.Delete(serviceName);
            ServiceManagerState managerState = new ServiceManagerState();
            managerState.delta = true;
            managerState.subtract = true;
            managerState.services = new List<string>();
            managerState.services.Add(serviceName);
            managerMonitor.Present(managerState);
        }

        public IEnumerable<string> GetModuleNames()
        {
            ServiceManagerState managerState = new ServiceManagerState();
            managerState.modules = modules.Keys.ToImmutableArray();
            managerMonitor.Present(managerState);
            return modules.Keys;
        }

        public IEnumerable<string> GetServiceNames()
        {
            ServiceManagerState managerState = new ServiceManagerState();
            managerState.services = services.Keys.ToImmutableArray();
            managerMonitor.Present(managerState);
            return services.Keys;
        }

        public IEnumerable<string> GetRunningServiceNames() {
            ServiceManagerState managerState = new ServiceManagerState();
            managerState.running = running.Keys.ToImmutableArray();
            managerMonitor.Present(managerState);
            return running.Keys;
        }

        private IEnumerable<string> GetServiceOptions(string serviceName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            IReadOnlyDictionary<string, string> info = services[serviceName];
            ServiceDescriptor service = descriptorCache.Use(serviceName, () => GenerateDescriptor(serviceName, info[ServiceDescriptor.ASSEMBLY_PROPERTY], info[ServiceDescriptor.MODULE_PROPERTY]));
            return service.GetConfigurableOptions();
        }

        private string GetServiceOptionValue(string serviceName, string optionName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            IReadOnlyDictionary<string, string> info = services[serviceName];
            ServiceDescriptor service = descriptorCache.Use(serviceName, () => GenerateDescriptor(serviceName, info[ServiceDescriptor.ASSEMBLY_PROPERTY], info[ServiceDescriptor.MODULE_PROPERTY]));
            if (!service.GetConfigurableOptions().Contains(optionName)) throw new KeyNotFoundException($"Option \"{optionName}\" for service \"{serviceName}\" not found.");
            return service.GetConfigurableValue(optionName);
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> GetOptions() {
            ServiceManagerState managerState = new ServiceManagerState();
            Dictionary<string, IReadOnlyDictionary<string, string>> serviceOptions = new Dictionary<string, IReadOnlyDictionary<string, string>>();
            foreach (string service in GetServiceNames())
            {
                Dictionary<string, string> optionsOfService = new Dictionary<string, string>();
                foreach (string option in GetServiceOptions(service))
                {
                    optionsOfService.Add(option, GetServiceOptionValue(service, option));
                }
                serviceOptions.Add(service, optionsOfService);
            }
            managerState.serviceOptions = serviceOptions;
            managerMonitor.Present(managerState);
            return serviceOptions;
        }

        public bool SetServiceOptionValue(string serviceName, string optionName, string value)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            IReadOnlyDictionary<string, string> info = services[serviceName];
            ServiceDescriptor service = descriptorCache.Use(serviceName, () => GenerateDescriptor(serviceName, info[ServiceDescriptor.ASSEMBLY_PROPERTY], info[ServiceDescriptor.MODULE_PROPERTY]));
            if (!service.GetConfigurableOptions().Contains(optionName)) throw new KeyNotFoundException($"Option \"{optionName}\" for service \"{serviceName}\" not found.");
            ServiceManagerState managerState = new ServiceManagerState();
            if (service.SetConfigurableValue(optionName, value)) {
                managerState.delta = true;
                Dictionary<string, IReadOnlyDictionary<string, string>> changedOption = new Dictionary<string, IReadOnlyDictionary<string, string>>();
                Dictionary<string, string> options = new Dictionary<string, string>();
                options[optionName] = GetServiceOptionValue(serviceName, optionName);
                changedOption[serviceName] = options;
                managerState.serviceOptions = changedOption;
            }
            managerMonitor.Present(managerState);
            return managerState.delta;
        }

        public void StartService(string serviceName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            if (running.ContainsKey(serviceName)) throw new InvalidOperationException($"Service under name \"{serviceName}\" is already running.");
            IReadOnlyDictionary<string, string> info = services[serviceName];
            ServiceDescriptor service = descriptorCache.Use(serviceName, () => GenerateDescriptor(serviceName, info[ServiceDescriptor.ASSEMBLY_PROPERTY], info[ServiceDescriptor.MODULE_PROPERTY]));
            service.ServiceStateChangeEvent += OnServiceStateChange;
            service.LogUpdateEvent += OnLogUpdated;
            service.Start();
        }

        public void StopService(string serviceName)
        {
            if (!running.ContainsKey(serviceName)) throw new InvalidOperationException($"Service under name \"{serviceName}\" is not running.");
            running[serviceName].Stop();
        }

        public void ExecuteCommand(string serviceName, string command)
        {
            if (!running.ContainsKey(serviceName)) throw new InvalidOperationException($"Service under name \"{serviceName}\" is not running.");
            running[serviceName].ExecuteCommand(command);
        }

        private byte[] GetServiceLogBuffer(string serviceName) {
            if (!running.ContainsKey(serviceName)) throw new InvalidOperationException($"Service under name \"{serviceName}\" is not running and therefore, does not have a log.");
            IReadOnlyDictionary<string, string> info = services[serviceName];
            ServiceDescriptor service = descriptorCache.Use(serviceName, () => GenerateDescriptor(serviceName, info[ServiceDescriptor.ASSEMBLY_PROPERTY], info[ServiceDescriptor.MODULE_PROPERTY]));
            return service.GetLogBuffer();
        }

        public Dictionary<string, byte[]> GetLogBuffer() {
            Dictionary<string, byte[]> logs = new Dictionary<string, byte[]>();
            foreach (string service in running.Keys)
            {
                logs.Add(service, running[service].GetLogBuffer());
            }
            ServiceManagerState managerState = new ServiceManagerState();
            managerState.logs = logs;
            managerMonitor.Present(managerState);
            return logs;
        }

        private ServiceDescriptor GenerateDescriptor(string name, string assembly, string module) {
            return new ServiceDescriptor(modules[assembly][module].InstantiateService(services.GetPathForKey(name)), name, module, assembly);
        }

        private void OnServiceStateChange(object sender, ServiceState state) {
            ServiceDescriptor serviceInfo = (ServiceDescriptor)sender;
            ServiceManagerState managerChange = new ServiceManagerState();

            switch (state)
            {
                case ServiceState.Running:
                    if (running.TryAdd(serviceInfo.ServiceName, serviceInfo)) {
                        managerChange.delta = true;
                        managerChange.running = new List<string>();
                        managerChange.running.Add(serviceInfo.ServiceName);
                    }
                    break;
                case ServiceState.Stopped:
                    ServiceDescriptor removed;
                    if (running.TryRemove(serviceInfo.ServiceName, out removed)) {
                        removed.ServiceStateChangeEvent -= OnServiceStateChange;
                        removed.LogUpdateEvent -= OnLogUpdated;
                        Dictionary<string, string> removedInfo = new Dictionary<string, string>();
                        removedInfo[ServiceDescriptor.ASSEMBLY_PROPERTY] = removed.GetAssemblyName();
                        removedInfo[ServiceDescriptor.MODULE_PROPERTY] = removed.GetModuleName();
                        services[serviceInfo.ServiceName] = removedInfo;
                        managerChange.delta = true;
                        managerChange.subtract = true;
                        managerChange.running = new List<string>();
                        managerChange.running.Add(serviceInfo.ServiceName);
                    }
                    break;
            }

            managerMonitor.Present(managerChange);
        }

        void OnLogUpdated(object sender, string update) {
            ServiceDescriptor service = (ServiceDescriptor)sender;
            ServiceManagerState state = new ServiceManagerState();
            state.delta = true;
            Dictionary<string, string> logUpdate = new Dictionary<string, string>();
            logUpdate[service.ServiceName] = update;
            managerMonitor.Present(state);
        }

        public void ExecuteAction(ServiceManagerAction action)
        {
            switch (action.action)
            {
                case ServiceManagerAction.Type.View:
                    GetServiceNames();
                    GetRunningServiceNames();
                    GetModuleNames();
                    GetLogBuffer();
                    GetOptions();
                    break;

                case ServiceManagerAction.Type.CreateService:
                    CreateService(action.serviceName, action.assemblyName, action.moduleName);
                    break;

                case ServiceManagerAction.Type.DeleteService:
                    DeleteService(action.serviceName);
                    break;

                case ServiceManagerAction.Type.ExecuteCommand:
                    ExecuteCommand(action.serviceName, action.command);
                    break;
                case ServiceManagerAction.Type.SetServiceOption:
                    SetServiceOptionValue(action.serviceName, action.option, action.value);
                    break;
                case ServiceManagerAction.Type.Start:
                    StartService(action.serviceName);
                    break;
                case ServiceManagerAction.Type.Stop:
                    StopService(action.serviceName);
                    break;
            }
        }
    }
}