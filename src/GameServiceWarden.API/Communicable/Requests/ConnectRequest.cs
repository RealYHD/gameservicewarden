namespace GameServiceWarden.API.Communicable.Requests
{
    public struct ConnectRequest : ICommunicable
    {
        public string requestedIdentifier;
        public string programName;
        public string programAuthor;
        public string versionNumber;
        public string details;

        public CommunicableType Type => CommunicableType.Connect;
    }
}