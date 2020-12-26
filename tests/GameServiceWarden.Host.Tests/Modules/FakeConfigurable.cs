using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Host.Tests.Modules
{
    public class FakeConfigurable : IConfigurable
    {
        private string value;
        public string OptionName { get; private set; }

        public FakeConfigurable(string optionName)
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