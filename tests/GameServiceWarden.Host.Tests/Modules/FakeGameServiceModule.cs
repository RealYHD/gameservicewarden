using System.Collections.Generic;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Host.Tests.Modules
{
    public class FakeGameServiceModule : IGameServiceModule
    {
        public string Name => "FakeModule";

        public string Description => "A fake module for testing.";

        private IConfigurable[] configurables;
        public FakeGameServiceModule(params IConfigurable[] configurables)
        {
            this.configurables = configurables;
        }

        public IEnumerable<string> Authors { get; private set; } = new string[] { "FakeAuthor", "FakeAuthor2" };

        public IGameService CreateGameService()
        {
            return new FakeService(configurables);
        }
    }
}