namespace GameServiceWarden.ModuleAPI
{
    public interface IServiceConfigurable
    {
        string OptionName { get; }
        string GetValue();
        bool SetValue(string value);
    }
}