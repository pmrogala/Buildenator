using Microsoft.CodeAnalysis;

namespace Buildenator.Benchmarks;

// Benchmark-only baseline, instantiated directly via CSharpGeneratorDriver — never loaded as a real analyzer.
#pragma warning disable RS1041 // compiler extension target framework
[Generator]
internal class EmptySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {

    }
}
#pragma warning restore RS1041