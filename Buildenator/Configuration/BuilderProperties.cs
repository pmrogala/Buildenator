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
    private readonly Dictionary<string, List<IMethodSymbol>> _buildingMethods;
    private readonly Dictionary<string, IFieldSymbol> _fields;
    private readonly HashSet<string> _defaultValueNames;
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
        bool? generateStaticPropertyForBuilderCreation = null;
        bool? initializeCollectionsWithEmpty = null;
        bool? useChildBuilders = null;

        if (globalAttributes.HasValue)
        {
            defaultNameWith = globalAttributes.Value.GetOrThrow<string>(0, nameof(MakeBuilderAttributeInternal.BuildingMethodsPrefix));
            defaultStaticBuilder = globalAttributes.Value.GetOrThrow<bool>(1, nameof(MakeBuilderAttributeInternal.GenerateDefaultBuildMethod));
            nullableStrategy = globalAttributes.Value.GetOrThrow<NullableStrategy>(2, nameof(MakeBuilderAttributeInternal.NullableStrategy));
            generateMethodsForUnreachableProperties = globalAttributes.Value.GetOrThrow<bool>(3, nameof(MakeBuilderAttributeInternal.GenerateMethodsForUnreachableProperties));
            implicitCast = globalAttributes.Value.GetOrThrow<bool>(4, nameof(MakeBuilderAttributeInternal.ImplicitCast));
            generateStaticPropertyForBuilderCreation = globalAttributes.Value.GetOrThrow<bool>(5, nameof(MakeBuilderAttributeInternal.GenerateStaticPropertyForBuilderCreation));
            initializeCollectionsWithEmpty = globalAttributes.Value.GetOrThrow<bool>(6, nameof(MakeBuilderAttributeInternal.InitializeCollectionsWithEmpty));
            useChildBuilders = globalAttributes.Value.GetOrThrow<bool>(7, nameof(MakeBuilderAttributeInternal.UseChildBuilders));
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
                builderAttribute.GenerateDefaultBuildMethod ?? defaultStaticBuilder,
                nullableStrategy,
                builderAttribute.GenerateMethodsForUnreachableProperties ??
                generateMethodsForUnreachableProperties,
                builderAttribute.ImplicitCast ?? implicitCast,
                builderAttribute.StaticFactoryMethodName,
                builderAttribute.GenerateStaticPropertyForBuilderCreation ?? generateStaticPropertyForBuilderCreation,
                builderAttribute.InitializeCollectionsWithEmpty ?? initializeCollectionsWithEmpty,
                builderAttribute.UseChildBuilders ?? useChildBuilders));
    }

    private BuilderProperties(INamespaceOrTypeSymbol builderSymbol, MakeBuilderAttributeInternal attributeData)
    {
        ContainingNamespace = builderSymbol.ContainingNamespace.ToDisplayString();
        Name = builderSymbol.Name;
        FullName = builderSymbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
        BuildingMethodsPrefix = attributeData.BuildingMethodsPrefix ?? DefaultConstants.BuildingMethodsPrefix;
        NullableStrategy = attributeData.NullableStrategy ?? NullableStrategy.Default;
        GenerateDefaultBuildMethod = attributeData.GenerateDefaultBuildMethod ?? true;
        ImplicitCast = attributeData.ImplicitCast ?? false;
        ShouldGenerateMethodsForUnreachableProperties = attributeData.GenerateMethodsForUnreachableProperties ?? false;
        OriginalLocation = builderSymbol.Locations.First();
        StaticFactoryMethodName = attributeData.StaticFactoryMethodName;
        GenerateStaticPropertyForBuilderCreation = attributeData.GenerateStaticPropertyForBuilderCreation ?? false;
        InitializeCollectionsWithEmpty = attributeData.InitializeCollectionsWithEmpty ?? true;
        UseChildBuilders = attributeData.UseChildBuilders ?? true;

        if (string.IsNullOrWhiteSpace(BuildingMethodsPrefix))
            throw new ArgumentNullException(nameof(attributeData), "Prefix name shouldn't be empty!");

        _buildingMethods = [];
        _fields = [];
        _defaultValueNames = [];
        var members = builderSymbol.GetMembers();
        foreach (var member in members)
        {
            switch (member)
            {
                case IMethodSymbol { MethodKind: MethodKind.Ordinary } method
                when method.Name.StartsWith(BuildingMethodsPrefix)
                && method.Name != DefaultConstants.BuildMethodName:
                    if (!_buildingMethods.TryGetValue(method.Name, out var methods))
                    {
                        methods = [];
                        _buildingMethods.Add(method.Name, methods);
                    }
                    methods.Add(method);
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Ordinary, Name: DefaultConstants.PreBuildMethodName }:
                    IsPreBuildMethodOverriden = true;
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
                case IMethodSymbol { MethodKind: MethodKind.Ordinary, Name: DefaultConstants.BuildManyMethodName }:
                    IsBuildManyMethodOverriden = true;
                    _diagnostics.Add(new BuildenatorDiagnostic(
                        BuildenatorDiagnosticDescriptors.BuildManyMethodOverridenDiagnostic,
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
                    // Track fields that follow the Default{PropertyName} naming convention
                    if (IsAccessibleDefaultValueMember(field.IsStatic, field.DeclaredAccessibility, field.Name))
                    {
                        _defaultValueNames.Add(field.Name);
                    }
                    break;
                case IPropertySymbol property:
                    // Track static properties that follow the Default{PropertyName} naming convention
                    if (IsAccessibleDefaultValueMember(property.IsStatic, property.DeclaredAccessibility, property.Name))
                    {
                        _defaultValueNames.Add(property.Name);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Checks if a member is a valid default value member based on naming convention and accessibility.
    /// </summary>
    private static bool IsAccessibleDefaultValueMember(bool isStatic, Accessibility accessibility, string memberName)
    {
        // Must be static
        if (!isStatic)
            return false;
        
        // Must follow Default{PropertyName} naming convention
        if (!memberName.StartsWith(DefaultConstants.DefaultFieldPrefix) || memberName.Length <= DefaultConstants.DefaultFieldPrefix.Length)
            return false;
        
        // Must be accessible (public or internal) to be used in generated code
        return accessibility == Accessibility.Public || accessibility == Accessibility.Internal;
    }

    public string ContainingNamespace { get; }
    public string Name { get; }
    public string FullName { get; }
    public string BuildingMethodsPrefix { get; }
    public NullableStrategy NullableStrategy { get; }
    public bool GenerateDefaultBuildMethod { get; }
    public bool ImplicitCast { get; }
    public bool IsPreBuildMethodOverriden { get; }
    public bool IsPostBuildMethodOverriden { get; }
    public bool IsDefaultConstructorOverriden { get; }
    public bool ShouldGenerateMethodsForUnreachableProperties { get; }
    public bool IsBuildMethodOverriden { get; }
    public bool IsBuildManyMethodOverriden { get; }
    public Location OriginalLocation { get; }
    public string? StaticFactoryMethodName { get; }
    public bool GenerateStaticPropertyForBuilderCreation { get; }
    public bool InitializeCollectionsWithEmpty { get; }
    public bool UseChildBuilders { get; }

    public IReadOnlyDictionary<string, List<IMethodSymbol>> BuildingMethods => _buildingMethods;
    public IReadOnlyDictionary<string, IFieldSymbol> Fields => _fields;
    public IReadOnlyCollection<string> DefaultValueNames => _defaultValueNames;

    public IEnumerable<BuildenatorDiagnostic> Diagnostics => _diagnostics;
}