using System.Collections.Generic;

namespace GameServiceWarden.Core.Persistence
{
    public interface IPersistentDictionary<V> : IDictionary<string, V>
    {
        string MapDirectory { get; }
        string GetPathForKey(string key);
    }
}