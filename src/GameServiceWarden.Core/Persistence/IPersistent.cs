using System.Collections.Generic;
using GameServiceWarden.Core.Games;

namespace GameServiceWarden.Core.Persistence
{
    public interface IPersistent<V> : IReadOnlyPersistent<V>
    {
        new V this[string key] { get; set; }

        void Add(string key, V value);
        void Clear();
        bool Remove(string key);
    }
}