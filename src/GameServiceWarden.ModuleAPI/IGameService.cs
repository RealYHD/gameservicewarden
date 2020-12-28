using System;
using System.Collections.Generic;
using System.IO;

namespace GameServiceWarden.ModuleAPI
{
    public interface IGameService
    {
        event EventHandler<ServiceState> StateChangeEvent;
        IReadOnlyCollection<IGameConfigurable> Configurables{ get; }
        void InitializeService(TextWriter stream);
        void ElegantShutdown();
        void ExecuteCommand(string command);
    }
}