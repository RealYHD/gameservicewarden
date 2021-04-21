using System;
using System.Collections.Generic;
using GameServiceWarden.InteractionAPI.Module;
using GameServiceWarden.Core.Module;

namespace GameServiceWarden.Core.Tests.Modules
{
    public class FakeServiceManagerMonitor : IServiceManagerMonitor
    {
        public List<ServiceManagerTotal> states = new List<ServiceManagerTotal>();
        public List<ServiceManagerDelta> deltas = new List<ServiceManagerDelta>();
        public ServiceManagerTotal this[int i]
        {
            get
            {
                return states[i];
            }
        }

        public void Present(ServiceManagerTotal state)
        {
            states.Add(state);
        }

        public void Present(ServiceManagerDelta delta)
        {
            deltas.Add(delta);
        }

        public ServiceManagerTotal GetLastState()
        {
            return states[states.Count - 1];
        }

        public ServiceManagerDelta GetLastDelta() {
            return deltas[deltas.Count - 1];
        }
    }
}