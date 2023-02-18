using Buildenator.Abstraction;
using Buildenator.Configuration.Contract;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Linq;
using Buildenator.Configuration;

namespace Buildenator.CodeAnalysis
{
    internal sealed class TypedSymbol : ITypedSymbol
    {
        public TypedSymbol(IPropertySymbol symbol, IMockingProperties? mockingInterfaceStrategy, FixtureInterfacesStrategy? fixtureConfiguration)
        {
            Symbol = symbol;
            Type = symbol.Type;
            _mockingProperties = mockingInterfaceStrategy;
            _fixtureConfiguration = fixtureConfiguration;
        }

        public TypedSymbol(IParameterSymbol symbol, IMockingProperties? mockingInterfaceStrategy, FixtureInterfacesStrategy? fixtureConfiguration)
        {
            Symbol = symbol;
            Type = symbol.Type;
            _mockingProperties = mockingInterfaceStrategy;
            _fixtureConfiguration = fixtureConfiguration;
        }

        public bool NeedsFieldInit() => IsMockable();

        private ISymbol Symbol { get; }
        private ITypeSymbol Type { get; }

        private string? _underscoreName;
        public string UnderScoreName => _underscoreName ??= Symbol.UnderScoreName();

        private string? _typeFullName;
        public string TypeFullName => _typeFullName ??= Type.ToDisplayString();

        public string TypeName => Type.Name;

        public string SymbolPascalName => Symbol.PascalCaseName();
        public string SymbolName => Symbol.Name;

        private readonly IMockingProperties? _mockingProperties;
        private bool? _isMockable;
        public bool IsMockable()
            => _isMockable ??= _mockingProperties?.Strategy switch
            {
                MockingInterfacesStrategy.All
                    when Type.TypeKind == TypeKind.Interface => true,
                MockingInterfacesStrategy.WithoutGenericCollection
                    when Type.TypeKind == TypeKind.Interface && Type.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => true,
                _ => false
            };


        private readonly FixtureInterfacesStrategy? _fixtureConfiguration;
        private bool? _isFakeable;
        public bool IsFakeable()
            => _isFakeable ??= _fixtureConfiguration switch
            {
                null => false,
                FixtureInterfacesStrategy.None
                    when Type.TypeKind == TypeKind.Interface => false,
                FixtureInterfacesStrategy.OnlyGenericCollections
                    when Type.TypeKind == TypeKind.Interface && Type.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => false,
                _ => true
            };

        public string GenerateFieldInitialization()
            => _mockingProperties is null ? string.Empty : $"{UnderScoreName} = {string.Format(_mockingProperties.FieldDeafultValueAssigmentFormat, TypeFullName)};";

        
        public string GenerateFieldType()
	        => IsMockable() ? GenerateMockableFieldType() : TypeFullName;
        
        public string GenerateLazyFieldType()
	        => IsMockable() ? GenerateMockableFieldType() : $"Nullbox<{TypeFullName}>?";

        public string GenerateMethodParameterDefinition()
	        => IsMockable() ? $"Action<{GenerateMockableFieldType()}> {DefaultConstants.SetupActionLiteral}" : $"{TypeFullName} {DefaultConstants.ValueLiteral}";

        private string GenerateMockableFieldType() => string.Format(_mockingProperties!.TypeDeclarationFormat, TypeFullName);
    }
}