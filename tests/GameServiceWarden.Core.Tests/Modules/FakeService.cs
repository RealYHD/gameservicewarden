using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Core.Tests.Modules
{
    public class FakeService : IService
    {
        public IReadOnlyCollection<IServiceConfigurable> Configurables { get; set; }

        public event EventHandler<ServiceState> StateChangeEvent;
        public event EventHandler<string> UpdateLogEvent;

        public ServiceState CurrentState { get; private set; } = ServiceState.Stopped;
        private MemoryStream memoryStream;
        private StreamWriter consoleWriter;
        private Stack<Task> taskStack = new Stack<Task>();

        public FakeService(params IServiceConfigurable[] configurables)
        {
            HashSet<IServiceConfigurable> modifiable = new HashSet<IServiceConfigurable>();
            foreach (IServiceConfigurable configurable in configurables)
            {
                modifiable.Add(configurable);
            }
            this.Configurables = modifiable;
        }
        
        public void ElegantShutdown()
        {
            CurrentState = ServiceState.Stopped;
            StateChangeEvent?.Invoke(this, ServiceState.Stopped);
            Task task;
            while(taskStack.TryPop(out task)) {
                if (task.IsCompleted) {
                    task.Wait();
                } else {
                    throw new InvalidOperationException("A task was not completed.");
                }
            }
        }

        public void ExecuteCommand(string command)
        {
            taskStack.Push(consoleWriter.WriteLineAsync(command));
            taskStack.Push(consoleWriter.FlushAsync());
            UpdateLogEvent?.Invoke(this, command);
        }

        public void InitializeService()
        {
            CurrentState = ServiceState.Running;
            memoryStream = new MemoryStream();
            this.consoleWriter = new StreamWriter(memoryStream);
            StateChangeEvent?.Invoke(this, ServiceState.Running);
        }

        public byte[] GetLogBuffer()
        {
            return memoryStream.ToArray();
        }
    }
}