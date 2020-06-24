using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CA1812 // Internal class that is never instantiated - Converters are Activator.CreateInstance'd

namespace DNSMadeEasy.Json
{
	internal sealed class NullableImmutableArrayConverterFactory : JsonConverterFactory
	{
		public static bool IsNeeded { get; } = !CanLoadedDeserializerHandle();

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catching is the only way to determine this, and don't want to accidentally be too specific with an exception type.")]
		private static bool CanLoadedDeserializerHandle()
		{
			try
			{
				JsonSerializer.Deserialize<ImmutableArray<int>?>("[]");
				return true;
			}
			catch
			{
				return false;
			}
		}

		public override bool CanConvert(Type typeToConvert)
		{
			if (typeToConvert is null)
				throw new ArgumentNullException(nameof(typeToConvert));

			var nullableType = Nullable.GetUnderlyingType(typeToConvert);
			var arrayElementType = GetImmutableArrayElementType(nullableType);
			return arrayElementType != null;
		}

		private static Type? GetImmutableArrayElementType(Type? type)
		{
			if (type is null)
				return null;

			if (!type.IsGenericType)
				return null;

			if (!type.IsValueType)
				return null;

			var genericTypeDef = type.GetGenericTypeDefinition();
			return genericTypeDef == typeof(ImmutableArray<>)
				? type.GetGenericArguments()[0]
				: null;
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (typeToConvert is null)
				throw new ArgumentNullException(nameof(typeToConvert));

			var elementType = GetImmutableArrayElementType(Nullable.GetUnderlyingType(typeToConvert));

			if (elementType is null)
				throw new ArgumentException("Cannot create create converter for this type", nameof(typeToConvert));

			return (JsonConverter) Activator.CreateInstance(typeof(NullableImmutableArrayConverter<>).MakeGenericType(elementType))!;
		}

		private sealed class NullableImmutableArrayConverter<T> : JsonConverter<ImmutableArray<T>?>
		{
			public override ImmutableArray<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.Null)
					return null;
				else if (reader.TokenType == JsonTokenType.StartArray)
					return JsonSerializer.Deserialize<ImmutableArray<T>>(ref reader, options);
				else
					throw new JsonException();
			}

			public override void Write(Utf8JsonWriter writer, ImmutableArray<T>? value, JsonSerializerOptions options)
			{
				throw new NotImplementedException();
			}
		}
	}
}
