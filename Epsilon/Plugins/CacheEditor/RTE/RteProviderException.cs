using System;
using System.Runtime.Serialization;

namespace CacheEditor.RTE
{
    [Serializable]
    public class RteProviderException : System.Exception
    {
        public IRteProvider Provider { get; }

        public RteProviderException() : base() { }
        public RteProviderException(string message) : base(message) { }
        public RteProviderException(string message, Exception innerException)
        : base(message, innerException) { }

        public RteProviderException(IRteProvider provider, string message)
        : base(message)
        {
            Provider = provider;
        }

        public RteProviderException(IRteProvider provider, string message, System.Exception innerException)
        : base(message, innerException)
        {
            Provider = provider;
        }
    }

    [Serializable]
    public class RteTargetNotAvailableException : RteProviderException
    {
        private static readonly string DefaultMessage = "Target not available";

        public RteTargetNotAvailableException() : base(DefaultMessage) { }
        public RteTargetNotAvailableException(string message) : base(message) { }
        public RteTargetNotAvailableException(string message, System.Exception innerException)
        : base(message, innerException) { }

        public RteTargetNotAvailableException(IRteProvider provider, string message)
        : base(provider, message)
        {
            
        }

        public RteTargetNotAvailableException(IRteProvider provider, string message, System.Exception innerException)
        : base(message, innerException)
        {
            
        }
    }
}
