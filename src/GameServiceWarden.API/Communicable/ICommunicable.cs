using System;

namespace GameServiceWarden.API.Communicable
{
    public interface ICommunicable
    {
        CommunicableType Type { get; }
    }
}