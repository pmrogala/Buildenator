using Buildenator.Abstraction;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Buildenator.CodeAnalysis
{
    internal sealed class TypedSymbol : ITypedSymbol
    {
        public TypedSymbol(IPropertySymbol symbol, MockingInterfacesStrategy? mockingInterfaceStrategy, FixtureInterfacesStrategy? fixtureConfiguration)
        {
            Symbol = symbol;
            Type = symbol.Type;
            _mockingInterfaceStrategy = mockingInterfaceStrategy;
            _fixtureConfiguration = fixtureConfiguration;
        }

        public TypedSymbol(IParameterSymbol symbol, MockingInterfacesStrategy? mockingInterfaceStrategy, FixtureInterfacesStrategy? fixtureConfiguration)
        {
            Symbol = symbol;
            Type = symbol.Type;
            _mockingInterfaceStrategy = mockingInterfaceStrategy;
            _fixtureConfiguration = fixtureConfiguration;
        }

        private ISymbol Symbol { get; }
        private ITypeSymbol Type { get; }

        private string? _underscoreName = null;
        public string UnderScoreName => _underscoreName ??= Symbol.UnderScoreName();

        private string? _typeFullName = null;
        public string TypeFullName => _typeFullName ??= Type.ToDisplayString();

        public string TypeName => Type.Name;

        public string SymbolPascalName => Symbol.PascalCaseName();
        public string SymbolName => Symbol.Name;

        private readonly MockingInterfacesStrategy? _mockingInterfaceStrategy;
        private bool? _isMockable = null;
        public bool IsMockable()
            => _isMockable ??= _mockingInterfaceStrategy switch
            {
                MockingInterfacesStrategy.All
                    when Type.TypeKind == TypeKind.Interface => true,
                MockingInterfacesStrategy.WithoutGenericCollection
                    when Type.TypeKind == TypeKind.Interface && Type.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => true,
                _ => false
            };


        private readonly FixtureInterfacesStrategy? _fixtureConfiguration;
        private bool? _IsFakeable = null;
        public bool IsFakeable()
            => _IsFakeable ??= _fixtureConfiguration switch
            {
                null => false,
                FixtureInterfacesStrategy.None
                    when Type.TypeKind == TypeKind.Interface => false,
                FixtureInterfacesStrategy.OnlyGenericCollections
                    when Type.TypeKind == TypeKind.Interface && Type.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => false,
                _ => true
            };
    }
}