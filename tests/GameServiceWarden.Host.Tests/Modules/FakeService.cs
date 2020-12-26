using System;
using System.Collections.Generic;
using System.IO;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Host.Tests.Modules
{
    public class FakeService : IGameService
    {
        public IReadOnlyCollection<IConfigurable> Configurables { get; set; }

        public event EventHandler<ServiceState> StateChangeEvent;

        public ServiceState CurrentState { get; private set; } = ServiceState.Stopped;

        private TextWriter consoleStream;

        public FakeService(params IConfigurable[] configurables)
        {
            HashSet<IConfigurable> modifiable = new HashSet<IConfigurable>();
            foreach (IConfigurable configurable in configurables)
            {
                modifiable.Add(configurable);
            }
            this.Configurables = modifiable;
        }
        
        public void ElegantShutdown()
        {
            CurrentState = ServiceState.Stopped;
            StateChangeEvent?.Invoke(this, CurrentState);
        }

        public void ExecuteCommand(string command)
        {
            consoleStream.WriteLine(command);
            consoleStream.Flush();
        }

        public void InitializeService(TextWriter stream)
        {
            CurrentState = ServiceState.Running;
            StateChangeEvent?.Invoke(this, CurrentState);
            this.consoleStream = stream;
        }
    }
}