using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DNSMadeEasy.JsonCodegen
{
	internal static class Converters
	{
		public static string Generate(string outputPath, ImmutableArray<SyntaxTree> syntaxTrees, CSharpCompilation compilation)
		{
			using (var outputFile = new FileStream(Path.Combine(outputPath, "Converters.cs"), FileMode.Create, FileAccess.Write))
			using (var writer = new StreamWriter(outputFile, leaveOpen: true))
			{
				writer.WriteLine("using System;");
				writer.WriteLine("using System.Text.Json;");
				writer.WriteLine("using System.Text.Json.Serialization;");
				writer.WriteLine("using System.Collections.Immutable;");

				writer.WriteLine();

				writer.WriteLine("#pragma warning disable CA1801 // Unused parameter");

				writer.WriteLine();

				writer.WriteLine("namespace DNSMadeEasy.Json");
				writer.WriteLine("{");

				writer.WriteLine("partial class CustomJsonConverterFactory");
				writer.WriteLine("{");

				var jsonTypes = JsonAttributeScanningWalker.GetJsonTypes(syntaxTrees, compilation);
				var anonymousTypes = JsonAnonymousTypeWithValueScanningWalker.GetAnonymousTypes(syntaxTrees, compilation);
				
				writer.WriteLine($"private static ImmutableDictionary<Type, JsonConverter> CreateConverters()");
				writer.WriteLine("{");
				writer.WriteLine("var builder = ImmutableDictionary.CreateBuilder<Type, JsonConverter>();");
				foreach (var type in jsonTypes.Values.Where(t => t.ConverterName != null))
					writer.WriteLine($"builder.Add(typeof({type.Type}), new {type.ConverterName}());");

				foreach (var (type, _, prototype) in anonymousTypes)
				{
					writer.WriteLine("{");
					writer.WriteLine($@"var type = GetAnonymousType(false ? {prototype} : null);");
					writer.WriteLine($"builder.Add(type, new ValueConverter(type));");
					writer.WriteLine("}");
				}

				writer.WriteLine("return builder.ToImmutable();");
				writer.WriteLine("}");

				foreach (var type in jsonTypes.Values.Where(t => t.ConverterName != null))
				{
					writer.WriteLine();
					writer.WriteLine($"private sealed class {type.ConverterName} : JsonConverter<{type.Type}>");
					writer.WriteLine("{");
					
					writer.WriteLine($"public override {type.Type} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => ReadInternal(ref reader, options);");
					writer.WriteLine($"public override void Write(Utf8JsonWriter writer, {type.Type} value, JsonSerializerOptions options) => WriteInternal(writer, value, options);");
					
					writer.WriteLine($"internal static {type.Type} ReadInternal(ref Utf8JsonReader reader, JsonSerializerOptions options)");
					writer.WriteLine("{");
					if (type.ConstructionMember is ConstructorDeclarationSyntax)
						GenerateConstructorReader(type, writer, jsonTypes, compilation);
					else if (type.ConstructionMember is MethodDeclarationSyntax)
						GenerateParseReader(type, writer, jsonTypes, compilation);
					else if (type.TinyType != null)
						GenerateTinyTypeReader(type, writer, jsonTypes);
					else if (type.ConstructionMember != null || type.TinyType != null)
						throw new InvalidOperationException("Unknown declaration type");

					writer.WriteLine("}");
					writer.WriteLine();
					writer.WriteLine($"internal static void WriteInternal(Utf8JsonWriter writer, {type.Type} value, JsonSerializerOptions options)");
					writer.WriteLine("{");

					if (type.ConstructionMember is ConstructorDeclarationSyntax)
						writer.WriteLine("throw new NotImplementedException();"); //GenerateConstructorWriter(type, writer, jsonTypes);
					else if (type.ConstructionMember is MethodDeclarationSyntax)
						GenerateParseWriter(type, writer, jsonTypes, compilation);
					else if (type.TinyType != null)
						GenerateTinyTypeWriter(type, writer, jsonTypes);
					else if (type.ConstructionMember != null || type.TinyType != null)
						throw new InvalidOperationException("Unknown declaration type");

					writer.WriteLine("}");

					writer.WriteLine("}");
				}
				
				writer.WriteLine($"partial class ValueConverter");
				writer.WriteLine("{");
				writer.WriteLine("private static ImmutableDictionary<Type, Action<Utf8JsonWriter, object, JsonSerializerOptions>> CreateDelegates()");
				writer.WriteLine("{");
				writer.WriteLine("var builder = ImmutableDictionary.CreateBuilder<Type, Action<Utf8JsonWriter, object, JsonSerializerOptions>>();");
				foreach (var (type, methodName, prototype) in anonymousTypes)
					writer.WriteLine($@"builder.Add(GetAnonymousType(false ? {prototype} : null), {methodName});");
				
				writer.WriteLine("return builder.ToImmutable();");
				writer.WriteLine("}");

				foreach (var (type, methodName, prototype) in anonymousTypes)
					GenerateAnonymousTypeMethod(type, methodName, prototype, writer, jsonTypes, compilation);

				writer.WriteLine("}");

				writer.WriteLine("}");
				writer.WriteLine("}");

				return outputFile.Name;
			}
		}

		

		private sealed class JsonAttributeScanningWalker : CSharpSyntaxWalker
		{
			private JsonAttributeScanningWalker(Compilation compilation, SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node) : base(depth)
			{
				Compilation = compilation;
				JsonTypes.Add(TypeName.String, new JsonType(TypeName.String, _ => "reader.GetString()", (_, valueExpr) => $"writer.WriteStringValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.Bool, new JsonType(TypeName.Bool, _ => "reader.GetBoolean()", (_, valueExpr) => $"writer.WriteBooleanValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.Byte, new JsonType(TypeName.Byte, _ => "reader.GetByte()", (_, valueExpr) => $"writer.WriteNumberValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.SByte, new JsonType(TypeName.SByte, _ => "reader.GetSByte()", (_, valueExpr) => $"writer.WriteNumberValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.Int16, new JsonType(TypeName.Int16, _ => "reader.GetInt16()", (_, valueExpr) => $"writer.WriteNumberValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.Int32, new JsonType(TypeName.Int32, _ => "reader.GetInt32()", (_, valueExpr) => $"writer.WriteNumberValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.Int64, new JsonType(TypeName.Int64, _ => "reader.GetInt64()", (_, valueExpr) => $"writer.WriteNumberValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.UInt16, new JsonType(TypeName.UInt16, _ => "reader.GetUInt16()", (_, valueExpr) => $"writer.WriteNumberValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.UInt32, new JsonType(TypeName.UInt32, _ => "reader.GetUInt32()", (_, valueExpr) => $"writer.WriteNumberValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.UInt64, new JsonType(TypeName.UInt64, _ => "reader.GetUInt64()", (_, valueExpr) => $"writer.WriteNumberValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.Single, new JsonType(TypeName.Single, _ => "reader.GetSingle()", (_, valueExpr) => $"writer.WriteNumberValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.Double, new JsonType(TypeName.Double, _ => "reader.GetDouble()", (_, valueExpr) => $"writer.WriteNumberValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.Decimal, new JsonType(TypeName.Decimal, _ => "reader.GetDecimal()", (_, valueExpr) => $"writer.WriteNumberValue({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.DateTime, new JsonType(TypeName.DateTime, _ => "reader.GetDateTime()", (_, valueExpr) => $"JsonSerializer.Serialize<DateTime>(writer, {valueExpr}, options);", null, null, null, null));
				JsonTypes.Add(TypeName.DateTimeOffset, new JsonType(TypeName.DateTimeOffset, _ => "reader.GetDateTimeOffset()", (_, valueExpr) => $"JsonSerializer.Serialize<DateTimeOffset>({valueExpr});", null, null, null, null));
				JsonTypes.Add(TypeName.Guid, new JsonType(TypeName.Guid, _ => "reader.GetGuid()", (_, valueExpr) => $"JsonSerializer.Serialize<Guid>({valueExpr});", null, null, null, null));
			}
			
			private Compilation Compilation { get; }
			private ImmutableDictionary<TypeName, JsonType>.Builder JsonTypes { get; } = ImmutableDictionary.CreateBuilder<TypeName, JsonType>();
			
			public static ImmutableDictionary<TypeName, JsonType> GetJsonTypes(ImmutableArray<SyntaxTree> nodes, Compilation compilation)
			{
				var walker = new JsonAttributeScanningWalker(compilation);

				foreach (var tinyType in TinyTypeScanningWalker.GetTinyTypes(compilation, nodes))
					walker.VisitTinyType(tinyType);

				foreach (var node in nodes)
					walker.Visit(node.GetRoot());

				return walker.JsonTypes.ToImmutable();
			}

			public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
			{
				if (!node.AttributeLists.Any())
				{
					base.VisitConstructorDeclaration(node);
					return;
				}
				
				var hasAttribute = node.AttributeLists
					.SelectMany(al => al.Attributes)
					.Any(attr => attr.Name.ToString() == "JsonConstructor");

				if (!hasAttribute)
				{
					base.VisitConstructorDeclaration(node);
					return;
				}

				var semanticModel = Compilation.GetSemanticModel(node.SyntaxTree);

				var typeName = node
					.Ancestors().OfType<TypeDeclarationSyntax>().First()
					.ToTypeName(semanticModel);

				var converterName = $"{typeName.ShortName}ConstructorConverter";
				JsonTypes.Add(typeName, new JsonType(
					type: typeName,
					directReadExpression: jsonTypes => $"{converterName}.ReadInternal(ref reader, options)",
					directWriteStatement: (jsonTypes, valueExpr) => $"{converterName}.WriteInternal(writer, {valueExpr}, options);",
					converterName: converterName,
					constructionMember: node,
					outputMember: null,
					tinyType: null
				));
			}

			private void VisitTinyType(TinyType tinyType)
			{
				#pragma warning disable CS8603 // Possible null reference return.
				JsonTypes.Add(tinyType.Type, new JsonType(
					type: tinyType.Type,
					directReadExpression: jsonTypes => jsonTypes.TryGetValue(tinyType.InnerType, out var innerTypeJsonType) && innerTypeJsonType.CanDirectRead(jsonTypes) ? $"({tinyType.Type})({innerTypeJsonType.DirectReadExpression!(jsonTypes)})" : null,
					directWriteStatement: (jsonTypes, valueExpr) => jsonTypes.TryGetValue(tinyType.InnerType, out var innerTypeJsonType) && innerTypeJsonType.CanDirectWrite(jsonTypes) ? innerTypeJsonType.DirectWriteStatement!(jsonTypes, $"({tinyType.InnerType})({valueExpr})") : null,
					converterName: $"{tinyType.Type.ShortName}TinyTypeConverter",
					constructionMember: null,
					outputMember: null,
					tinyType: tinyType
				));
				#pragma warning restore CS8603 // Possible null reference return.
			}

			public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				if (!node.AttributeLists.Any())
				{
					base.VisitMethodDeclaration(node);
					return;
				}
				
				var attribute = node.AttributeLists
					.SelectMany(al => al.Attributes)
					.SingleOrDefault(attr => attr.Name.ToString() == "JsonParseMethod");

				if (attribute is null)
				{
					base.VisitMethodDeclaration(node);
					return;
				}

				var semanticModel = Compilation.GetSemanticModel(node.SyntaxTree);

				var type = node.Ancestors().OfType<TypeDeclarationSyntax>().First();
				var typeName = type.ToTypeName(semanticModel);
				var typeSymbol = semanticModel.GetDeclaredSymbol(type);

				if (node.ReturnType.ToTypeName(semanticModel) != typeName)
					throw new InvalidOperationException($"Parse method {typeName}.{node.Identifier} must have a signature returning the containing type.");

				if (node.ParameterList.Parameters.Count != 1)
					throw new InvalidOperationException($"Parse method {typeName}.{node.Identifier} must have a single parameter");

				if (node.Modifiers.Any(m => m.ToString() == "private" || m.ToString() == "internal"))
					throw new InvalidOperationException($"Parse method {typeName}.{node.Identifier} must have be public or internal");

				if (!node.Modifiers.Any(m => m.ToString() == "static"))
					throw new InvalidOperationException($"Parse method {typeName}.{node.Identifier} must have be static");

				var parseType = node.ParameterList.Parameters.Single().Type!.ToTypeName(semanticModel);

				var writeMemberName = attribute.ArgumentList?.Arguments.SingleOrDefault();
				Func<ImmutableDictionary<TypeName, JsonType>, string, string>? writeStatement = null;
				MemberDeclarationSyntax? outputMember = null;
				if (writeMemberName is not null)
				{
					if (writeMemberName.Expression is null)
						throw new InvalidOperationException("JsonParseMethod declarations must have zero arguments or a single unnamed nameof parameter naming the write member");

					if (writeMemberName.Expression is not InvocationExpressionSyntax invocation || invocation.Expression.ToString() != "nameof")
						throw new InvalidOperationException("JsonParseMethod declarations that specify a write member must use nameof to reference the member");

					if (invocation.ArgumentList.Arguments.Count != 1 || invocation.ArgumentList.Arguments.Single().Expression is null)
						throw new InvalidOperationException("Invalid nameof operator");

					var symbolInfo = semanticModel.GetSymbolInfo(invocation.ArgumentList.Arguments.Single().Expression);
					var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().SingleOrDefault(m => m.Parameters.Length == 0 && !m.ReturnsVoid && !m.IsStatic && SymbolEqualityComparer.Default.Equals(m.ContainingType, typeSymbol));
										
					if (symbol is IMethodSymbol method)
					{
						var writeOutputType = method.ReturnType.ToTypeName();
						writeStatement = (jsonTypes, valueExpr) => jsonTypes.TryGetValue(writeOutputType, out var parseTypeJsonType) && parseTypeJsonType.CanDirectWrite(jsonTypes) ? parseTypeJsonType.DirectWriteStatement!(jsonTypes, $"({valueExpr}).{method.Name}()") : null;
						outputMember = method.DeclaringSyntaxReferences.First().GetSyntax() as MemberDeclarationSyntax;
					}
					else if (symbol is IPropertySymbol property)
					{
						var writeOutputType = property.Type.ToTypeName();
						writeStatement = (jsonTypes, valueExpr) => jsonTypes.TryGetValue(writeOutputType, out var parseTypeJsonType) && parseTypeJsonType.CanDirectWrite(jsonTypes) ? parseTypeJsonType.DirectWriteStatement!(jsonTypes, $"({valueExpr}).{property.Name}") : null;
						outputMember = property.DeclaringSyntaxReferences.First().GetSyntax() as MemberDeclarationSyntax;
					}
					else
					{
						throw new InvalidOperationException($"The member {invocation.ArgumentList.Arguments.Single().Expression} is not an instance property or method with no parameters and a return value on the same type as the parse method.");
					}
				}

				#pragma warning disable CS8603 // Possible null reference return.
				JsonTypes.Add(typeName, new JsonType(
					type: typeName,
					directReadExpression: jsonTypes => jsonTypes.TryGetValue(parseType, out var parseTypeJsonType) && parseTypeJsonType.CanDirectRead(jsonTypes) ? $"{typeName}.{node.Identifier}({parseTypeJsonType.DirectReadExpression!(jsonTypes)})" : null,
					directWriteStatement: writeStatement,
					converterName: $"{typeName.ShortName}MethodConverter",
					constructionMember: node,
					outputMember: outputMember,
					tinyType: null
				));
				#pragma warning restore CS8603 // Possible null reference return.
			}
		}

		private sealed class JsonType
		{
			public TypeName Type { get; }
			public bool CanDirectRead(ImmutableDictionary<TypeName, JsonType> jsonTypes) => DirectReadExpression?.Invoke(jsonTypes) != null;
			public Func<ImmutableDictionary<TypeName, JsonType>, string?>? DirectReadExpression { get; }
			public bool CanDirectWrite(ImmutableDictionary<TypeName, JsonType> jsonTypes) => DirectWriteStatement?.Invoke(jsonTypes, "") != null;
			public Func<ImmutableDictionary<TypeName, JsonType>, string, string?>? DirectWriteStatement { get; }
			public string? ConverterName { get; }
			public MemberDeclarationSyntax? ConstructionMember { get; }
			public MemberDeclarationSyntax? OutputMember { get; }
			public TinyType? TinyType { get; }

			public JsonType(TypeName type, Func<ImmutableDictionary<TypeName, JsonType>, string>? directReadExpression, Func<ImmutableDictionary<TypeName, JsonType>, string, string>? directWriteStatement, string? converterName, MemberDeclarationSyntax? constructionMember, MemberDeclarationSyntax? outputMember, TinyType? tinyType)
			{
				Type = type;
				DirectReadExpression = directReadExpression;
				DirectWriteStatement = directWriteStatement;
				ConverterName = converterName;
				ConstructionMember = constructionMember;
				TinyType = tinyType;
				OutputMember = outputMember;
			}
		}

		private static void GenerateConstructorReader(JsonType jsonType, TextWriter writer, ImmutableDictionary<TypeName, JsonType> jsonTypes, Compilation compilation)
		{
			var semanticModel = compilation.GetSemanticModel(jsonType.ConstructionMember!.SyntaxTree);

			var arguments = ((ConstructorDeclarationSyntax)jsonType.ConstructionMember!).ParameterList.Parameters
				.Select(param =>
				{
					var type = param.Type!.ToString();
					var isNullable = param.Type is NullableTypeSyntax;
					var argJsonType = jsonTypes.TryGetValue(param.Type!.ToTypeName(semanticModel), out var jt) ? jt : default(JsonType?);
					var (getter, needsConverter) = JsonGetter(type, isNullable, argJsonType, jsonTypes);

					return new
					{
						Type = type,
						IsNullable = isNullable,
						Name = param.Identifier.ToString(),
						JsonType = argJsonType,
						Getter = getter,
						NeedsConverter = needsConverter
					};
				})
				.ToList();
			
			writer.WriteLine("if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();");
			writer.WriteLine();

			foreach (var argument in arguments)
			{
				if (argument.IsNullable)
					writer.WriteLine($"{argument.Type} {argument.Name} = default;");
				else
					writer.WriteLine($"{argument.Type}? {argument.Name} = default;");
			}

			foreach (var type in arguments.Where(a => a.NeedsConverter).Select(a => a.Type).Distinct())
				writer.WriteLine($"var {ConverterLocalName(type)} = GetConverter<{type}>(options);");

			writer.WriteLine("while(reader.Read())");
			writer.WriteLine("{");
			writer.WriteLine("if (reader.TokenType == JsonTokenType.EndObject) break;");
			writer.WriteLine();
			writer.WriteLine("var propertyName = reader.GetString();");
			writer.WriteLine("reader.Read();");
			writer.WriteLine("switch(propertyName)");
			writer.WriteLine("{");

			foreach (var argument in arguments)
			{
				writer.WriteLine($@"case ""{argument.Name}"":");
				writer.WriteLine($"{argument.Name} = {argument.Getter};");
				writer.WriteLine("break;");
			}

			writer.WriteLine("}"); // switch

			writer.WriteLine("}"); // while

			foreach (var argument in arguments.Where(a => !a.IsNullable))
				writer.WriteLine($"if ({argument.Name} is null) throw new JsonException();");

			writer.WriteLine($"return new {jsonType.Type}({string.Join(", ", arguments.Select(a => $"({a.Type}){a.Name}"))});");

			static (string? getter, bool needsConverter) JsonGetter(string type, bool isNullable, JsonType? jsonType, ImmutableDictionary<TypeName, JsonType> jsonTypes)
			{
				if (jsonType?.CanDirectRead(jsonTypes) == true)
					return (jsonType.DirectReadExpression!(jsonTypes), false);
				else if (isNullable)
					return ($"JsonSerializer.Deserialize<{type}>(ref reader, options)", false);
				else
					return ($"{ConverterLocalName(type)} != null ? {ConverterLocalName(type)}.Read(ref reader, typeof({type}), options) : JsonSerializer.Deserialize<{type}>(ref reader, options)", true);
			}

			static string ConverterLocalName(string type)
			{
				return type
					       .Replace("?", "_nullable")
					       .Replace("<", "_")
					       .Replace(">", "_")
					       .ToLowerInvariant()
				       + "Converter";
			}
		}
		private static void GenerateParseReader(JsonType type, StreamWriter writer, ImmutableDictionary<TypeName, JsonType> jsonTypes, Compilation compilation)
		{
			if (type.CanDirectRead(jsonTypes))
			{
				writer.WriteLine($"return {type.DirectReadExpression!(jsonTypes)};");
				return;
			}

			var semanticModel = compilation.GetSemanticModel(type.ConstructionMember!.SyntaxTree);
			var method = (MethodDeclarationSyntax) type.ConstructionMember!;
			var parseType = method.ParameterList.Parameters.Single().Type!.ToTypeName(semanticModel);
			
			if (jsonTypes.TryGetValue(parseType, out var parseTypeJsonType) && parseTypeJsonType.CanDirectRead(jsonTypes))
			{
				writer.WriteLine($"return {type.Type}.{method.Identifier}({parseTypeJsonType.DirectReadExpression!(jsonTypes)});");
				return;
			}

			writer.WriteLine($"return {type.Type}.{method.Identifier}(GetValue<{parseType}>(ref reader, options));");
		}

		private static void GenerateParseWriter(JsonType type, StreamWriter writer, ImmutableDictionary<TypeName, JsonType> jsonTypes, Compilation compilation)
		{
			if (type.CanDirectWrite(jsonTypes))
			{
				writer.WriteLine(type.DirectWriteStatement!(jsonTypes, "value"));
				return;
			}
						
			if (type.OutputMember is MethodDeclarationSyntax method)
			{
				var semanticModel = compilation.GetSemanticModel(type.OutputMember!.SyntaxTree);
				var outputType = method.ReturnType.ToTypeName(semanticModel);
			
				if (jsonTypes.TryGetValue(outputType, out var parseTypeJsonType) && parseTypeJsonType.CanDirectWrite(jsonTypes))
				{
					writer.WriteLine(parseTypeJsonType.DirectWriteStatement!(jsonTypes, $"value.{method.Identifier}()"));
					return;
				}

				writer.WriteLine($"JsonSerializer.Serialize(writer, value.{method.Identifier}(), options);");
			}
			else if (type.OutputMember is PropertyDeclarationSyntax property)
			{
				var semanticModel = compilation.GetSemanticModel(type.OutputMember!.SyntaxTree);
				var outputType = property.Type.ToTypeName(semanticModel);
			
				if (jsonTypes.TryGetValue(outputType, out var parseTypeJsonType) && parseTypeJsonType.CanDirectWrite(jsonTypes))
				{
					writer.WriteLine(parseTypeJsonType.DirectWriteStatement!(jsonTypes, $"value.{property.Identifier}"));
					return;
				}

				writer.WriteLine($"JsonSerializer.Serialize(writer, value.{property.Identifier}, options);");
			}
			else
			{
				writer.WriteLine("throw new NotImplementedException();");
			}
		}

		private static void GenerateTinyTypeReader(JsonType jsonType, TextWriter writer, ImmutableDictionary<TypeName, JsonType> jsonTypes)
		{
			if (jsonType.CanDirectRead(jsonTypes))
			{
				writer.WriteLine($"return {jsonType.DirectReadExpression!(jsonTypes)};");
				return;
			}

			var tinyType = jsonType.TinyType!.Value;
			writer.WriteLine($"return ({tinyType.Type})GetValue<{tinyType.InnerType}>(ref reader, options);");
		}

		private static void GenerateTinyTypeWriter(JsonType jsonType, TextWriter writer, ImmutableDictionary<TypeName, JsonType> jsonTypes)
		{
			if (!jsonType.CanDirectWrite(jsonTypes))
				throw new InvalidOperationException("All TinyTypes should be able to direct write");
			
			writer.WriteLine(jsonType.DirectWriteStatement!(jsonTypes, "value"));
		}

		private static void GenerateAnonymousTypeMethod(ITypeSymbol type, string methodName, string prototype, TextWriter writer, ImmutableDictionary<TypeName, JsonType> jsonTypes, Compilation compilation)
		{
			var valueType = compilation.GetTypeByMetadataName("DNSMadeEasy.Value`1")!;

			writer.WriteLine($"private static void {methodName}(Utf8JsonWriter writer, object valueObj, JsonSerializerOptions options)");
			writer.WriteLine("{");

			writer.WriteLine($"var value = Cast(false ? {prototype} : null, valueObj)!;");

			writer.WriteLine("writer.WriteStartObject();");

			foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
			{
				if (IsValueOfTType(property.Type, valueType))
				{
					var innerType = (property.Type as INamedTypeSymbol)!.TypeArguments.Single() as INamedTypeSymbol;
					writer.WriteLine($"if (value.{property.Name}.HasValue)");
					writer.WriteLine("{");
					writer.WriteLine($@"writer.WritePropertyName(""{property.Name}"");");

					if (jsonTypes.TryGetValue(innerType!.ToTypeName(), out var jsonType))
					{
						if (jsonType.CanDirectWrite(jsonTypes))
							writer.WriteLine(jsonType.DirectWriteStatement!(jsonTypes, $"({innerType!}) value.{property.Name}"));
						else if (jsonType.ConverterName != null)
							writer.WriteLine($"{jsonType.ConverterName}.WriteInternal(writer, ({innerType!}) value.{property.Name}, options);");
						else
							writer.WriteLine($"JsonSerializer.Serialize(writer, value.{property.Name}.Value, options);");
					}
					else
					{
						writer.WriteLine($"JsonSerializer.Serialize(writer, ({innerType!}) value.{property.Name}, options);");
					}
					writer.WriteLine("}");
				}
				else
				{
					writer.WriteLine($@"writer.WritePropertyName(""{property.Name}"");");
					if (jsonTypes.TryGetValue(property.Type.ToTypeName(), out var jsonType))
					{
						if (jsonType.CanDirectWrite(jsonTypes))
							writer.WriteLine(jsonType.DirectWriteStatement!(jsonTypes, $"value.{property.Name}"));
						else if (jsonType.ConverterName != null)
							writer.WriteLine($"{jsonType.ConverterName}.WriteInternal(writer, value.{property.Name}, options);");
						else
							writer.WriteLine($"JsonSerializer.Serialize(writer, value.{property.Name}, options);");
					}
					else
					{
						writer.WriteLine($"JsonSerializer.Serialize(writer, value.{property.Name}, options);");
					}
				}

				writer.WriteLine();
			}

			writer.WriteLine("writer.WriteEndObject();");

			writer.WriteLine("}");
		}

		private static bool IsValueOfTType(ITypeSymbol type, INamedTypeSymbol valueType)
		{
			if (!type.IsValueType)
				return false;
				
			if (!(type is INamedTypeSymbol namedTypeSymbol))
				throw new InvalidOperationException($"Can't process the type `{type.MetadataName}`");
					
			if (!namedTypeSymbol.IsGenericType)
				return false;

			return namedTypeSymbol.OriginalDefinition.Equals(valueType, SymbolEqualityComparer.Default);
		}

		private sealed class JsonAnonymousTypeWithValueScanningWalker : CSharpSyntaxWalker
		{
			private HashSet<INamedTypeSymbol> AnonymousTypes { get; } = new HashSet<INamedTypeSymbol>();

			private Compilation Compilation { get; }
			private INamedTypeSymbol ValueType { get; }

			private JsonAnonymousTypeWithValueScanningWalker(Compilation compilation)
			{
				Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
				ValueType = Compilation.GetTypeByMetadataName("DNSMadeEasy.Value`1")!;
			}

			public static ImmutableArray<AnonymousType> GetAnonymousTypes(ImmutableArray<SyntaxTree> nodes, Compilation compilation)
			{
				var walker = new JsonAnonymousTypeWithValueScanningWalker(compilation);
				
				foreach (var node in nodes)
					walker.Visit(node.GetRoot());

				var array = ImmutableArray.CreateBuilder<AnonymousType>(walker.AnonymousTypes.Count);
				var methodName = "A";
				foreach (var type in walker.AnonymousTypes)
				{
					array.Add(new AnonymousType(type, methodName));
					methodName = NextMethodName(methodName);
				}
				return array.MoveToImmutable();
			}

			private static string NextMethodName(string str)
			{
				if (str.Length == 1 && str[0] < 'Z')
					return ((char)(str[str.Length - 1] + 1)).ToString();
				else if (str.All(c => c == 'Z'))
					return "B" + new string('A', str.Length);
				else if (str[str.Length - 1] < 'Z')
					return str.Substring(0, str.Length - 1) + (char)(str[str.Length - 1] + 1);
				else
					return NextMethodName(str.Substring(0, str.Length - 1)) + 'A';
			}

			public override void VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
			{
				var semanticModel = Compilation.GetSemanticModel(node.SyntaxTree, ignoreAccessibility: true);
				var typeInfo = semanticModel.GetTypeInfo(node).Type!;

				var properties = typeInfo.GetMembers().OfType<IPropertySymbol>();
				if (!properties.Any(p => IsValueOfTType(p.Type, ValueType)))
					return;

				AnonymousTypes.Add((INamedTypeSymbol)typeInfo);
			}
		}

		private readonly struct AnonymousType
		{
			public INamedTypeSymbol Type { get; }
			public string MethodName { get; }
			public string Prototype { get; }

			public AnonymousType(INamedTypeSymbol type, string methodName)
			{
				Type = type ?? throw new ArgumentNullException(nameof(type));
				MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
				Prototype = GeneratePrototype(Type);
			}

			private static string GeneratePrototype(INamedTypeSymbol type)
			{
				var propertyParts = type.GetMembers().OfType<IPropertySymbol>().Select(p => $"{p.Name} = default({p.Type.ToTypeName()})");
				return $"new {{ {string.Join(", ", propertyParts) } }}";
			}

			public void Deconstruct(out INamedTypeSymbol type, out string methodName, out string prototype)
			{
				type = Type;
				methodName = MethodName;
				prototype = Prototype;
			}
		}
	}
}
