using System.Diagnostics.CodeAnalysis;

namespace DNSMadeEasy
{
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Extremely thin wrapper - should not have any extraneous members")]
	[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Extremely thin wrapper - should not have any extraneous members")]
	// NOTE: This should only be used in anonymous types for immediate serialization onto the wire
	// It is not generally usable outside of this context, as it cannot be deserialized,
	// and replaces the built-in serialization for any class that contains a property of this type
	public readonly struct Value<T>
	{
		public bool HasValue { get; }
		private readonly T value;

		public Value(T value)
		{
			this.value = value;
			this.HasValue = true;
		}

		public static implicit operator T(Value<T> value) => value.value;

		public static implicit operator Value<T>(T value) => new Value<T>(value);
	}
}