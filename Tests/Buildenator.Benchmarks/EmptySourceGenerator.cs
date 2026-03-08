using Microsoft.CodeAnalysis;

namespace Buildenator.Benchmarks;

[Generator]
internal class EmptySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {

    }
}