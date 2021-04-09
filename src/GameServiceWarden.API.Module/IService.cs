using System;
using System.Collections.Generic;
using System.IO;

namespace GameServiceWarden.API.Module
{
    public interface IService
    {
        event EventHandler<bool> StateChangeEvent;
        IReadOnlyCollection<IServiceConfigurable> Configurables{ get; }
        void InitializeService(Stream stream);
        void ElegantShutdown();
        void ExecuteCommand(string command);
    }
}