namespace GameServiceWarden.Core.UI
{
    public interface ITextCommand
    {
        string Prefix { get; }
        string Help{ get; }
        bool Validate(string input);
        void Execute(string input);
    }
}