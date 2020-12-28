namespace GameServiceWarden.ModuleAPI
{
    public interface IGameConfigurable
    {
        string OptionName { get; }
        bool SetValue(string value);
        string GetValue();
    }
}