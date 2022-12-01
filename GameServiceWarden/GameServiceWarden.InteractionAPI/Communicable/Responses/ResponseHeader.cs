using System;

namespace GameServiceWarden.InteractionAPI.Communicable.Responses
{
    public static class ResponseHeader
    {
        public static byte[] Encode(CommunicableType type, uint length) {
            byte[] res = new byte[sizeof(uint) + sizeof(uint)];
            BitConverter.GetBytes((uint)type).CopyTo(res, 0);
            BitConverter.GetBytes(length).CopyTo(res, sizeof(uint));
            return res;
        }
    }
}