using Microsoft.CodeAnalysis;

namespace Buildenator.Benchmarks
{
    internal class EmptySourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            return;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            return;
        }
    }
}
