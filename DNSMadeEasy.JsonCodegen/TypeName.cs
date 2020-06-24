using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace DNSMadeEasy.JsonCodegen
{
	public readonly struct TypeName : IEquatable<TypeName>
	{
		public string FullName { get; }
		public string ShortName { get; }

		private static SymbolDisplayFormat LongDisplayFormat { get; } = new SymbolDisplayFormat(
			SymbolDisplayGlobalNamespaceStyle.Omitted,
			SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
			SymbolDisplayGenericsOptions.IncludeTypeParameters
		);

		private static SymbolDisplayFormat ShortDisplayFormat { get; } = new SymbolDisplayFormat(
			SymbolDisplayGlobalNamespaceStyle.Omitted,
			SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
			SymbolDisplayGenericsOptions.IncludeTypeParameters
		);

		public static TypeName From(ITypeSymbol type) => type.ToDisplayString(LongDisplayFormat) switch
		{
			"System.String" => String,
			"System.Boolean" => Bool,
			"System.Byte" => Byte,
			"System.SByte" => SByte,
			"System.Int16" => Int16,
			"System.Int32" => Int32,
			"System.Int64" => Int64,
			"System.UInt16" => UInt16,
			"System.UInt32" => UInt32,
			"System.UInt64" => UInt64,
			"System.Single" => Single,
			"System.Double" => Double,
			"System.Decimal" => Decimal,
			"System.DateTime" => DateTime,
			"System.DateTimeOffset" => DateTimeOffset,
			"System.Guid" => Guid,
			_ => new TypeName(type)
		};

		private TypeName(ITypeSymbol type)
		{
			FullName = type.ToDisplayString(LongDisplayFormat);
			ShortName = type.ToDisplayString(ShortDisplayFormat);
		}

		private TypeName(string alias)
		{
			FullName = alias;
			ShortName = alias;
		}

		public override bool Equals(object? obj) => obj is TypeName name && Equals(name);
		public bool Equals(TypeName other) => FullName == other.FullName;
		public static bool operator ==(TypeName a, TypeName b) => a.Equals(b);
		public static bool operator !=(TypeName a, TypeName b) => !a.Equals(b);

		public override string ToString() => FullName;
		public override int GetHashCode() => FullName.GetHashCode();

		public static TypeName String => new TypeName("string");
		public static TypeName Bool => new TypeName("bool");
		public static TypeName Byte => new TypeName("byte");
		public static TypeName SByte => new TypeName("sbyte");
		public static TypeName Int16 => new TypeName("short");
		public static TypeName Int32 => new TypeName("int");
		public static TypeName Int64 => new TypeName("long");
		public static TypeName UInt16 => new TypeName("ushort");
		public static TypeName UInt32 => new TypeName("uint");
		public static TypeName UInt64 => new TypeName("ulong");
		public static TypeName Single => new TypeName("float");
		public static TypeName Double => new TypeName("double");
		public static TypeName Decimal => new TypeName("decimal");
		public static TypeName DateTime => new TypeName("System.DateTime");
		public static TypeName DateTimeOffset => new TypeName("System.DateTimeOffset");
		public static TypeName Guid => new TypeName("System.Guid");

		public bool IsNumericType => this == Byte || this == SByte || this == Int16 || this == Int32 || this == Int64 || this == UInt16 || this == UInt32 || this == UInt64 || this == Single || this == Double || this == Double;
	}

	internal static class TypeNameExtensions
	{
		public static TypeName ToTypeName(this SyntaxNode node, SemanticModel semanticModel)
		{
			var symbol = semanticModel.GetTypeInfo(node).Type
				?? semanticModel.GetDeclaredSymbol(node) as ITypeSymbol;

			if (symbol is null)
				throw new InvalidOperationException();

			return symbol.ToTypeName();
		}
		public static TypeName ToTypeName(this ITypeSymbol typeSymbol) => TypeName.From(typeSymbol);
	}
}
