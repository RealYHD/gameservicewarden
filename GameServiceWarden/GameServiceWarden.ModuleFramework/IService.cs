using System;
using System.Collections.Generic;
using System.IO;

namespace GameServiceWarden.ModuleFramework
{
    public interface IService
    {
        event EventHandler<ServiceState> StateChangeEvent;
        event EventHandler<string> UpdateLogEvent;
        IReadOnlyCollection<IServiceConfigurable> Configurables{ get; }
        void InitializeService();
        void ElegantShutdown();
        byte[] GetLogBuffer();
        void ExecuteCommand(string command);
    }
}