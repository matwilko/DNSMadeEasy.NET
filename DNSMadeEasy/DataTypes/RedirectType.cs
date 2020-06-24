using System;
using System.Collections.Generic;
using System.Diagnostics;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	[DebuggerDisplay("{DebuggerDisplay, nq}")]
	public readonly struct RedirectType : IEquatable<RedirectType>
	{
		private readonly byte value;

		private RedirectType(byte value)
		{
			this.value = value;
		}

		public static RedirectType HiddenFrameMasked => default(RedirectType);
		public static RedirectType Standard302       => new RedirectType(1);
		public static RedirectType Standard301       => new RedirectType(2);

		[JsonParseMethod(writeMethod: nameof(ToString))]
		public static RedirectType Parse(string type)
		{
			if (type is null)
				throw new ArgumentNullException(nameof(type));
			else if (type.Equals("Hidden Frame Masked", StringComparison.OrdinalIgnoreCase))
				return HiddenFrameMasked;
			else if (type.Equals("Standard - 302", StringComparison.OrdinalIgnoreCase))
				return Standard302;
			else if (type.Equals("Standard - 301", StringComparison.OrdinalIgnoreCase))
				return Standard301;
			else
				throw new FormatException("Unrecognised redirect type");
		}

#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations

		public override string ToString() => value switch
		{
			0 => "Hidden Frame Masked",
			1 => "Standard - 302",
			2 => "Standard - 301",
			_ => throw new InvalidOperationException("Unknown value")
		};

		private string DebuggerDisplay => value switch
		{
			0 => nameof(HiddenFrameMasked),
			1 => nameof(Standard301),
			2 => nameof(Standard302),
			_ => throw new InvalidOperationException("Unknown value")
		};

#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations

		public bool Equals(RedirectType other) => value == other.value;
		public override bool Equals(object? obj) => obj is RedirectType other && Equals(other);
		public override int GetHashCode() => value.GetHashCode();
		public static bool operator ==(RedirectType left, RedirectType right) => left.Equals(right);
		public static bool operator !=(RedirectType left, RedirectType right) => !left.Equals(right);

		private sealed class RedirectTypeEqualityComparer : IEqualityComparer<RedirectType>
		{
			public bool Equals(RedirectType x, RedirectType y) => x == y;
			public int GetHashCode(RedirectType obj) => obj.GetHashCode();
		}

		public static IEqualityComparer<RedirectType> EqualityComparer { get; } = new RedirectTypeEqualityComparer();
	}
}