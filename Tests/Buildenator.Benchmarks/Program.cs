using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Buildenator.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<GenerationTests>();
        }
    }

    [SimpleJob]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class GenerationTests
    {
        // Create the driver that will control the generation, passing in our generator
        private static readonly GeneratorDriver _driver = CSharpGeneratorDriver.Create(new BuildersGenerator());
        private static readonly GeneratorDriver _emptyDriver = CSharpGeneratorDriver.Create(new EmptySourceGenerator());

        [Benchmark()]
        public object DriverAndCompliationOverheadForSimpleCaseTest()
        {
            return _emptyDriver.RunGenerators(CreateCompilation(SimpleSource));
        }

        [Benchmark()]
        public object SimpleGenerationTest()
        {
            return _driver.RunGenerators(CreateCompilation(SimpleSource));
        }

        [Benchmark()]
        public object ComplicatedGenerationTest()
        {
            return _driver.RunGenerators(CreateCompilation(ComplicatedSource, ComplicatedSource2, ComplicatedSource3));
        }

        private static Compilation CreateCompilation(params SyntaxTree[] syntaxTrees)
        {
            return CSharpCompilation.Create("c" + Guid.NewGuid().ToString("N"),
                            syntaxTrees,
                            _references,
                            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        private readonly static SyntaxTree SimpleSource = CSharpSyntaxTree.ParseText(GenerateEntityAndBuilder(2, 5));
        private readonly static SyntaxTree ComplicatedSource = CSharpSyntaxTree.ParseText(GenerateEntityAndBuilder(30, 10));
        private readonly static SyntaxTree ComplicatedSource2 = CSharpSyntaxTree.ParseText(GenerateEntityAndBuilder(30, 10));
        private readonly static SyntaxTree ComplicatedSource3 = CSharpSyntaxTree.ParseText(GenerateEntityAndBuilder(30, 10));

        private readonly static PortableExecutableReference[] _references = new[]
        {
            MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(MakeBuilderAttribute).GetTypeInfo().Assembly.Location)
        };

        private static string GenerateEntityAndBuilder(int entityCount = 3, int propertiesCount = 10)
        {
            return @"using Buildenator.Abstraction;

namespace Buildenator.IntegrationTests.Source.Builders
{
" + string.Concat(GenerateNameList(entityCount)
.Select(x => $@"
    [MakeBuilder(typeof({x}))]
    public partial class {x}Builder
    {{
    }}

    public class {x}
    {{
        " + string.Concat(GenerateNameList(propertiesCount).Select(x => $@"
        public string {x} {{ get; set; }}
")) + @"
    }}
}}"));
        }

        private static IEnumerable<string> GenerateNameList(int entityCount)
        {
            return Enumerable.Range(0, entityCount).Select(x => "A" + Guid.NewGuid().ToString("N"));
        }
    }
}
