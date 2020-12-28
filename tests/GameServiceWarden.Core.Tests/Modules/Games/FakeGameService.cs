using System;
using System.Collections.Generic;
using System.IO;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Core.Tests.Modules.Games
{
    public class FakeGameService : IGameService
    {
        public IReadOnlyCollection<IGameConfigurable> Configurables { get; set; }

        public event EventHandler<ServiceState> StateChangeEvent;

        public ServiceState CurrentState { get; private set; } = ServiceState.Stopped;

        private TextWriter consoleStream;

        public FakeGameService(params IGameConfigurable[] configurables)
        {
            HashSet<IGameConfigurable> modifiable = new HashSet<IGameConfigurable>();
            foreach (IGameConfigurable configurable in configurables)
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