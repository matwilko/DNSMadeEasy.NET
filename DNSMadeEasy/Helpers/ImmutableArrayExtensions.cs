using System.Collections.Immutable;

namespace DNSMadeEasy
{
	internal static class ImmutableArrayExtensions
	{
		public static ImmutableArray<T> EnsureInitialized<T>(this ImmutableArray<T> array)
		{
			return !array.IsDefaultOrEmpty
				? array
				: ImmutableArray<T>.Empty;
		}

		public static ImmutableArray<T> EnsureInitialized<T>(this ImmutableArray<T>? array)
		{
			return array.HasValue && !array.Value.IsDefaultOrEmpty
				? array.Value
				: ImmutableArray<T>.Empty;
		}
	}
}
