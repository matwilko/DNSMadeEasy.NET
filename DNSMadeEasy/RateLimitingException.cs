using System;
using System.Runtime.Serialization;

namespace DNSMadeEasy
{
	#if NETSTANDARD2_0
	[Serializable]
	#endif
	public sealed class RateLimitingException : Exception
	{
		public RateLimitingException()
		{
		}

		public RateLimitingException(string message) : base(message)
		{
		}

		public RateLimitingException(string message, Exception innerException) : base(message, innerException)
		{
		}

		#if NETSTANDARD2_0
		private RateLimitingException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
		#endif
	}
}
