using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DNSMadeEasy.JsonCodegen
{
	internal static class TinyTypes
	{
		public static string Generate(string outputPath, ImmutableArray<SyntaxTree> syntaxTrees, Compilation compilation)
		{
			var tinyTypes = TinyTypeScanningWalker.GetTinyTypes(compilation, syntaxTrees);

			using (var outputFile = new FileStream(Path.Combine(outputPath, "TinyTypes.cs"), FileMode.Create, FileAccess.Write))
			using (var writer = new StreamWriter(outputFile, leaveOpen: true))
			{
				writer.WriteLine("using System;");
				writer.WriteLine("using System.Diagnostics;");
				writer.WriteLine("using System.Collections.Generic;");
				writer.WriteLine("using System.Globalization;");

				writer.WriteLine();
				writer.WriteLine("#pragma warning disable CA2225 // Provide method alternative to op_Explicit");
				writer.WriteLine();

				foreach (var ns in tinyTypes.GroupBy(tt => tt.Namespace))
				{
					writer.WriteLine();
					writer.WriteLine($"namespace {ns.Key}");
					writer.WriteLine("{");

					foreach (var tinyType in ns)
					{
						writer.WriteLine();
						writer.WriteLine(@"[DebuggerDisplay(""{value,nq}"")]");
						writer.WriteLine($"readonly partial struct {tinyType.Type.ShortName} : IEquatable<{tinyType.Type.ShortName}>");
						writer.WriteLine("{");

						writer.WriteLine($"private readonly {tinyType.InnerType} value;");
						writer.WriteLine();

						writer.WriteLine($"public {tinyType.Type.ShortName}({tinyType.InnerType} value)");
						writer.WriteLine("{");
						writer.WriteLine("this.value = value;");
						writer.WriteLine("}");
						writer.WriteLine();
						writer.WriteLine($"public static explicit operator {tinyType.InnerType}({tinyType.Type.ShortName} domainId) => domainId.value;");
						writer.WriteLine($"public static explicit operator {tinyType.Type.ShortName}({tinyType.InnerType} value) => new {tinyType.Type.ShortName}(value);");
						writer.WriteLine();
						if (tinyType.InnerType.IsNumericType)
							writer.WriteLine($"public override string ToString() => value.ToString(CultureInfo.InvariantCulture);");
						else
							writer.WriteLine($"public override string ToString() => value.ToString();");
					    writer.WriteLine();
						writer.WriteLine($"public bool Equals({tinyType.Type.ShortName} other) => value.Equals(other.value);");
						writer.WriteLine($"public override bool Equals(object? obj) => obj is {tinyType.Type.ShortName} other && Equals(other);");
				        writer.WriteLine();
						if (tinyType.InnerType.IsNumericType)
							writer.WriteLine($"public override int GetHashCode() => value;");
						else
							writer.WriteLine($"public override int GetHashCode() => value.GetHashCode();");
				        writer.WriteLine();
						writer.WriteLine($"public static bool operator ==({tinyType.Type.ShortName} left, {tinyType.Type.ShortName} right) => left.Equals(right);");
						writer.WriteLine($"public static bool operator !=({tinyType.Type.ShortName} left, {tinyType.Type.ShortName} right) => !left.Equals(right);");
				        writer.WriteLine();
						writer.WriteLine($"public static IEqualityComparer<{tinyType.Type.ShortName}> EqualityComparer {{ get; }} = new ValueEqualityComparer();");
						writer.WriteLine($"private sealed class ValueEqualityComparer : IEqualityComparer<{tinyType.Type.ShortName}>");
						writer.WriteLine("{");
						writer.WriteLine($"public bool Equals({tinyType.Type.ShortName} x, {tinyType.Type.ShortName} y) => x.Equals(y);");
						writer.WriteLine($"public int GetHashCode({tinyType.Type.ShortName} obj) => obj.GetHashCode();");
						writer.WriteLine("}");

						writer.WriteLine("}");
					}

					writer.WriteLine("}");
				}

				return outputFile.Name;
			}
		}
	}

	public readonly struct TinyType
	{
		public TypeName Type { get; }
		public TypeName InnerType { get; }
		public string Namespace { get; }

		public TinyType(TypeName type, TypeName innerType, string ns)
		{
			Type = type;
			InnerType = innerType;
			Namespace = ns;
		}
	}

	public sealed class TinyTypeScanningWalker : CSharpSyntaxWalker
	{
		private TinyTypeScanningWalker(Compilation compilation, SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node) : base(depth)
		{
			Compilation = compilation;
		}

		private Compilation Compilation { get; }
		private ImmutableArray<TinyType>.Builder TinyTypes { get; } = ImmutableArray.CreateBuilder<TinyType>();

		public static ImmutableArray<TinyType> GetTinyTypes(Compilation compilation, ImmutableArray<SyntaxTree> nodes)
		{
			var walker = new TinyTypeScanningWalker(compilation);

			foreach (var node in nodes)
				walker.Visit(node.GetRoot());

			return walker.TinyTypes.ToImmutable();
		}

		public override void VisitStructDeclaration(StructDeclarationSyntax node)
		{
			if (!node.AttributeLists.Any())
				return;

			var hasAttribute = node.AttributeLists
				.SelectMany(al => al.Attributes)
				.Any(attr => attr.Name.ToString() == "TinyType");

			if (!hasAttribute)
				return;

			var semanticModel = Compilation.GetSemanticModel(node.SyntaxTree, ignoreAccessibility: true);

			var type = node.ToTypeName(semanticModel);
			var innerType = (node.AttributeLists
					.SelectMany(al => al.Attributes)
					.Single(attr => attr.Name.ToString() == "TinyType")
					.ArgumentList!.Arguments.Single()
					.Expression as TypeOfExpressionSyntax)
				!.Type.ToTypeName(semanticModel);

			var ns = node.Ancestors().OfType<NamespaceDeclarationSyntax>().First().Name.ToString();

			TinyTypes.Add(new TinyType(type, innerType, ns));
		}
	}
}
