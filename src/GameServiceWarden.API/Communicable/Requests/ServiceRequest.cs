using GameServiceWarden.API.Games;

namespace GameServiceWarden.API.Communicable.Requests
{
    public struct ServiceRequest : ICommunicable
    {
        public ServiceManagerAction serviceManagerAction;

        public CommunicableType Type => CommunicableType.Service;
    }
}