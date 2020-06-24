using System;
using System.Diagnostics;
using DNSMadeEasy.Json;

namespace DNSMadeEasy
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public readonly struct RecordSource : IEquatable<RecordSource>
	{
		private readonly bool isFromTemplate;

		private RecordSource(bool isFromTemplate)
		{
			this.isFromTemplate = isFromTemplate;
		}

		public static RecordSource DomainSpecific => default(RecordSource);
		public static RecordSource FromTemplate   => new RecordSource(true);

		public bool Equals(RecordSource other) => isFromTemplate == other.isFromTemplate;
		public override bool Equals(object? obj) => obj is RecordSource other && Equals(other);
		public override int GetHashCode() => isFromTemplate.GetHashCode();
		public static bool operator ==(RecordSource left, RecordSource right) => left.Equals(right);
		public static bool operator !=(RecordSource left, RecordSource right) => !left.Equals(right);

		[JsonParseMethod(writeMethod: nameof(ToInt))]
		public static RecordSource ParseFromInt(int value) => value switch
		{
			0 => FromTemplate,
			1 => DomainSpecific,
			_ => throw new FormatException("Unknown value for record source")
		};

		public int ToInt() => isFromTemplate
			? 0
			: 1;

		public override string ToString() => isFromTemplate
			? "From Template"
			: "Domain Specific";

		private string DebuggerDisplay => isFromTemplate
			? nameof(FromTemplate)
			: nameof(DomainSpecific);
	}
}