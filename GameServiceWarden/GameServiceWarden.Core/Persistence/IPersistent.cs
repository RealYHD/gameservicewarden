using System.Collections.Generic;
using GameServiceWarden.Core.Module;

namespace GameServiceWarden.Core.Persistence
{
    public interface IPersistent<V> : IReadOnlyPersistent<V>
    {
        new V this[string key] { get; set; }

        void AddToPersistence(string key, V value);
        void Clear();
        bool Delete(string key);
    }
}