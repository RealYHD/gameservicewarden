using System;
using System.IO.Pipes;

namespace GameServiceWarden.API.Communicable.Requests
{
    public static class RequestHeader
    {
        public static void Decode(byte[] header, out CommunicableType type, out uint length) {
            type = (CommunicableType) BitConverter.ToUInt32(header, 0);
            length = BitConverter.ToUInt32(header, sizeof(uint));
        }
    }
}