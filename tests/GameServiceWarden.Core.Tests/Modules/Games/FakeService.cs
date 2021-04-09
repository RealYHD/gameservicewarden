using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GameServiceWarden.API.Module;

namespace GameServiceWarden.Core.Tests.Modules.Games
{
    public class FakeService : IService
    {
        public IReadOnlyCollection<IServiceConfigurable> Configurables { get; set; }

        public event EventHandler<bool> StateChangeEvent;
        public ServiceState CurrentState { get; private set; } = ServiceState.Stopped;

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
            StateChangeEvent?.Invoke(this, false);
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
        }

        public void InitializeService(Stream stream)
        {
            CurrentState = ServiceState.Running;
            this.consoleWriter = new StreamWriter(stream);
            StateChangeEvent?.Invoke(this, true);
        }
    }
}