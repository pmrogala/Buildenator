using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Buildenator.Extensions;

public static class GeneratorExecutionContextExtensions
{
	private static volatile int _duplicationNumber = 1;
	public static void AddCsSourceFile(this in SourceProductionContext context, string fileNameWithoutExtension, SourceText sourceText)
	{
		context.AddSource($"{fileNameWithoutExtension}_{Interlocked.Increment(ref _duplicationNumber)}.cs", sourceText);
	}

}