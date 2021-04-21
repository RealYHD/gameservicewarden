using System.Text.Json;
using System.Threading.Tasks;
using GameServiceWarden.ClientAPI;
using GameServiceWarden.ClientAPI.Module;
using GameServiceWarden.Core.Module;

namespace GameServiceWarden.Core.UI
{
    public class IPCPresenter : IServiceManagerMonitor
    {
        private IPCMediator mediator;

        public IPCPresenter(IPCMediator mediator)
        {
            this.mediator = mediator;
        }

        public void Present(ServiceManagerState state)
        {
            Task replyTask = mediator.ReplyAll(CommunicableType.Service, JsonSerializer.SerializeToUtf8Bytes(state));
        }
    }
}