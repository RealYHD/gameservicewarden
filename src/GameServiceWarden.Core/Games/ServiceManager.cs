using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using GameServiceWarden.API.Games;
using GameServiceWarden.Core.Persistence;
using GameServiceWarden.API.Module;

namespace GameServiceWarden.Core.Games
{
    public class ServiceManager : IServiceManagerActionExecuter
    {
        private IServiceManagerMonitor managerMonitor;
        private readonly ConcurrentDictionary<string, ServiceDescriptor> running;
        private readonly IPersistent<ServiceDescriptor> services;
        private readonly IReadOnlyPersistent<IReadOnlyDictionary<string, IServiceModule>> modules;

        public ServiceManager(IServiceManagerMonitor actionMonitor, IPersistent<ServiceDescriptor> services, IReadOnlyPersistent<IReadOnlyDictionary<string, IServiceModule>> modules)
        {
            this.services = services ?? throw new ArgumentNullException("services");
            this.modules = modules ?? throw new ArgumentNullException("modules");
            this.managerMonitor = actionMonitor ?? throw new ArgumentNullException("actionMonitor");
            this.running = new ConcurrentDictionary<string, ServiceDescriptor>();
        }

        public void CreateService(string serviceName, string assemblyName, string moduleName)
        {
            if (!this.modules.ContainsKey(assemblyName)) throw new KeyNotFoundException($"No file \"{assemblyName}\" found.");
            IReadOnlyDictionary<string, IServiceModule> assemblyModules = this.modules[assemblyName];
            if (services.ContainsKey(serviceName)) throw new ArgumentException($"Service of Name \"{serviceName}\" already exists.");

            services.AddToPersistence(serviceName, new ServiceDescriptor(assemblyModules[moduleName].InstantiateService(services.GetPathForKey(serviceName), true), serviceName, moduleName, assemblyName));
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
            if (services[serviceName].GetServiceState()) services[serviceName].Stop();
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
            ServiceDescriptor serviceInfo = services[serviceName];
            return serviceInfo.GetConfigurableOptions();
        }

        private string GetServiceOptionValue(string serviceName, string optionName)
        {
            if (!services.ContainsKey(serviceName)) throw new KeyNotFoundException($"Service under name \"{serviceName}\" not found.");
            if (!services[serviceName].GetConfigurableOptions().Contains(optionName)) throw new KeyNotFoundException($"Option \"{optionName}\" for service \"{serviceName}\" not found.");
            return services[serviceName].GetConfigurableValue(optionName);
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
            if (!services[serviceName].GetConfigurableOptions().Contains(optionName)) throw new KeyNotFoundException($"Option \"{optionName}\" for service \"{serviceName}\" not found.");
            ServiceManagerState managerState = new ServiceManagerState();
            if (services[serviceName].SetConfigurableValue(optionName, value)) {
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
            ServiceDescriptor info = services[serviceName];
            info.ServiceStateChangeEvent += OnServiceStateChange;
            info.Start();
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

        private string GetServiceLogPipeName(string serviceName)
        {
            if (!running.ContainsKey(serviceName)) throw new InvalidOperationException($"Service under name \"{serviceName}\" is not running.");
            return running[serviceName].ServiceLogPipeName;
        }

        public IReadOnlyDictionary<string, string> GetLogPipeNames() {
            ServiceManagerState managerState = new ServiceManagerState();
            Dictionary<string, string> logPipeNames = new Dictionary<string, string>();
            foreach (string service in GetRunningServiceNames())
            {
                logPipeNames.Add(service, GetServiceLogPipeName(service));
            }
            managerState.logs = logPipeNames;
            managerMonitor.Present(managerState);
            return logPipeNames;
        }

        private void OnServiceStateChange(object sender, bool state) {
            ServiceDescriptor serviceInfo = (ServiceDescriptor)sender;
            ServiceManagerState managerChange = new ServiceManagerState();

            switch (state)
            {
                case true:
                    if (running.TryAdd(serviceInfo.ServiceName, serviceInfo)) {
                        managerChange.delta = true;
                        managerChange.running = new List<string>();
                        managerChange.running.Add(serviceInfo.ServiceName);
                        Dictionary<string, string> logAdded = new Dictionary<string, string>();
                        logAdded.Add(serviceInfo.ServiceName, GetServiceLogPipeName(serviceInfo.ServiceName));
                        managerChange.logs = logAdded;
                    }
                    break;
                case false:
                    ServiceDescriptor removed;
                    if (running.Remove(serviceInfo.ServiceName, out removed)) {
                        removed.ServiceStateChangeEvent -= OnServiceStateChange;
                        services[serviceInfo.ServiceName] = removed;
                        managerChange.delta = true;
                        managerChange.subtract = true;
                        managerChange.running = new List<string>();
                        managerChange.running.Add(serviceInfo.ServiceName);
                        Dictionary<string, string> logRemoved = new Dictionary<string, string>();
                        logRemoved.Add(serviceInfo.ServiceName, null);
                        managerChange.logs = logRemoved;
                    }
                    break;
            }

            managerMonitor.Present(managerChange);
        }

        public void ExecuteAction(ServiceManagerAction action)
        {
            switch (action.action)
            {
                //TODO FINISH MOVING THIS!!!!
                case ServiceManagerAction.Type.View:
                    GetServiceNames();
                    GetRunningServiceNames();
                    GetModuleNames();
                    GetLogPipeNames();
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