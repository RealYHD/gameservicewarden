namespace GameServiceWarden.Host.Modules.Exceptions
{
    [System.Serializable]
    public class CorruptedServiceInfoException : System.Exception
    {
        public CorruptedServiceInfoException() { }
        public CorruptedServiceInfoException(string message) : base(message) { }
        public CorruptedServiceInfoException(string message, System.Exception inner) : base(message, inner) { }
        protected CorruptedServiceInfoException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}