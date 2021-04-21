using System;

namespace GameServiceWarden.ClientAPI
{
    public enum CommunicableType : uint
    {
        Disconnect,
        Connect,
        View,
        Delta,
        UnexpectedCommunication
    }
}