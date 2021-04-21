using System.Collections.Generic;

namespace GameServiceWarden.InteractionAPI.Module
{
    public struct ServiceManagerDelta
    {
        public bool subtract;
        public string service;
        public string running;
        public string modules;
        public byte[] logs;
        public string optionName;
        public string optionValue;
    }
}