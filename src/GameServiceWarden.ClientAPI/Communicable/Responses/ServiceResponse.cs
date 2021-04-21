using GameServiceWarden.ClientAPI.Module;

namespace GameServiceWarden.ClientAPI.Responses
{
    public struct ServiceResponse : ICommunicable
    {
        public ServiceManagerState gameServiceDelta;

        public CommunicableType Type => CommunicableType.Service;
    }
}