using System.Security.Cryptography.X509Certificates;

namespace GameServiceWarden.API.Communicable.Responses
{
    public struct ConnectResponse : ICommunicable
    {
        public string identifier;
        public bool nameTaken;
        public bool invalidName;
        public string errorMsg;

        public CommunicableType Type => CommunicableType.Connect;
    }
}