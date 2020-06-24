using System.Collections.Generic;

namespace DNSMadeEasy.Helpers
{
	internal static class TExtensions
	{
		public static bool IsBetween<T>(this T value, T lowerBound, T upperBound)
		{
			return Comparer<T>.Default.Compare(value, lowerBound) >= 0
				&& Comparer<T>.Default.Compare(value, upperBound) < 0;
		}
	}
}
