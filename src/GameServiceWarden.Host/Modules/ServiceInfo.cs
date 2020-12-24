using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using GameServiceWarden.Host.Preferences;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Host.Modules
{
    public class ServiceInfo : IDisposable //entity
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
        private object controlLock = new object();
        private volatile ServiceState state;
        private readonly IGameService service;
        private readonly string assemblyName;
        private readonly string moduleName;
        private readonly Dictionary<string, IConfigurable> configurables = new Dictionary<string, IConfigurable>();
        private bool disposed;

        public ServiceInfo(IGameService service, string moduleName, string assemblyName)
        {
            this.service = service ?? throw new ArgumentNullException("serviceable");
            this.moduleName = moduleName ?? throw new ArgumentNullException("moduleName");
            this.assemblyName = assemblyName ?? throw new ArgumentNullException("assemblyName");
            this.service.StateChangeEvent += OnServiceStateChange;

            foreach (IConfigurable configurable in service.Configurables)
            {
                configurables.Add(configurable.OptionName, configurable);
            }
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
        /// Gets the possible <see cref="IConfigurable"/>'s for this service.
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{string, IConfigurable}"/> is returned where the string is the option name and the configurable is what handles actually changing the values.</returns>
        public IReadOnlyDictionary<string, IConfigurable> GetConfigurables()
        {
            return new ReadOnlyDictionary<string, IConfigurable>(this.configurables);
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