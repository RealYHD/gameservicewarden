namespace GameServiceWarden.Core.Preferences
{
    public interface IPersistable
    {
        public void Save();
        public void Load();
    }
}