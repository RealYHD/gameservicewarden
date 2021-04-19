using System.Collections.Generic;
using GameServiceWarden.API.Module;

namespace GameServiceWarden.Core.Tests.Modules.Games
{
    public class FakeServiceModule : IServiceModule
    {
        public string Name => "FakeModule";

        public string Description => "A fake module for testing.";

        private IServiceConfigurable[] configurables;
        public FakeServiceModule(params IServiceConfigurable[] configurables)
        {
            this.configurables = configurables;
        }

        public IEnumerable<string> Authors { get; private set; } = new string[] { "FakeAuthor", "FakeAuthor2" };

        public IService InstantiateService(string workspace)
        {
            return new FakeService(configurables);
        }
    }
}