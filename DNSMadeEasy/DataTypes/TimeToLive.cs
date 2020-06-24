using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public readonly struct TimeToLive : IEquatable<TimeToLive>, IComparable<TimeToLive>, IComparable
	{
		private readonly int value;

		private TimeToLive(int value)
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), "TTL must be positive");

			this.value = value;
		}

		[JsonParseMethod(writeMethod: nameof(ToInt32))]
		public static TimeToLive FromInt32(int value) => new TimeToLive(value);
		public static TimeToLive FromTimeSpan(TimeSpan value)
		{
			if (value < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), "TTL must be positive");

			var seconds = Math.Floor(value.TotalSeconds);

			if (seconds > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(value), $"Time span is too large, it must be less than or equal to {int.MaxValue} seconds");

			return new TimeToLive((int)seconds);
		}

		public static implicit operator int(TimeToLive ttl) => ttl.value;
		public static implicit operator TimeToLive(int ttl) => new TimeToLive(ttl);
		public static implicit operator TimeSpan(TimeToLive ttl) => TimeSpan.FromSeconds(ttl.value);
		public static explicit operator TimeToLive(TimeSpan ttl) => FromTimeSpan(ttl);

		public static TimeToLive Zero     => default(TimeToLive);
		public static TimeToLive MinValue => Zero;
		public static TimeToLive MaxValue => new TimeToLive(int.MaxValue);

		public override string ToString() => value.ToString("D", CultureInfo.InvariantCulture);
		public int ToInt32() => value;
		public TimeSpan ToTimeSpan() => TimeSpan.FromSeconds(value);

		private string DebuggerDisplay => $"{value:D} ({((TimeSpan)this):c})";

		public bool Equals(TimeToLive other) => value == other.value;
		public override bool Equals(object? obj) => obj is TimeToLive other && Equals(other);
		public override int GetHashCode() => value;
		public static bool operator ==(TimeToLive left, TimeToLive right) => left.Equals(right);
		public static bool operator !=(TimeToLive left, TimeToLive right) => !left.Equals(right);

		public static IEqualityComparer<TimeToLive> EqualityComparer { get; } = new TimeToLiveEqualityComparer();

		private sealed class TimeToLiveEqualityComparer : IEqualityComparer<TimeToLive>
		{
			public bool Equals(TimeToLive x, TimeToLive y) => x.Equals(y);
			public int GetHashCode(TimeToLive obj) => obj.GetHashCode();
		}

		public int CompareTo(TimeToLive other) => value.CompareTo(other.value);

		public int CompareTo(object? obj)
		{
			if (ReferenceEquals(null, obj))
				return 1;
			return obj is TimeToLive other
				? CompareTo(other)
				: throw new ArgumentException($"Object must be of type {nameof(TimeToLive)}");
		}

		public static bool operator <(TimeToLive left, TimeToLive right) => left.value < right.value;
		public static bool operator >(TimeToLive left, TimeToLive right) => left.value > right.value;
		public static bool operator <=(TimeToLive left, TimeToLive right) => left.value <= right.value;
		public static bool operator >=(TimeToLive left, TimeToLive right) => left.value >= right.value;

		private sealed class TimeToLiveComparer : IComparer<TimeToLive>
		{
			public int Compare(TimeToLive x, TimeToLive y) => x.value.CompareTo(y.value);
		}

		public static IComparer<TimeToLive> Comparer { get; } = new TimeToLiveComparer();
	}
}