using Buildenator.Abstraction;
using Buildenator.Configuration.Contract;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using Buildenator.Extensions;

namespace Buildenator.Configuration;

internal readonly struct FixtureProperties : IFixtureProperties
{
    private const string FixtureLiteral = "_fixture";
        
    public static FixtureProperties? CreateOrDefault(
        ImmutableArray<TypedConstant>? globalFixtureProperties,
        ImmutableArray<TypedConstant>? localFixtureProperties)
    {
        return (localFixtureProperties ?? globalFixtureProperties) is { } notNullProperties
            ? new FixtureProperties(notNullProperties)
            : null;
    }

    private FixtureProperties(ImmutableArray<TypedConstant> attributeParameters)
    {
        var i = 0;
        Name = attributeParameters.GetOrThrow(i++, nameof(Name));
        CreateSingleFormat = attributeParameters.GetOrThrow(i++, nameof(CreateSingleFormat));
        ConstructorParameters = (string?)attributeParameters[i++].Value;
        AdditionalConfiguration = (string?)attributeParameters[i++].Value;
        Strategy = attributeParameters.GetOrThrow<FixtureInterfacesStrategy>(i++, nameof(Strategy));
        AdditionalNamespaces = ((string?)attributeParameters[i].Value)?.Split(',') ?? Array.Empty<string>();
    }

    public string Name { get; }
    public string CreateSingleFormat { get; }
    public string? ConstructorParameters { get; }
    public string? AdditionalConfiguration { get; }
    public FixtureInterfacesStrategy Strategy { get; }
    public string[] AdditionalNamespaces { get; }

    public string GenerateAdditionalConfiguration()
        => AdditionalConfiguration is null ? string.Empty : string.Format(AdditionalConfiguration, FixtureLiteral, Name);

    public bool NeedsAdditionalConfiguration() => AdditionalConfiguration is not null;
}