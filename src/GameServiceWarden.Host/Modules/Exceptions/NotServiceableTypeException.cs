using System;

namespace GameServiceWarden.Host.Modules.Exceptions
{
    [System.Serializable]
    public class NotServiceableTypeException : Exception
    {
        public NotServiceableTypeException() { }
        public NotServiceableTypeException(string message) : base(message) { }
        public NotServiceableTypeException(string message, System.Exception inner) : base(message, inner) { }
        protected NotServiceableTypeException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}