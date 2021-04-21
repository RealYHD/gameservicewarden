using GameServiceWarden.ClientAPI.Module;

namespace GameServiceWarden.Core.Module
{
    public interface IServiceManagerMonitor
    {
        void Present(ServiceManagerState state);
    }
}