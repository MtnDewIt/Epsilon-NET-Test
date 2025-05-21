using System;
using System.Runtime.Serialization;

namespace CacheEditor.RTE
{
    [Serializable]
    public class RteTargetNotAvailableException : RteProviderException
    {
        private static readonly string DefaultMessage = "Target not available";

        public RteTargetNotAvailableException(): base(DefaultMessage)
        {
        }

        public RteTargetNotAvailableException(string message) : base(message)
        {
        }

        public RteTargetNotAvailableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public RteTargetNotAvailableException(IRteProvider provider, string message) : base(provider, message)
        {
        }

        public RteTargetNotAvailableException(IRteProvider provider, string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RteTargetNotAvailableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
