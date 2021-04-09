using System.Collections.Generic;

namespace GameServiceWarden.API.Games
{
    public struct ServiceManagerState
    {
        public bool delta;
        public bool subtract;
        public ICollection<string> services;
        public ICollection<string> running;
        public ICollection<string> modules;
        public IReadOnlyDictionary<string, string> logs;
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> serviceOptions;
    }
}