using System;
using System.Runtime.Serialization;

namespace App.Boilerplate.Core.Routing
{
    [Serializable]
    public class PageTreeRoutingException : Exception
    {
        public PageTreeRoutingException(string message) : base(message)
        {
        }

        public PageTreeRoutingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PageTreeRoutingException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}