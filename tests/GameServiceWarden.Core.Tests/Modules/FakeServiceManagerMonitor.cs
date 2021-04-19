using System;
using System.Collections.Generic;
using GameServiceWarden.API.Games;
using GameServiceWarden.Core.Module;

namespace GameServiceWarden.Core.Tests.Modules
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