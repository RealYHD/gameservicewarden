using System;
using System.Collections.Generic;
using GameServiceWarden.API.Games;
using GameServiceWarden.Core.Games;

namespace GameServiceWarden.Core.Tests.Modules.Games
{
    public class FakeServiceManagerMonitor : IServiceManagerMonitor
    {
        public List<ServiceManagerState> states = new List<ServiceManagerState>();
        public ServiceManagerState this[int i]
        {
            get
            {
                return states[i];
            }
        }
        public void Present(ServiceManagerState state)
        {
            states.Add(state);
        }

        public ServiceManagerState GetLastState()
        {
            return states[states.Count - 1];
        }
    }
}