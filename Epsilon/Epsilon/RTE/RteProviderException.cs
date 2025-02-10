using System;
using System.Runtime.Serialization;

namespace Epsilon.RTE
{
	[Serializable]
	public class RteProviderException : Exception
	{
		public IRteProvider Provider { get; }

		public RteProviderException() {
		}

		public RteProviderException(string message)
			: base(message) {
		}

		public RteProviderException(string message, Exception innerException)
			: base(message, innerException) {
		}

		public RteProviderException(IRteProvider provider, string message)
			: base(message) {
			Provider = provider;
		}

		public RteProviderException(IRteProvider provider, string message, Exception innerException)
			: base(message, innerException) {
			Provider = provider;
		}

		protected RteProviderException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}
	}
}
