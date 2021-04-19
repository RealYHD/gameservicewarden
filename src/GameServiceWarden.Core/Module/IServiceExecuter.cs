using GameServiceWarden.API.Games;

namespace GameServiceWarden.Core.Module
{
    public interface IServiceManagerActionExecuter
    {
        void ExecuteAction(ServiceManagerAction action);
    }
}