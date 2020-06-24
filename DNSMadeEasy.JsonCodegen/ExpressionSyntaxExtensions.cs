using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DNSMadeEasy.JsonCodegen
{
	internal static class ExpressionSyntaxExtensions
	{
		public static bool TryGetStringLiteral(this ExpressionSyntax expression, out string stringLiteral)
		{
			if (expression is LiteralExpressionSyntax literalExpression && literalExpression.Token.IsKind(SyntaxKind.StringLiteralToken))
			{
				stringLiteral = literalExpression.Token.ValueText;
				return true;
			}
			else
			{
				stringLiteral = null;
				return false;
			}
		}
	}
}
