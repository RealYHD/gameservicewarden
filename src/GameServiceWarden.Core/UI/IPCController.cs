using System.Diagnostics;
using System.Text.Json;
using GameServiceWarden.InteractionAPI;
using GameServiceWarden.InteractionAPI.Communicable.Requests;
using GameServiceWarden.Core.Module;
using GameServiceWarden.Core.Logging;

namespace GameServiceWarden.Core.UI
{
    public class IPCController
    {
        private IPCMediator mediator;
        private IServiceManagerActionExecuter serviceExecutioner;

        public IPCController(IPCMediator mediator, IServiceManagerActionExecuter serviceExecutioner)
        {
            this.mediator = mediator;
            this.serviceExecutioner = serviceExecutioner;
        }

        public void Process() {
            Logger.Log("Beginning to process interface requests.");
            mediator.Open();
            (string, CommunicableType, byte[]) action;
            while (mediator.RequestQueue.TryTake(out action))
            {
                switch (action.Item2)
                {
                    case CommunicableType.Delta:
                        ServiceRequest delta = JsonSerializer.Deserialize<ServiceRequest>(action.Item3);
                        serviceExecutioner.ExecuteAction(delta.serviceManagerAction);
                        break;
                    case CommunicableType.View:
                        serviceExecutioner.View();
                        break;
                }
            }
            mediator.Close();
        }
    }
}