namespace GameServiceWarden.ClientAPI.Module
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