namespace GameServiceWarden.ModuleAPI
{
    public interface IConfigurable
    {
        string OptionName { get; }
        bool SetValue(string value);
        string GetValue();
    }
}