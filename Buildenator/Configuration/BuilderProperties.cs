using Buildenator.Abstraction;
using Buildenator.Configuration.Contract;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Buildenator.Extensions;
using System.Linq;
using Buildenator.Diagnostics;

namespace Buildenator.Configuration;

internal readonly struct BuilderProperties : IBuilderProperties
{
    private readonly Dictionary<string, IMethodSymbol> _buildingMethods;
    private readonly Dictionary<string, IFieldSymbol> _fields;
    private readonly List<BuildenatorDiagnostic> _diagnostics = [];

    public static BuilderProperties Create(
        INamespaceOrTypeSymbol builderSymbol,
        MakeBuilderAttributeInternal builderAttribute,
        ImmutableArray<TypedConstant>? globalAttributes,
        bool nullableAnnotaionEnabled)
    {
        string? defaultNameWith = null;
        bool? defaultStaticBuilder = null;
        NullableStrategy? nullableStrategy = null;
        bool? generateMethodsForUnreachableProperties = null;
        bool? implicitCast = null;

        if (globalAttributes.HasValue)
        {
            defaultNameWith = globalAttributes.Value.GetOrThrow<string>(0, nameof(MakeBuilderAttributeInternal.BuildingMethodsPrefix));
            defaultStaticBuilder = globalAttributes.Value.GetOrThrow<bool>(1, nameof(MakeBuilderAttributeInternal.DefaultStaticCreator));
            nullableStrategy = globalAttributes.Value.GetOrThrow<NullableStrategy>(2, nameof(MakeBuilderAttributeInternal.NullableStrategy));
            generateMethodsForUnreachableProperties = globalAttributes.Value.GetOrThrow<bool>(3, nameof(MakeBuilderAttributeInternal.GenerateMethodsForUnreachableProperties));
            implicitCast = globalAttributes.Value.GetOrThrow<bool>(4, nameof(MakeBuilderAttributeInternal.ImplicitCast));
        }

        nullableStrategy = builderAttribute.NullableStrategy is null ? nullableStrategy: builderAttribute.NullableStrategy;

        if ((nullableStrategy is null || nullableStrategy == NullableStrategy.Default) && nullableAnnotaionEnabled)
        {
            nullableStrategy = NullableStrategy.Enabled;
        }


        return new BuilderProperties(builderSymbol,
            new MakeBuilderAttributeInternal(
                builderAttribute.TypeForBuilder,
                builderAttribute.BuildingMethodsPrefix ?? defaultNameWith,
                builderAttribute.DefaultStaticCreator ?? defaultStaticBuilder,
                nullableStrategy,
                builderAttribute.GenerateMethodsForUnreachableProperties ??
                generateMethodsForUnreachableProperties,
                builderAttribute.ImplicitCast ?? implicitCast,
                builderAttribute.StaticFactoryMethodName));
    }

    private BuilderProperties(INamespaceOrTypeSymbol builderSymbol, MakeBuilderAttributeInternal attributeData)
    {
        ContainingNamespace = builderSymbol.ContainingNamespace.ToDisplayString();
        Name = builderSymbol.Name;
        FullName = builderSymbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
        BuildingMethodsPrefix = attributeData.BuildingMethodsPrefix ?? DefaultConstants.BuildingMethodsPrefix;
        NullableStrategy = attributeData.NullableStrategy ?? NullableStrategy.Default;
        StaticCreator = attributeData.DefaultStaticCreator ?? true;
        ImplicitCast = attributeData.ImplicitCast ?? false;
        ShouldGenerateMethodsForUnreachableProperties = attributeData.GenerateMethodsForUnreachableProperties ?? false;
        OriginalLocation = builderSymbol.Locations.First();
        StaticFactoryMethodName = attributeData.StaticFactoryMethodName;

        if (string.IsNullOrWhiteSpace(BuildingMethodsPrefix))
            throw new ArgumentNullException(nameof(attributeData), "Prefix name shouldn't be empty!");

        _buildingMethods = [];
        _fields = [];
        var members = builderSymbol.GetMembers();
        foreach (var member in members)
        {
            switch (member)
            {
                case IMethodSymbol { MethodKind: MethodKind.Ordinary } method
                when method.Name.StartsWith(BuildingMethodsPrefix)
                && method.Name != DefaultConstants.BuildMethodName:
                    _buildingMethods.Add(method.Name, method);
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Ordinary, Name: DefaultConstants.PostBuildMethodName }:
                    IsPostBuildMethodOverriden = true;
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Ordinary, Name: DefaultConstants.BuildMethodName, Parameters.Length: 0 }:
                    IsBuildMethodOverriden = true;
                    _diagnostics.Add(new BuildenatorDiagnostic(
                        BuildenatorDiagnosticDescriptors.BuildMethodOverridenDiagnostic,
                        OriginalLocation));
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Constructor, Parameters.Length: 0, IsImplicitlyDeclared: false }:
                    IsDefaultConstructorOverriden = true;
                    _diagnostics.Add(new BuildenatorDiagnostic(
                        BuildenatorDiagnosticDescriptors.DefaultConstructorOverridenDiagnostic,
                        OriginalLocation));
                    break;
                case IFieldSymbol field:
                    _fields.Add(field.Name, field);
                    break;
            }
        }
    }

    public string ContainingNamespace { get; }
    public string Name { get; }
    public string FullName { get; }
    public string BuildingMethodsPrefix { get; }
    public NullableStrategy NullableStrategy { get; }
    public bool StaticCreator { get; }
    public bool ImplicitCast { get; }
    public bool IsPostBuildMethodOverriden { get; }
    public bool IsDefaultConstructorOverriden { get; }
    public bool ShouldGenerateMethodsForUnreachableProperties { get; }
    public bool IsBuildMethodOverriden { get; }
    public Location OriginalLocation { get; }
    public string? StaticFactoryMethodName { get; }

    public IReadOnlyDictionary<string, IMethodSymbol> BuildingMethods => _buildingMethods;
    public IReadOnlyDictionary<string, IFieldSymbol> Fields => _fields;

    public IEnumerable<BuildenatorDiagnostic> Diagnostics => _diagnostics;
}