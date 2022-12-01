using System.Security.Cryptography.X509Certificates;

namespace GameServiceWarden.InteractionAPI.Communicable.Responses
{
    public struct ConnectResponse
    {
        public string identifier;
        public bool nameTaken;
        public bool invalidName;
        public string errorMsg;
    }
}