using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace DNSMadeEasy.JsonCodegen
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			var fileList = File.ReadAllLines(args[0]);
			var referenceList = File.ReadAllLines(args[1]);
			var tldJson = args[2];
			var outputPath = args[3];

			var syntaxTrees = fileList.Select(ParseFile).ToImmutableArray();

			var compilation = CSharpCompilation.Create("codegen",
				syntaxTrees,
				referenceList.Select(r => MetadataReference.CreateFromFile(r)),
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, concurrentBuild: true)
			);

			var outputFileList = new[]
			{
				Converters.Generate(outputPath, syntaxTrees, compilation),
				TinyTypes.Generate(outputPath, syntaxTrees, compilation),
				await TLDs.Generate(outputPath, tldJson)
			};

			foreach (var file in outputFileList)
				FormatFile(file);

			File.WriteAllLines(args[0] + ".post", outputFileList);
		}

		private static SyntaxTree ParseFile(string filePath)
		{
			using var file = File.OpenRead(filePath);
			return CSharpSyntaxTree.ParseText(SourceText.From(file), new CSharpParseOptions(documentationMode: DocumentationMode.None));
		}

		private static void FormatFile(string filePath)
		{
			using var file = File.Open(filePath, FileMode.Open);
			var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(file), new CSharpParseOptions(documentationMode: DocumentationMode.None));

			var workspace = new AdhocWorkspace();
			var formattedNode = Formatter.Format(syntaxTree.GetRoot(), workspace);

			file.Seek(0, SeekOrigin.Begin);
			using var writer = new StreamWriter(file);
			formattedNode.WriteTo(writer);
		}
	}
}
