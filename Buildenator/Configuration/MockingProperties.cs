using Buildenator.Abstraction;
using Buildenator.Configuration.Contract;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using Buildenator.Extensions;

namespace Buildenator.Configuration;

internal readonly struct MockingProperties : IMockingProperties
{
    public static MockingProperties? CreateOrDefault(
        ImmutableArray<TypedConstant>? globalProperties,
        ImmutableArray<TypedConstant>? localMockingProperties)
    {
            
        if ((localMockingProperties ?? globalProperties) is not { } attributeParameters)
            return null;

        var strategy = attributeParameters.GetOrThrow<MockingInterfacesStrategy>(0, nameof(Strategy));
        var typeDeclarationFormat = attributeParameters.GetOrThrow(1, nameof(TypeDeclarationFormat));
        var defaultValueAssignmentFormat = attributeParameters.GetOrThrow(2, nameof(FieldDefaultValueAssignmentFormat));
        var returnObjectFormat = attributeParameters.GetOrThrow(3, nameof(ReturnObjectFormat));
        var additionalNamespaces = (string?)attributeParameters[4].Value;

        return new MockingProperties(
            strategy,
            typeDeclarationFormat,
            defaultValueAssignmentFormat,
            returnObjectFormat,
            additionalNamespaces?.Split(',') ?? Array.Empty<string>());
    }

    private MockingProperties(
        MockingInterfacesStrategy strategy,
        string typeDeclarationFormat,
        string fieldDefaultValueAssignmentFormat,
        string returnObjectFormat,
        string[] additionalNamespaces)
    {
        Strategy = strategy;
        TypeDeclarationFormat = typeDeclarationFormat;
        FieldDefaultValueAssignmentFormat = fieldDefaultValueAssignmentFormat;
        ReturnObjectFormat = returnObjectFormat;
        AdditionalNamespaces = additionalNamespaces;
    }

    public MockingInterfacesStrategy Strategy { get; }
    public string TypeDeclarationFormat { get; }
    public string FieldDefaultValueAssignmentFormat { get; }
    public string ReturnObjectFormat { get; }
    public string[] AdditionalNamespaces { get; }

}