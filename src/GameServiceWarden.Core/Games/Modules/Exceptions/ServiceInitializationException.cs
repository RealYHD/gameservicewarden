namespace GameServiceWarden.Core.Games.Modules.Exceptions
{
    [System.Serializable]
    public class ServiceInitializationException : System.Exception
    {
        public ServiceInitializationException() { }
        public ServiceInitializationException(string message) : base(message) { }
        public ServiceInitializationException(string message, System.Exception inner) : base(message, inner) { }
        protected ServiceInitializationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}