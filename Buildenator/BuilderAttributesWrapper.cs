using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Immutable;

namespace Buildenator;

internal sealed class BuilderAttributesWrapper(
    INamedTypeSymbol BuilderSymbol,
    MakeBuilderAttributeInternal BuilderAttribute,
    ImmutableArray<TypedConstant>? MockingAttribute,
    ImmutableArray<TypedConstant>? FixtureAttribute) : System.IEquatable<BuilderAttributesWrapper>
{
    public INamedTypeSymbol BuilderSymbol { get; } = BuilderSymbol;
    public MakeBuilderAttributeInternal BuilderAttribute { get; } = BuilderAttribute;
    public ImmutableArray<TypedConstant>? MockingAttribute { get; } = MockingAttribute;
    public ImmutableArray<TypedConstant>? FixtureAttribute { get; } = FixtureAttribute;

    public bool Equals(BuilderAttributesWrapper other)
    {
        return 
            BuilderSymbol.Equals(other.BuilderSymbol, SymbolEqualityComparer.Default)
            && BuilderAttribute.Equals(other.BuilderAttribute)
            && (MockingAttribute?.SequenceEqual(other.MockingAttribute ?? []) ?? (MockingAttribute is null && other.MockingAttribute is null))
            && (FixtureAttribute?.SequenceEqual(other.FixtureAttribute ?? []) ?? (FixtureAttribute is null && other.FixtureAttribute is null));
    }

    public void Deconstruct(out INamedTypeSymbol BuilderSymbol, out MakeBuilderAttributeInternal BuilderAttribute, out ImmutableArray<TypedConstant>? MockingAttribute, out ImmutableArray<TypedConstant>? FixtureAttribute)
    {
        BuilderSymbol = this.BuilderSymbol;
        BuilderAttribute = this.BuilderAttribute;
        MockingAttribute = this.MockingAttribute;
        FixtureAttribute = this.FixtureAttribute;
    }

    public override bool Equals(object? obj) => obj is BuilderAttributesWrapper other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = (hash * 31) + SymbolEqualityComparer.Default.GetHashCode(BuilderSymbol);
            hash = (hash * 31) + BuilderAttribute.GetHashCode();

            if (MockingAttribute != null)
            {
                foreach (var item in MockingAttribute)
                {
                    hash = (hash * 31) + item.GetHashCode();
                }
            }
            else
            {
                hash *= 31;
            }

            if (FixtureAttribute != null)
            {
                foreach (var item in FixtureAttribute)
                {
                    hash = (hash * 31) + item.GetHashCode();
                }
            }
            else
            {
                hash *= 31;
            }

            return hash;
        }
    }
}