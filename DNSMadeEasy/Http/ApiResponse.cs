using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CA1812 // Internal class that is never instantiated - Converters are Activator.CreateInstance'd

namespace DNSMadeEasy.Http
{
	internal sealed class ApiResponse<T>
	{
		public int Page { get; }
		public int TotalPages { get; }
		public int TotalRecords { get; }
		public ImmutableArray<T> Data { get; }

		public ApiResponse(int page, int totalPages, int totalRecords, ImmutableArray<T> data)
		{
			Page = page;
			TotalPages = totalPages;
			TotalRecords = totalRecords;
			Data = data;
		}
	}

	internal sealed class ApiSingleResponse<T>
	{
		public int Page { get; }
		public int TotalPages { get; }
		public int TotalRecords { get; }
		public T Data { get; }

		public ApiSingleResponse(int page, int totalPages, int totalRecords, T data)
		{
			Page = page;
			TotalPages = totalPages;
			TotalRecords = totalRecords;
			Data = data;
		}
	}

	internal sealed class ApiResponseConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert)
		{
			if (typeToConvert is null)
				throw new ArgumentNullException(nameof(typeToConvert));

			if (!typeToConvert.IsGenericType)
				return false;

			var typeDef = typeToConvert.GetGenericTypeDefinition();
			return typeDef == typeof(ApiResponse<>) || typeDef == typeof(ApiSingleResponse<>);
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (typeToConvert is null)
				throw new ArgumentNullException(nameof(typeToConvert));

			var typeDef = typeToConvert.GetGenericTypeDefinition();
			var typeArg = typeToConvert.GetGenericArguments().Single();

			if (typeDef == typeof(ApiResponse<>))
			{
				var converterType = typeof(ApiResponseConverter<>).MakeGenericType(typeArg);
				return (JsonConverter)Activator.CreateInstance(converterType)!;
			}
			else if (typeDef == typeof(ApiSingleResponse<>))
			{
				var converterType = typeof(ApiSingleResponseConverter<>).MakeGenericType(typeArg);
				return (JsonConverter) Activator.CreateInstance(converterType)!;
			}
			else
			{
				throw new ArgumentException("Cannot create converter for this type", nameof(typeToConvert));
			}
		}

		private static T GetValue<T>(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var converter = typeof(T) != typeof(object)
				? options?.GetConverter(typeof(T)) as JsonConverter<T>
				: null;

			return converter != null
				? converter.Read(ref reader, typeof(T), options)
				: JsonSerializer.Deserialize<T>(ref reader, options);
		}

		private sealed class ApiResponseConverter<T> : JsonConverter<ApiResponse<T>>
		{
			public override ApiResponse<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

				int? page = default;
				int? totalPages = default;
				int? totalRecords = default;
				ImmutableArray<T>? data = default;
				while (reader.Read())
				{
					if (reader.TokenType == JsonTokenType.EndObject) break;

					var propertyName = reader.GetString();
					reader.Read();
					switch (propertyName)
					{
						case "page":
							page = reader.GetInt32();
							break;
						case "totalPages":
							totalPages = reader.GetInt32();
							break;
						case "totalRecords":
							totalRecords = reader.GetInt32();
							break;
						case "data":
							data = GetValue<ImmutableArray<T>>(ref reader, options);
							break;
					}
				}
				if (page is null) throw new JsonException();
				if (totalPages is null) throw new JsonException();
				if (totalRecords is null) throw new JsonException();
				if (data is null) throw new JsonException();
				return new ApiResponse<T>((int)page, (int)totalPages, (int)totalRecords, (ImmutableArray<T>)data);
			}

			public override void Write(Utf8JsonWriter writer, ApiResponse<T> value, JsonSerializerOptions options)
			{
				throw new NotImplementedException();
			}
		}

		private sealed class ApiSingleResponseConverter<T> : JsonConverter<ApiSingleResponse<T>>
		{
			public override ApiSingleResponse<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

				int? page = default;
				int? totalPages = default;
				int? totalRecords = default;
				T data = default;
				while (reader.Read())
				{
					if (reader.TokenType == JsonTokenType.EndObject) break;

					var propertyName = reader.GetString();
					reader.Read();
					switch (propertyName)
					{
						case "page":
							page = reader.GetInt32();
							break;
						case "totalPages":
							totalPages = reader.GetInt32();
							break;
						case "totalRecords":
							totalRecords = reader.GetInt32();
							break;
						case "data":
							if (reader.TokenType != JsonTokenType.StartArray)
								throw new JsonException();

							reader.Read();
							data = GetValue<T>(ref reader, options);

							if (reader.TokenType != JsonTokenType.EndArray)
								throw new JsonException();

							break;
					}
				}
				if (page is null) throw new JsonException();
				if (totalPages is null) throw new JsonException();
				if (totalRecords is null) throw new JsonException();
				if (data is null) throw new JsonException();
				return new ApiSingleResponse<T>((int)page, (int)totalPages, (int)totalRecords, data);
			}

			public override void Write(Utf8JsonWriter writer, ApiSingleResponse<T> value, JsonSerializerOptions options)
			{
				throw new NotImplementedException();
			}
		}
	}
}
