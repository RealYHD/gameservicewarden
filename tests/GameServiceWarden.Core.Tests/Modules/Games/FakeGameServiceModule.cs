using System.Collections.Generic;
using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Core.Tests.Modules.Games
{
    public class FakeGameServiceModule : IGameServiceModule
    {
        public string Name => "FakeModule";

        public string Description => "A fake module for testing.";

        private IGameConfigurable[] configurables;
        public FakeGameServiceModule(params IGameConfigurable[] configurables)
        {
            this.configurables = configurables;
        }

        public IEnumerable<string> Authors { get; private set; } = new string[] { "FakeAuthor", "FakeAuthor2" };

        public IGameService CreateGameService()
        {
            return new FakeGameService(configurables);
        }

        public IGameService InstantiateGameService(string workspace, bool clean)
        {
            return new FakeGameService(configurables);
        }
    }
}