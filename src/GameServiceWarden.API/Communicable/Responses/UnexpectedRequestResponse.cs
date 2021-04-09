namespace GameServiceWarden.API.Communicable.Responses
{
    public struct UnexpectedRequestResponse : ICommunicable
    {
        public CommunicableType origin;
        public string message;

        public CommunicableType Type => CommunicableType.UnexpectedCommunication;
    }
}