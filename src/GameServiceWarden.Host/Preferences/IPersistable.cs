namespace GameServiceWarden.Host.Preferences
{
    public interface IPersistable
    {
        public void Save();
        public void Load();
    }
}