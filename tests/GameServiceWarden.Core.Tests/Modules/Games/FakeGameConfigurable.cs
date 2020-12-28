using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Core.Tests.Modules.Games
{
    public class FakeGameConfigurable : IGameConfigurable
    {
        private string value;
        public string OptionName { get; private set; }

        public FakeGameConfigurable(string optionName)
        {
            this.OptionName = optionName;
        }

        public string GetValue()
        {
            return value;
        }

        public bool SetValue(string value)
        {
            this.value = value;
            return true;
        }
    }
}