namespace GameServiceWarden.API.Games
{
    public struct ServiceManagerAction
    {
        public enum Type
        {

            Start,
            Stop,
            CreateService,
            DeleteService,
            ExecuteCommand,
            SetServiceOption,
            View
        }

        public string assemblyName;
        public string moduleName;
        public string serviceName;
        public string option;
        public string command;
        public string value;
        public Type action;
    }
}