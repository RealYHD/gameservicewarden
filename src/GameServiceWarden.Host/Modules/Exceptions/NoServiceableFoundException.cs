using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace GameServiceWarden.Host.Modules.Exceptions
{
    [Serializable]
    public class NoServiceableFoundException : Exception
    {
        public NoServiceableFoundException(string message) : base(message) { }
        public NoServiceableFoundException(string message, Exception inner) : base(message, inner) { }
        protected NoServiceableFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}