using System;

namespace GameServiceWarden.API.Communicable
{
    public enum CommunicableType : uint
    {
        Disconnect,
        Connect,
        Service,
        UnexpectedCommunication
    }
}