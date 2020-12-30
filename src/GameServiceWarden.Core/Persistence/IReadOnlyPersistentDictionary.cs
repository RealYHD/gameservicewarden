using System.Collections.Generic;

namespace GameServiceWarden.Core.Persistence
{
    public interface IReadOnlyPersistentDictionary<V> : IReadOnlyDictionary<string, V>
    {
        string MapDirectory { get; }
        string GetPathForKey(string key);
    }
}