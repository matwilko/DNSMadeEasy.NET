using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DNSMadeEasy
{
	internal sealed class DnsRecordJsonConverter : JsonConverter<DnsRecord>
	{
		public override DnsRecord Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			if (!document.RootElement.TryGetProperty("type", out var typeValue) || typeValue.ValueKind != JsonValueKind.String)
				throw new FormatException("Could not parse a DNS record because it is missing or has a non-string value for `type`");

			var type = typeValue.GetString();

			using var buffer = new ArrayBufferWriter((int)reader.BytesConsumed);
			using var writer = new Utf8JsonWriter(buffer);
			document.WriteTo(writer);
			writer.Flush();

			return type switch
			{
				"A"       => JsonSerializer.Deserialize<ARecord>(buffer.WrittenSpan, options)!,
				"AAAA"    => JsonSerializer.Deserialize<AAAARecord>(buffer.WrittenSpan, options)!,
				"ANAME"   => JsonSerializer.Deserialize<ANameRecord>(buffer.WrittenSpan, options)!,
				"CNAME"   => JsonSerializer.Deserialize<CNameRecord>(buffer.WrittenSpan, options)!,
				"HTTPRED" => JsonSerializer.Deserialize<HttpRedirectionRecord>(buffer.WrittenSpan, options)!,
				"MX"      => JsonSerializer.Deserialize<MXRecord>(buffer.WrittenSpan, options)!,
				"NS"      => JsonSerializer.Deserialize<NSRecord>(buffer.WrittenSpan, options)!,
				"PTR"     => JsonSerializer.Deserialize<PTRRecord>(buffer.WrittenSpan, options)!,
				"SRV"     => JsonSerializer.Deserialize<SRVRecord>(buffer.WrittenSpan, options)!,
				"TXT"     => JsonSerializer.Deserialize<TXTRecord>(buffer.WrittenSpan, options)!,
				"SPF"     => JsonSerializer.Deserialize<SPFRecord>(buffer.WrittenSpan, options)!,
				_         => throw new FormatException($"Unrecognised record type `{type}`")
			};
		}

		public override void Write(Utf8JsonWriter writer, DnsRecord value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}