using GameServiceWarden.API.Games;

namespace GameServiceWarden.Core.Games
{
    public interface IServiceManagerMonitor
    {
        void Present(ServiceManagerState state);
    }
}