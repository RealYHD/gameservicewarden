using System;

namespace GameServiceWarden.InteractionAPI
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