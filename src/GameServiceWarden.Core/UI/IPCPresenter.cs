using System.Text.Json;
using System.Threading.Tasks;
using GameServiceWarden.InteractionAPI;
using GameServiceWarden.InteractionAPI.Module;
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

        public void Present(ServiceManagerTotal state)
        {
            Task replyTask = mediator.ReplyAll(CommunicableType.View, JsonSerializer.SerializeToUtf8Bytes(state));
            replyTask.Wait();
        }

        public void Present(ServiceManagerDelta delta)
        {
            Task replyTask = mediator.ReplyAll(CommunicableType.Delta, JsonSerializer.SerializeToUtf8Bytes(delta));
            replyTask.Wait();
        }
    }
}