using System;
using System.Collections.Generic;
using System.IO;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Host.Tests.Modules
{
    public class FakeService : IGameService
    {
        public IReadOnlyCollection<IConfigurable> Configurables { get; set; } = new HashSet<IConfigurable>();

        public event EventHandler<ServiceState> StateChangeEvent;

        public ServiceState CurrentState { get; private set; } = ServiceState.Stopped;

        public void ElegantShutdown()
        {
            CurrentState = ServiceState.Stopped;
            StateChangeEvent?.Invoke(this, CurrentState);
        }

        public void ExecuteCommand(string command)
        {
        }

        public void InitializeService(TextWriter stream)
        {
            CurrentState = ServiceState.Running;
            StateChangeEvent?.Invoke(this, CurrentState);
        }
    }
}