using GameServiceWarden.InteractionAPI.Module;

namespace GameServiceWarden.Core.Module
{
    public interface IServiceManagerMonitor
    {
        void Present(ServiceManagerTotal state);
        void Present(ServiceManagerDelta delta);
    }
}