using System;
using System.Runtime.Serialization;

namespace GameServiceWarden.Core.Games.Modules.Exceptions
{
    [System.Serializable]
    public class ModuleLoadException : Exception
    {
        public ModuleLoadException() { }
        public ModuleLoadException(string message) : base(message) { }
        public ModuleLoadException(string message, Exception inner) : base(message, inner) { }
        protected ModuleLoadException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}