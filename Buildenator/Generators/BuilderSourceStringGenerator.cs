using Buildenator.Abstraction;
using Buildenator.CodeAnalysis;
using Buildenator.Configuration.Contract;
using System.Collections.Generic;
using Buildenator.Configuration;
using static Buildenator.Generators.NamespacesGenerator;
using static Buildenator.Generators.ConstructorsGenerator;
using System.Reflection;
using Buildenator.Diagnostics;

namespace Buildenator.Generators;

internal sealed class BuilderSourceStringGenerator
{
    private readonly IBuilderProperties _builder;
    private readonly IEntityToBuild _entity;
    private readonly IFixtureProperties? _fixtureConfiguration;
    private readonly IMockingProperties? _mockingConfiguration;
    private readonly PropertiesStringGenerator _propertiesStringGenerator;
    private readonly List<BuildenatorDiagnostic> _diagnostics = [];

    public BuilderSourceStringGenerator(
        IBuilderProperties builder,
        IEntityToBuild entity,
        IFixtureProperties? fixtureConfiguration,
        IMockingProperties? mockingConfiguration)
    {
        _builder = builder;
        _entity = entity;
        var privateConstructorDiagnostic = GetBuiltTypeWithoutPublicConstructorDiagnosticOrDefault();
        if (privateConstructorDiagnostic is not null)
        {
            _diagnostics.Add(privateConstructorDiagnostic);
        }
        _diagnostics.AddRange(_builder.Diagnostics);
        _diagnostics.AddRange(_entity.Diagnostics);

        _fixtureConfiguration = fixtureConfiguration;
        _mockingConfiguration = mockingConfiguration;
        _propertiesStringGenerator = new PropertiesStringGenerator(_builder, _entity);
    }

    private BuildenatorDiagnostic? GetBuiltTypeWithoutPublicConstructorDiagnosticOrDefault()
    {
        return _entity.ConstructorToBuild is null
                ? _builder.IsBuildMethodOverriden
                    ? null
                    : new BuildenatorDiagnostic(
                        BuildenatorDiagnosticDescriptors.NoPublicConstructorsDiagnostic,
                        _builder.OriginalLocation,
                        _entity.FullName)
                : null;
    }

    public string FileName => _builder.Name;

    public IEnumerable<BuildenatorDiagnostic> Diagnostics => _diagnostics;

    public string CreateBuilderCode()
        => $@"{AutoGenerationComment}
{GenerateNamespaces(_fixtureConfiguration, _mockingConfiguration, _entity)}

namespace {_builder.ContainingNamespace}
{{
{GenerateGlobalNullable()}{GenerateBuilderDefinition()}
    {{
{(_fixtureConfiguration is null ? string.Empty : $"        private readonly {_fixtureConfiguration.Name} {DefaultConstants.FixtureLiteral} = new {_fixtureConfiguration.Name}({_fixtureConfiguration.ConstructorParameters});")}
{(_builder.IsDefaultConstructorOverriden ? string.Empty : GenerateConstructor(_builder.Name, _entity, _fixtureConfiguration, _builder.InitializeCollectionsWithEmpty, _builder))}
{_propertiesStringGenerator.GeneratePropertiesCode()}
{(_builder.IsBuildMethodOverriden ? string.Empty : _entity.GenerateBuildsCode(_builder.ShouldGenerateMethodsForUnreachableProperties))}
{(_builder.IsBuildManyMethodOverriden ? string.Empty : GenerateBuildManyCode())}
{(_builder.GenerateStaticPropertyForBuilderCreation ? $"        public static {_builder.FullName} {_entity.Name} => new {_builder.FullName}();" : "")}
{(_builder.GenerateDefaultBuildMethod ? _entity.GenerateDefaultBuildsCode() : string.Empty)}
{(_builder.ImplicitCast ? GenerateImplicitCastCode() : string.Empty)}
{GeneratePreBuildMethod()}
{GeneratePostBuildMethod()}
    }}
}}";

    private object GeneratePreBuildMethod()
        => _builder.IsPreBuildMethodOverriden
            ? string.Empty
            : @$"{CommentsGenerator.GenerateSummaryOverrideComment()}
        public void {DefaultConstants.PreBuildMethodName}() {{ }}";

    private object GeneratePostBuildMethod()
        => _builder.IsPostBuildMethodOverriden
            ? string.Empty
            : @$"{CommentsGenerator.GenerateSummaryOverrideComment()}
        public void {DefaultConstants.PostBuildMethodName}({_entity.FullName} buildResult) {{ }}";

    private string GenerateGlobalNullable()
        => _builder.NullableStrategy switch
        {
            NullableStrategy.Enabled => "#nullable enable\n",
            NullableStrategy.Disabled => "#nullable disable\n",
            _ => string.Empty
        };

    private string GenerateBuilderDefinition()
        => @$"    public partial class {_entity.FullNameWithConstraints.Replace(_entity.Name, _builder.Name)}";

    private string GenerateBuildManyCode()
    {
        return $@"        public System.Collections.Generic.IEnumerable<{_entity.FullName}> BuildMany(int count = 3)
        {{
            return Enumerable.Range(0, count).Select(_ => Build());
        }}
";

    }

    private string GenerateImplicitCastCode()
    {
        return $@"        public static implicit operator {_entity.FullName}({_builder.FullName} builder) => builder.{DefaultConstants.BuildMethodName}();";
    }

    private static readonly string AutoGenerationComment = @$"
// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a source generator named Buildenator (https://github.com/pmrogala/Buildenator)
//     Version {Assembly.GetAssembly(MethodBase.GetCurrentMethod().DeclaringType).GetName().Version}
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------";
}