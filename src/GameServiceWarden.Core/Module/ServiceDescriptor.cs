using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameServiceWarden.Core.Module.Exceptions;
using GameServiceWarden.Core.Logging;
using GameServiceWarden.ModuleFramework;
using System.Net.Sockets;

//TODO Update UML
namespace GameServiceWarden.Core.Module
{
    public class ServiceDescriptor //entity
    {
        public const string MODULE_PROPERTY = "SERVICE_MODULE";
        public const string ASSEMBLY_PROPERTY = "SERVICE_ASSEMBLY";
        private const int TIMEOUT = 1000;
        /// <summary>
        /// The name of the service itself, independent of the name of the module this service is using.
        /// </summary>
        public string ServiceName { get { return serviceName; } }
        private readonly string serviceName;
        private volatile ServiceState state;
        private readonly IService service;
        private string moduleName;
        private readonly string assemblyName;
        /// <summary>
        /// Name of module this service uses.
        /// </summary>
        private readonly IReadOnlyDictionary<string, IServiceConfigurable> configurables;
        public event EventHandler<ServiceState> ServiceStateChangeEvent;
        public event EventHandler<string> LogUpdateEvent;
        public ServiceDescriptor(IService service, string serviceName, string moduleName, string assemblyName)
        {
            this.service = service ?? throw new ArgumentNullException("service");
            this.moduleName = moduleName ?? throw new ArgumentNullException("moduleName");
            this.assemblyName = assemblyName ?? throw new ArgumentNullException("assemblyName");
            this.serviceName = serviceName ?? throw new ArgumentNullException("serviceName");
            this.service.StateChangeEvent += OnServiceStateChange;
            this.service.UpdateLogEvent += OnLogUpdate;

            Dictionary<string, IServiceConfigurable> tempConfigurables = new Dictionary<string, IServiceConfigurable>();
            foreach (IServiceConfigurable configurable in service.Configurables)
            {
                tempConfigurables.Add(configurable.OptionName, configurable);
            }
            this.configurables = new ReadOnlyDictionary<string, IServiceConfigurable>(tempConfigurables);
        }

        /// <summary>
        /// Starts this service.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when the service is already running.</exception>
        public void Start()
        {
            Logger.Log($"\"{ServiceName}\" is starting.");
            if (state == ServiceState.Running) throw new InvalidOperationException("Service instance already running.");
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);
            Task initializationTask = Task.Run(() => service.InitializeService(), cancellationTokenSource.Token);
            try {
                initializationTask.Wait();
            } catch (AggregateException a) {
                a.Handle((e) => e is TaskCanceledException);
            }
            cancellationTokenSource.Dispose();
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when the is not running.</exception>
        public void Stop()
        {
            if (state != ServiceState.Running) throw new InvalidOperationException("Service instance not running.");
            Logger.Log($"\"{ServiceName}\" is stopping.");
            service.ElegantShutdown();
        }

        /// <summary>
        /// Sends a command to this service to execute.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <exception cref="InvalidOperationException">Is thrown when the service is not running.</exception>
        public void ExecuteCommand(string command)
        {
            Logger.Log($"\"{ServiceName}\" is executing command \"{command}\".", LogLevel.DEBUG);
            if (state != ServiceState.Running) throw new InvalidOperationException("Service instance not running.");
            service.ExecuteCommand(command);
        }

        /// <summary>
        /// Gets the possible <see cref="IServiceConfigurable"/>'s names for this service.
        /// </summary>
        /// <returns>A <see cref="ISet{string}"/> returned where the string is the option's name.</returns>
        public ISet<string> GetConfigurableOptions()
        {
            return new HashSet<string>(this.configurables.Keys);
        }

        public bool SetConfigurableValue(string configurationName, string value)
        {
            if (!this.configurables.ContainsKey(configurationName)) throw new KeyNotFoundException($"Unable to find option with name \"{configurationName}\".");
            return this.configurables[configurationName].SetValue(value);
        }

        public string GetConfigurableValue(string configurationName)
        {
            if (!this.configurables.ContainsKey(configurationName)) throw new KeyNotFoundException($"Unable to find option with name \"{configurationName}\".");
            return this.configurables[configurationName].GetValue();
        }

        public ServiceState GetServiceState()
        {
            return state;
        }

        public string GetServiceName()
        {
            return serviceName;
        }

        public string GetModuleName()
        {
            return moduleName;
        }

        /// <returns>The name of assembly this module is contained in.</returns>
        public string GetAssemblyName()
        {
            return assemblyName;
        }

        public byte[] GetLogBuffer() {
            if (state == ServiceState.Stopped) throw new InvalidOperationException("Cannot get log of service that is not running.");
            return service.GetLogBuffer();
        }

        private void OnServiceStateChange(object sender, ServiceState state)
        {
            this.state = state;
            Logger.Log($"The service \"{ServiceName}\" is changing states to {this.state}.", LogLevel.DEBUG);
            ServiceStateChangeEvent?.Invoke(this, this.state);
        }

        private void OnLogUpdate(object sender, string log) {
            LogUpdateEvent?.Invoke(this, log);
        }
    }
}