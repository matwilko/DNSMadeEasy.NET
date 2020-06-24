using System;
using System.Globalization;

namespace DNSMadeEasy
{
	#if NETSTANDARD2_0
	internal static class StringExtensions
	{
		public static bool Contains(this string str, char chr, StringComparison comparisonType) => str.IndexOf(chr, comparisonType) >= 0;
		
		public static bool Contains(this string str, string searchStr, StringComparison comparisonType) => str.IndexOf(searchStr, comparisonType) >= 0;

		public static int IndexOf(this string str, char value, StringComparison comparisonType)
		{
			switch (comparisonType)
			{
				case StringComparison.CurrentCulture:
				case StringComparison.CurrentCultureIgnoreCase:
					return CultureInfo.CurrentCulture.CompareInfo.IndexOf(str, value, GetCaseCompareOfComparisonCulture(comparisonType));

				case StringComparison.InvariantCulture:
				case StringComparison.InvariantCultureIgnoreCase:
					return CultureInfo.InvariantCulture.CompareInfo.IndexOf(str, value, GetCaseCompareOfComparisonCulture(comparisonType));

				#pragma warning disable CA1307 // Could vary based on user's locale settings - no it couldn't...
				case StringComparison.Ordinal:
					return str.IndexOf(value);
				#pragma warning restore CA1307

				case StringComparison.OrdinalIgnoreCase:
					return CultureInfo.InvariantCulture.CompareInfo.IndexOf(str, value, CompareOptions.OrdinalIgnoreCase);

				default:
					throw new ArgumentException("The string comparison type passed in is currently not supported.", nameof(comparisonType));
			}

			static CompareOptions GetCaseCompareOfComparisonCulture(StringComparison comparisonType)
			{
				return (CompareOptions)((int)comparisonType & (int)CompareOptions.IgnoreCase);
			}
		}
		
	}
	#endif
}
