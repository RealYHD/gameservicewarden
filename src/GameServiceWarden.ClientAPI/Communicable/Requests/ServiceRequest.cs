using GameServiceWarden.ClientAPI.Module;

namespace GameServiceWarden.ClientAPI.Requests
{
    public struct ServiceRequest : ICommunicable
    {
        public ServiceManagerAction serviceManagerAction;

        public CommunicableType Type => CommunicableType.Service;
    }
}