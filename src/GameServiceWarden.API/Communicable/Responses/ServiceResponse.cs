using GameServiceWarden.API.Games;

namespace GameServiceWarden.API.Communicable.Responses
{
    public struct ServiceResponse : ICommunicable
    {
        public ServiceManagerState gameServiceDelta;

        public CommunicableType Type => CommunicableType.Service;
    }
}