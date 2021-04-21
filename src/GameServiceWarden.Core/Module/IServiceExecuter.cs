using GameServiceWarden.ClientAPI.Module;

namespace GameServiceWarden.Core.Module
{
    public interface IServiceManagerActionExecuter
    {
        void ExecuteAction(ServiceManagerAction action);
        void View();
    }
}