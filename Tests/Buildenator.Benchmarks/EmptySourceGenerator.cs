using Microsoft.CodeAnalysis;

namespace Buildenator.Benchmarks
{
    internal class EmptySourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
