using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DNSMadeEasy
{
	#if NETSTANDARD2_0
	[Serializable]
	#endif
	public sealed class ClockSkewException : Exception
	{
		public ClockSkewException()
		{
		}

		public ClockSkewException(string message) : base(message)
		{
		}

		public ClockSkewException(string message, Exception innerException) : base(message, innerException)
		{
		}

		#if NETSTANDARD2_0
		private ClockSkewException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		
#endif
	}
}
