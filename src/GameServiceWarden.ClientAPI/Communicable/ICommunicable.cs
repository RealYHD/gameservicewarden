using System;

namespace GameServiceWarden.ClientAPI
{
    public interface ICommunicable
    {
        CommunicableType Type { get; }
    }
}