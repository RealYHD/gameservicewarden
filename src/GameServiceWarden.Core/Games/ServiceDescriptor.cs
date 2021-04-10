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
using GameServiceWarden.Core.Games.Modules.Exceptions;
using GameServiceWarden.Core.Logging;
using GameServiceWarden.API.Module;

//TODO Update UML
namespace GameServiceWarden.Core.Games
{
    public class ServiceDescriptor //entity
    {
        private const string LOG_DISTRIBUTOR_PREFIX = "log_dist_";
        private const int TIMEOUT = 1000;
        /// <summary>
        /// The name of the service itself, independent of the name of the module this service is using.
        /// </summary>
        public string ServiceName { get { return serviceName; } }
        private readonly string serviceName;
        private readonly Guid runningUID;
        private volatile bool running;
        private readonly IService service;
        /// <summary>
        /// The services log output pipe name.
        /// </summary>
        public string ServiceLogPipeName { get { return (runningUID.ToString() + ".pipe"); } }
        private string moduleName;
        private readonly string assemblyName;
        private volatile NamedPipeServerStream logReceiver;
        private volatile NamedPipeClientStream logSender;
        private ConcurrentStack<NamedPipeServerStream> logStreamListeners;
        private Task logUpdateTask;
        private Task acceptingTask;
        private volatile CancellationTokenSource stopToken;

        /// <summary>
        /// Name of module this service uses.
        /// </summary>
        private readonly IReadOnlyDictionary<string, IServiceConfigurable> configurables;
        public event EventHandler<bool> ServiceStateChangeEvent;
        public ServiceDescriptor(IService service, string serviceName, string moduleName, string assemblyName)
        {
            this.service = service ?? throw new ArgumentNullException("service");
            this.moduleName = moduleName ?? throw new ArgumentNullException("moduleName");
            this.assemblyName = assemblyName ?? throw new ArgumentNullException("assemblyName");
            this.serviceName = serviceName ?? throw new ArgumentNullException("serviceName");
            this.service.StateChangeEvent += OnServiceStateChange;
            runningUID = Guid.NewGuid();


            Dictionary<string, IServiceConfigurable> tempConfigurables = new Dictionary<string, IServiceConfigurable>();
            foreach (IServiceConfigurable configurable in service.Configurables)
            {
                tempConfigurables.Add(configurable.OptionName, configurable);
            }
            this.configurables = new ReadOnlyDictionary<string, IServiceConfigurable>(tempConfigurables);

            logStreamListeners = new ConcurrentStack<NamedPipeServerStream>();
        }

