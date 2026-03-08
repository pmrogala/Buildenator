using Microsoft.CodeAnalysis;
#if DEBUG
#endif
using System.Linq;
using System.Collections.Immutable;
using System;

namespace Buildenator;

internal sealed class AssemblyPropertiesWrapper(
    ImmutableArray<TypedConstant>? globalFixtureProperties,
    ImmutableArray<TypedConstant>? mockingConfigurationBuilder,
    ImmutableArray<TypedConstant>? globalBuilderProperties) : IEquatable<AssemblyPropertiesWrapper>
{
    public readonly ImmutableArray<TypedConstant>? GlobalFixtureProperties = globalFixtureProperties;
    public readonly ImmutableArray<TypedConstant>? MockingConfigurationBuilder = mockingConfigurationBuilder;
    public readonly ImmutableArray<TypedConstant>? GlobalBuilderProperties = globalBuilderProperties;

    public override bool Equals(object? obj) => obj is AssemblyPropertiesWrapper other && Equals(other);

    public override int GetHashCode()
    {
        int hashCode = 874553126;
        hashCode = hashCode * -1521134295 + GlobalFixtureProperties?.GetHashCode() ?? 0;
        hashCode = hashCode * -1521134295 + MockingConfigurationBuilder?.GetHashCode() ?? 0;
        hashCode = hashCode * -1521134295 + GlobalBuilderProperties?.GetHashCode() ?? 0;
        return hashCode;
    }

    public void Deconstruct(out ImmutableArray<TypedConstant>? globalFixtureProperties, out ImmutableArray<TypedConstant>? mockingConfigurationBuilder, out ImmutableArray<TypedConstant>? globalBuilderProperties)
    {
        globalFixtureProperties = GlobalFixtureProperties;
        mockingConfigurationBuilder = MockingConfigurationBuilder;
        globalBuilderProperties = GlobalBuilderProperties;
    }

    public bool Equals(AssemblyPropertiesWrapper other)
    {
        return (GlobalFixtureProperties?.SequenceEqual(other.GlobalFixtureProperties ?? []) ?? (GlobalFixtureProperties is null && other.GlobalFixtureProperties is null))
               && (MockingConfigurationBuilder?.SequenceEqual(other.MockingConfigurationBuilder ?? []) ?? (MockingConfigurationBuilder is null && other.MockingConfigurationBuilder is null))
               && (GlobalBuilderProperties?.SequenceEqual(other.GlobalBuilderProperties ?? []) ?? (GlobalBuilderProperties is null && other.GlobalBuilderProperties is null));
    }
}