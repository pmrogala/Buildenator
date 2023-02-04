using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Buildenator.Extensions;

public static class GeneratorExecutionContextExtensions
{
	private static volatile int _duplicationNumber = 1;
	public static void AddCsSourceFile(this in GeneratorExecutionContext context, string fileNameWithoutExtension, SourceText sourceText)
	{
		try
		{
			context.AddSource($"{fileNameWithoutExtension}.cs", sourceText);
		}
		catch (System.ArgumentException e) when (e.ParamName == "hintName")
		{
			context.AddSource($"{fileNameWithoutExtension}{Interlocked.Increment(ref _duplicationNumber)}.cs", sourceText);
		}
	}

}