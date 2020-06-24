using System;

namespace DNSMadeEasy
{
	internal static class UnixTime
	{
		public static DateTime Epoch { get; } = new DateTime(1970, 01, 01, 00, 00, 00, DateTimeKind.Utc);

		public static DateTime FromMillisecondTimestamp(long timestamp) => Epoch + TimeSpan.FromMilliseconds(timestamp);
	}
}
