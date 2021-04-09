using GameServiceWarden.API.Module;

namespace GameServiceWarden.Core.Tests.Modules.Games
{
    public class FakeServiceConfigurable : IServiceConfigurable
    {
        private string value;
        public string OptionName { get; private set; }

        public FakeServiceConfigurable(string optionName)
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