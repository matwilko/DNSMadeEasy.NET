using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DNSMadeEasy.JsonCodegen
{
	internal static class TLDs
	{
		public static async Task<string> Generate(string outputPath, string tldJsonPath)
		{
			using (var tldJsonFile = File.OpenRead(tldJsonPath))
			using (var outputFile = new FileStream(Path.Combine(outputPath, "TLDs.cs"), FileMode.Create, FileAccess.Write))
			using (var writer = new StreamWriter(outputFile, leaveOpen: true))
			{
				var tldsObject = await JsonSerializer.DeserializeAsync<TldsObject>(tldJsonFile);

				writer.WriteLine("using System;");
				writer.WriteLine("using System.Diagnostics;");
				writer.WriteLine("using System.Collections.Generic;");
				writer.WriteLine("using System.Globalization;");

				writer.WriteLine();
				writer.WriteLine("#pragma warning disable IDE1006 // Naming Styles");
				writer.WriteLine("#pragma warning disable CA1720 // Identifier contains type name");
				writer.WriteLine();
				
				writer.WriteLine($"namespace DNSMadeEasy");
				writer.WriteLine("{");
				writer.WriteLine();
				writer.WriteLine($"partial struct DomainName");
				writer.WriteLine("{");

				foreach (var (tld, description) in tldsObject.generic)
				{
					writer.WriteLine($"/// <summary>{description}</summary>");
					writer.WriteLine($@"public static DomainName @{tld} => new DomainName(""{tld}"");");
					writer.WriteLine();
				}

				writer.WriteLine("public static class CountryCodes");
				writer.WriteLine("{");
				foreach (var (tld, (country, notes)) in tldsObject.countryCodes)
				{
					writer.WriteLine($"/// <summary>{country}</summary>");
					if (notes != null)
						writer.WriteLine($"/// <remarks>{notes}</remarks>");
					writer.WriteLine($@"public static DomainName @{tld} => new DomainName(""{tld}"");");
					writer.WriteLine();
				}

				writer.WriteLine("}");

				writer.WriteLine("public static class Geographic");
				writer.WriteLine("{");
				foreach (var (tld, (area, notes)) in tldsObject.geographic)
				{
					writer.WriteLine($"/// <summary>{area}</summary>");
					if (notes != null)
						writer.WriteLine($"/// <remarks>{notes}</remarks>");
					writer.WriteLine($@"public static DomainName @{tld} => new DomainName(""{tld}"");");
					writer.WriteLine();
				}

				writer.WriteLine("}");

				var allTlds = tldsObject.generic.Select(kvp => kvp.Key)
					.Concat(tldsObject.countryCodes.Select(cc => cc.Key))
					.Concat(tldsObject.geographic.Select(g => g.Key))
					.OrderBy(i => i)
					.ToList();

				writer.WriteLine($"private const int LongestKnownTld = {allTlds.Max(tld => tld.Length)};");
				writer.WriteLine();
				writer.WriteLine($"private static string[] KnownTlds {{ get; }} = {{ {string.Join(", ", allTlds.Select(tld => '"' + tld + '"'))} }};");

				writer.WriteLine("}");

				writer.WriteLine("}");

				return outputFile.Name;
			}
		}

		#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
		public sealed class TldsObject
		{
			public Dictionary<string, string> generic { get; set; }

			public Dictionary<string, CountryCodeTldDesc> countryCodes { get; set; }
			public Dictionary<string, GeographicTldDesc> geographic { get; set; }
		}
		#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


		[JsonConverter(typeof(CountryCodeTldDescConverter))]
		public sealed class CountryCodeTldDesc
		{
			public string Country { get; }
			public string Notes { get; }

			public CountryCodeTldDesc(string country)
			{
				Country = country;
			}

			public CountryCodeTldDesc(string country, string notes)
			{
				Country = country;
				Notes = notes;
			}

			public void Deconstruct(out string country, out string notes)
			{
				country = Country;
				notes = Notes;
			}
		}

		[JsonConverter(typeof(GeographicTldDescConverter))]
		public sealed class GeographicTldDesc
		{
			public string Area { get; }
			public string Notes { get; }

			public GeographicTldDesc(string area)
			{
				Area = area;
			}

			public GeographicTldDesc(string area, string notes)
			{
				Area = area;
				Notes = notes;
			}

			public void Deconstruct(out string area, out string notes)
			{
				area = Area;
				notes = Notes;
			}
		}

		public sealed class CountryCodeTldDescConverter : JsonConverter<CountryCodeTldDesc>
		{
			public override CountryCodeTldDesc Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.String)
				{
					return new CountryCodeTldDesc(reader.GetString());
				}
				else if (reader.TokenType == JsonTokenType.StartObject)
				{
					var desc = JsonSerializer.Deserialize<Desc>(ref reader, options);
					return new CountryCodeTldDesc(desc.country, desc.notes);
				}
				else
				{
					throw new InvalidOperationException();
				}
			}

			public override void Write(Utf8JsonWriter writer, CountryCodeTldDesc value, JsonSerializerOptions options)
			{
				throw new NotImplementedException();
			}

			private sealed class Desc
			{
				public string country { get; set; }
				public string notes { get; set; }
			}
		}

		public sealed class GeographicTldDescConverter : JsonConverter<GeographicTldDesc>
		{
			public override GeographicTldDesc Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.String)
				{
					return new GeographicTldDesc(reader.GetString());
				}
				else if (reader.TokenType == JsonTokenType.StartObject)
				{
					var desc = JsonSerializer.Deserialize<Desc>(ref reader, options);
					return new GeographicTldDesc(desc.area, desc.notes);
				}
				else
				{
					throw new InvalidOperationException();
				}
			}

			public override void Write(Utf8JsonWriter writer, GeographicTldDesc value, JsonSerializerOptions options)
			{
				throw new NotImplementedException();
			}

			private sealed class Desc
			{
				public string area { get; set; }
				public string notes { get; set; }
			}
		}
	}
}
