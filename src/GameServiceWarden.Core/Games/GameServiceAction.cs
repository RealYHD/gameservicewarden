using GameServiceWarden.ModuleAPI;

namespace GameServiceWarden.Core.Games
{
    public struct ServiceAction
    {
        public string AssemblyName { get; set; }
        public string ModuleName { get; set; }
        public string ServiceName { get; set; }
        public IGameServiceModule Module { get; set; }
        public GameServiceActions Action { get; set; }
    }
}