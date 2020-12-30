namespace GameServiceWarden.Core.Persistence
{
    public interface IPersistentDictionaryReader<V>
    {
        V Read(string path);
        bool IsValidValue(string path);
    }
}