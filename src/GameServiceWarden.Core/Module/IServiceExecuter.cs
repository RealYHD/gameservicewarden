using GameServiceWarden.InteractionAPI.Module;

namespace GameServiceWarden.Core.Module
{
    public interface IServiceManagerActionExecuter
    {
        void ExecuteAction(ServiceManagerAction action);
        void View();
    }
}