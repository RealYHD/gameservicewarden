using GameServiceWarden.API.Games;

namespace GameServiceWarden.Core.Games
{
    public interface IServiceManagerActionExecuter
    {
        void ExecuteAction(ServiceManagerAction action);
    }
}