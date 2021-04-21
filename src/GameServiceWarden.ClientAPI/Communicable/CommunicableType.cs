using System;

namespace GameServiceWarden.ClientAPI
{
    public enum CommunicableType : uint
    {
        Disconnect,
        Connect,
        Service,
        UnexpectedCommunication
    }
}