        /// <summary>
        /// Starts this service.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when the service is already running.</exception>
        public void Start()
        {
            Logger.Log($"\"{ServiceName}\" is starting.");
            if (running) throw new InvalidOperationException("Service instance already running.");
            logReceiver = new NamedPipeServerStream(LOG_DISTRIBUTOR_PREFIX + ServiceLogPipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            Task waitForConnection = logReceiver.WaitForConnectionAsync();
            logSender = new NamedPipeClientStream(".", LOG_DISTRIBUTOR_PREFIX + ServiceLogPipeName, PipeDirection.Out);
            logSender.Connect();
            waitForConnection.Wait();
            byte[] idToken = Guid.NewGuid().ToByteArray();
            ValueTask sendTokenTask = logSender.WriteAsync(idToken);
            byte[] receivedToken = new byte[idToken.Length];
            logReceiver.Read(receivedToken);

            if (!sendTokenTask.AsTask().Wait(500) || !sendTokenTask.IsCompletedSuccessfully)
            {
                throw new ServiceInitializationException("Error while sending identification token.");
            }
            if (!idToken.SequenceEqual(receivedToken))
            {
                throw new ServiceInitializationException("Wrong distributor identification token.");
            }
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TIMEOUT);
            Task initializationTask = Task.Run(() => service.InitializeService(logSender), cancellationTokenSource.Token);
            initializationTask.Wait();
            cancellationTokenSource.Dispose();
            stopToken = new CancellationTokenSource();
            acceptingTask = AcceptLogConnections();
            logUpdateTask = BroadcastLog();
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when the is not running.</exception>
        public void Stop()
        {
            if (!running) throw new InvalidOperationException("Service instance not running.");
            Logger.Log($"\"{ServiceName}\" is stopping.");
            service.ElegantShutdown();
            logSender.Close();
            logReceiver.Close();
            stopToken.Cancel();
            try
            {
                if (!acceptingTask.Wait(TIMEOUT)) {
                    throw new TimeoutException($"Could not stop \"{ServiceName}\" accepting task within {TIMEOUT}ms.");
                }
            }
            catch (AggregateException e)
            {
                e.Handle((exception) => exception is TaskCanceledException);
            }
            try
            {
                if (!logUpdateTask.Wait(TIMEOUT)) {
                    throw new TimeoutException($"Could not stop \"{ServiceName}\" broadcast within{TIMEOUT}ms.");
                }
            }
            catch (AggregateException e)
            {
                e.Handle((exception) => exception is TaskCanceledException);
            }
            logSender.Dispose();
            logReceiver.Dispose();
            logStreamListeners.Clear();
            stopToken.Dispose();
        }

        /// <summary>
        /// Sends a command to this service to execute.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <exception cref="InvalidOperationException">Is thrown when the service is not running.</exception>
        public void ExecuteCommand(string command)
        {
            Logger.Log($"\"{ServiceName}\" is executing command \"{command}\".", LogLevel.DEBUG);
            if (!running) throw new InvalidOperationException("Service instance not running.");
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

        public bool GetServiceState()
        {
            return running;
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

        private void OnServiceStateChange(object sender, bool running)
        {
            this.running = running;
            Logger.Log($"The service \"{ServiceName}\" is changing states to {(running ? "running" : "stopped")}.", LogLevel.DEBUG);
            ServiceStateChangeEvent?.Invoke(this, running);
        }

        private async Task AcceptLogConnections()
        {
            Logger.Log($"\"{ServiceName}\" is now accepting log listeners.");
            while (running)
            {
                NamedPipeServerStream pipe = new NamedPipeServerStream(ServiceLogPipeName, PipeDirection.Out, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await pipe.WaitForConnectionAsync(stopToken.Token);
                Logger.Log($"A log listener has connected. Currently broadcasting to {logStreamListeners.Count + 1} listener(s).", LogLevel.DEBUG);
                logStreamListeners.Push(pipe);
            }
            Logger.Log($"\"{ServiceName}\" stopped accepting log listeners.");
        }

        private async Task BroadcastLog()
        {
            Stack<NamedPipeServerStream> completeStack = new Stack<NamedPipeServerStream>();
            Stack<(Task, CancellationTokenSource)> writeTasks = new Stack<(Task, CancellationTokenSource)>();
            byte[] buffer = new byte[1024 * 8];
            int fill;
            Logger.Log($"\"{ServiceName}\" is now listening to the service log and broadcasting.");
            while (running && (fill = await logReceiver.ReadAsync(buffer, 0, buffer.Length, stopToken.Token)) > 0)
            {
                Logger.Log($"Broadcasting {fill} bytes.", LogLevel.DEBUG);
                NamedPipeServerStream pipe;
                while (logStreamListeners.TryPop(out pipe))
                {
                    if (!pipe.IsConnected)
                    {
                        pipe.Dispose();
                        Logger.Log($"\"{ServiceName}\" detected a disconnected log listener. Removing from list of listener(s).", LogLevel.DEBUG);
                    }
                    else
                    {
                        CancellationTokenSource cancelToken = new CancellationTokenSource(1000);
                        writeTasks.Push((pipe.WriteAsync(buffer, 0, fill, cancelToken.Token), cancelToken));
                        completeStack.Push(pipe);
                    }
                }
                NamedPipeServerStream completePipe;
                while (completeStack.TryPop(out completePipe))
                {
                    logStreamListeners.Push(completePipe);
                }
                (Task, CancellationTokenSource) taskAndCancel;
                while (writeTasks.TryPop(out taskAndCancel))
                {
                    await taskAndCancel.Item1;
                    taskAndCancel.Item2.Dispose();
                }
                Logger.Log($"\"{ServiceName}\" broadcasted to {logStreamListeners.Count} listener(s).", LogLevel.DEBUG);
            }
            Logger.Log($"\"{ServiceName}\" stopped listening to service log.");
        }
    }
}