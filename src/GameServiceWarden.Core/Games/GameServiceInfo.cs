using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Core.Games
{
    public class GameServiceInfo : IDisposable //entity
    {

        /// <summary>
        /// The name of the service itself, independent of the name of the module this service is using.
        /// </summary>
        public string ServiceName { get { return serviceName; } set { Interlocked.Exchange(ref serviceName, value); } }

        /// <summary>
        /// The services console output stream.
        /// </summary>
        private volatile string serviceName; //thread-safe(?)
        public Stream ServiceConsoleStream { get; private set; } // Thread safe.
        private readonly object controlLock = new object();
        private volatile ServiceState state;
        private readonly IGameService service;
        private readonly string assemblyName;
        private readonly string moduleName;
        private readonly IReadOnlyDictionary<string, IGameConfigurable> configurables;
        private bool disposed;

        public GameServiceInfo(IGameService service, string moduleName, string assemblyName)
        {
            this.service = service ?? throw new ArgumentNullException("service");
            this.moduleName = moduleName ?? throw new ArgumentNullException("moduleName");
            this.assemblyName = assemblyName ?? throw new ArgumentNullException("assemblyName");
            this.service.StateChangeEvent += OnServiceStateChange;

            Dictionary<string, IGameConfigurable> configurables = new Dictionary<string, IGameConfigurable>();
            foreach (IGameConfigurable configurable in service.Configurables)
            {
                configurables.Add(configurable.OptionName, configurable);
            }
            this.configurables = new ReadOnlyDictionary<string, IGameConfigurable>(configurables);
        }

        /// <summary>
        /// Starts this service.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when the service is already running.</exception>
        public void Start()
        {
            lock (controlLock)
            {
                if (state != ServiceState.Stopped) throw new InvalidOperationException("Service instance already running.");
                DateTimeFormatInfo format = new DateTimeFormatInfo();
                format.TimeSeparator = "-";
                format.DateSeparator = "_";
                ServiceConsoleStream = new MemoryStream(8 * 1024 * 1024); // 8 MB
                ServiceConsoleStream = Stream.Synchronized(ServiceConsoleStream);
                service.InitializeService(new StreamWriter(ServiceConsoleStream));
            }
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when the is not running.</exception>
        public void Stop()
        {
            lock (controlLock)
            {
                if (state != ServiceState.Running) throw new InvalidOperationException("Service instance not running.");
                this.service.ElegantShutdown();
                ServiceConsoleStream.Close();
                ServiceConsoleStream = null;
            }
        }

        /// <summary>
        /// Sends a command to this service to execute.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <exception cref="InvalidOperationException">Is thrown when the service is not running.</exception>
        public void ExecuteCommand(string command)
        {
            lock (controlLock)
            {
                if (state != ServiceState.Running) throw new InvalidOperationException("Service instance not running.");
                service.ExecuteCommand(command);
            }
        }

        /// <summary>
        /// Gets the possible <see cref="IGameConfigurable"/>'s names for this service.
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

        /// <returns>The <see cref="ServiceState"/> that this service is currently in.</returns>
        public ServiceState GetServiceState()
        {
            lock (controlLock)
            {
                return state;
            }
        }

        /// <returns>The name of the module this service uses.</returns>
        public string GetModuleName()
        {
            return moduleName;
        }

        /// <returns>The name of assembly this module is contained in.</returns>
        public string GetAssemblyName()
        {
            return assemblyName;
        }

        private void OnServiceStateChange(object sender, ServiceState current)
        {
            lock (controlLock)
            {
                this.state = current;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ServiceConsoleStream?.Dispose();
                }
                //No unmanaged code, therefore, no finalizer.
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}