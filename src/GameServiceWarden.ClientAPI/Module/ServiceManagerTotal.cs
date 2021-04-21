using System.Collections.Generic;

namespace GameServiceWarden.ClientAPI.Module
{
    public struct ServiceManagerTotal
    {
        public ICollection<string> services;
        public ICollection<string> running;
        public ICollection<string> modules;
        public IReadOnlyDictionary<string, byte[]> logs;
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> serviceOptions;
    }
}