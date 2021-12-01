using Buildenator.Configuration;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Buildenator
{
    internal sealed class TypedSymbol
    {
        public TypedSymbol(IPropertySymbol symbol, MockingProperties? mockingConfiguration, FixtureProperties? fixtureConfiguration)
        {
            Symbol = symbol;
            Type = symbol.Type;
            _mockingConfiguration = mockingConfiguration;
            _fixtureConfiguration = fixtureConfiguration;
        }

        public TypedSymbol(IParameterSymbol symbol, MockingProperties? mockingConfiguration, FixtureProperties? fixtureConfiguration)
        {
            Symbol = symbol;
            Type = symbol.Type;
            _mockingConfiguration = mockingConfiguration;
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

        private readonly MockingProperties? _mockingConfiguration;
        private bool? _isMockable = null;
        public bool IsMockable()
            => _isMockable ??= _mockingConfiguration switch
            {
                MockingProperties { Strategy: Abstraction.MockingInterfacesStrategy.All }
                    when Type.TypeKind == TypeKind.Interface => true,
                MockingProperties { Strategy: Abstraction.MockingInterfacesStrategy.WithoutGenericCollection }
                    when Type.TypeKind == TypeKind.Interface && Type.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => true,
                _ => false
            };


        private readonly FixtureProperties? _fixtureConfiguration;
        private bool? _IsFakeable = null;
        public bool IsFakeable()
            => _IsFakeable ??= _fixtureConfiguration switch
            {
                null => false,
                FixtureProperties { Strategy: Abstraction.FixtureInterfacesStrategy.None }
                    when Type.TypeKind == TypeKind.Interface => false,
                FixtureProperties { Strategy: Abstraction.FixtureInterfacesStrategy.OnlyGenericCollections }
                    when Type.TypeKind == TypeKind.Interface && Type.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => false,
                _ => true
            };
    }
}