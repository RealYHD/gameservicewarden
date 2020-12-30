namespace GameServiceWarden.Core.Persistence
{
    public interface IPersistentDictionaryWriter<V>
    {
        void Write(string path, V value);
    }
}