using System;
using System.Diagnostics;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public readonly struct GlobalTrafficDirectorLocation : IEquatable<GlobalTrafficDirectorLocation>
	{
		private readonly byte value;

		private GlobalTrafficDirectorLocation(byte value)
		{
			this.value = value;
		}

		public static GlobalTrafficDirectorLocation Default => default(GlobalTrafficDirectorLocation);
		public static GlobalTrafficDirectorLocation UsEast  => new GlobalTrafficDirectorLocation(1);
		public static GlobalTrafficDirectorLocation UsWest  => new GlobalTrafficDirectorLocation(2);
		public static GlobalTrafficDirectorLocation Europe  => new GlobalTrafficDirectorLocation(3);

		[JsonParseMethod(writeMethod: nameof(ToString))]
		public static GlobalTrafficDirectorLocation Parse(string location)
		{
			if (location is null)
				throw new ArgumentNullException(nameof(location));
			else if (location.Equals("DEFAULT", StringComparison.OrdinalIgnoreCase))
				return Default;
			else if (location.Equals("US_EAST", StringComparison.OrdinalIgnoreCase))
				return UsEast;
			else if (location.Equals("US_WEST", StringComparison.OrdinalIgnoreCase))
				return UsWest;
			else if (location.Equals("EUROPE", StringComparison.OrdinalIgnoreCase))
				return Europe;
			else
				throw new FormatException("Invalid Global Traffic Director Location");
		}

#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations

		public override string ToString() => value switch
		{
			0 => "DEFAULT",
			1 => "US_EAST",
			2 => "US_WEST",
			3 => "EUROPE",

			_ => throw new InvalidOperationException("Unknown value")

		};

		private string DebuggerDisplay => value switch
		{
			0 => nameof(Default),
			1 => nameof(UsEast),
			2 => nameof(UsWest),
			3 => nameof(Europe),
			_ => throw new InvalidOperationException("Unknown value")
		};

#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations

		public bool Equals(GlobalTrafficDirectorLocation other) => value == other.value;
		public override bool Equals(object? obj) => obj is GlobalTrafficDirectorLocation other && Equals(other);
		public override int GetHashCode() => value.GetHashCode();
		public static bool operator ==(GlobalTrafficDirectorLocation left, GlobalTrafficDirectorLocation right) => left.Equals(right);
		public static bool operator !=(GlobalTrafficDirectorLocation left, GlobalTrafficDirectorLocation right) => !left.Equals(right);
	}
}