using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DNSMadeEasy.Json
{
	internal sealed partial class CustomJsonConverterFactory : JsonConverterFactory
	{
		public static JsonConverterFactory Instance { get; } = new CustomJsonConverterFactory();

		private ImmutableDictionary<Type, JsonConverter> Converters { get; }

		private CustomJsonConverterFactory()
		{
			Converters = CreateConverters();
		}

		public override bool CanConvert(Type typeToConvert) => Converters.ContainsKey(typeToConvert);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => Converters[typeToConvert];

		private static JsonConverter<T>? GetConverter<T>(JsonSerializerOptions options)
		{
			if (typeof(T) == typeof(object))
				return null;

			return options?.GetConverter(typeof(T)) as JsonConverter<T>;
		}

		private static T GetValue<T>(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var converter = GetConverter<T>(options);
			return converter != null
				? converter.Read(ref reader, typeof(T), options)
				: JsonSerializer.Deserialize<T>(ref reader, options);
		}

		[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Workaround")]
		private static Type GetAnonymousType<T>(T _) => typeof(T);

		[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Activator")]
		private sealed partial class ValueConverter : JsonConverter<object>
		{
			private static ImmutableDictionary<Type, Action<Utf8JsonWriter, object, JsonSerializerOptions>> Delegates { get; } = CreateDelegates();
			private Action<Utf8JsonWriter, object, JsonSerializerOptions> WriteAction { get; }

			public ValueConverter(Type t)
			{
				WriteAction = Delegates[t];
			}

			public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
			public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options) => WriteAction(writer, value, options);

			private static TAnon Cast<TAnon>(TAnon _, object obj) => (TAnon)obj;
		}
	}
}
