using GameServiceWarden.API.Games;

namespace GameServiceWarden.Core.Module
{
    public interface IServiceManagerMonitor
    {
        void Present(ServiceManagerState state);
    }
}