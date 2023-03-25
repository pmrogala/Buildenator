using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.AutoMoq;
using Buildenator.CodeAnalysis;
using Buildenator.Configuration;
using Buildenator.Configuration.Contract;
using Buildenator.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Buildenator.UnitTests.Generators;

public class PropertiesStringGeneratorTests
{
    private readonly IFixture _fixture;
    private readonly IBuilderProperties _builder ;
    private readonly IEntityToBuild _entity ;
    private readonly ITypedSymbol _typedSymbol;

    public PropertiesStringGeneratorTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization
        {
            ConfigureMembers = true,
            GenerateDelegates = true
        });
        _builder = _fixture.Create<IBuilderProperties>();
        _entity = _fixture.Create<IEntityToBuild>();
        _typedSymbol = _fixture.Create<ITypedSymbol>();
    }

    [Fact]
    public void GeneratePropertiesCode_ShouldGenerateValidCode_WhenPropertiesAreSet()
    {
        // Arrange
        var typedSymbol1 = _fixture.Create<ITypedSymbol>();
        var typedSymbol2 = _fixture.Create<ITypedSymbol>();
        var properties = new[] { typedSymbol1, typedSymbol2 };

        var generator = new PropertiesStringGenerator(_builder, _entity);
        Mock.Get(_entity).Setup(x => x.GetAllUniqueSettablePropertiesAndParameters()).Returns(properties);
        Mock.Get(_entity).Setup(x => x.GetAllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch())
            .Returns(Array.Empty<ITypedSymbol>());

        // Act
        var result = generator.GeneratePropertiesCode();

        // Assert
        result.Should().Contain(typedSymbol1.UnderScoreName);
        result.Should().Contain(typedSymbol2.UnderScoreName);
    }

    [Fact]
    public void
        GeneratePropertiesCode_ShouldGenerateValidCode_WhenShouldGenerateMethodsForUnreachablePropertiesIsTrue()
    {
        // Arrange
        var typedSymbol1 = _fixture.Create<ITypedSymbol>();
        var typedSymbol2 = _fixture.Create<ITypedSymbol>();
        var properties = new[] { typedSymbol1 };
        var readOnlyProperties = new[] { typedSymbol2 };

        var generator = new PropertiesStringGenerator(_builder, _entity);
        Mock.Get(_builder).Setup(x => x.ShouldGenerateMethodsForUnreachableProperties).Returns(true);
        Mock.Get(_entity).Setup(x => x.GetAllUniqueSettablePropertiesAndParameters()).Returns(properties);
        Mock.Get(_entity).Setup(x => x.GetAllUniqueReadOnlyPropertiesWithoutConstructorsParametersMatch())
            .Returns(readOnlyProperties);

        // Act
        var result = generator.GeneratePropertiesCode();

        // Assert
        result.Should().Contain(typedSymbol1.UnderScoreName);
        result.Should().Contain(typedSymbol2.UnderScoreName);
    }

    [Fact]
    public void GeneratePropertiesCode_ShouldNotGenerateCodeForAlreadyDeclaredFieldsOrMethods()
    {
        // Arrange
        var properties = new[] { _typedSymbol };

        var generator = new PropertiesStringGenerator(_builder, _entity);
        Mock.Get(_entity).Setup(x => x.GetAllUniqueSettablePropertiesAndParameters()).Returns(properties);

        var existingField = _typedSymbol.UnderScoreName;
        var existingMethod = $"Build{_typedSymbol.SymbolPascalName}";
        Mock.Get(_builder).Setup(x => x.Fields).Returns(new Dictionary<string, IFieldSymbol> { { existingField, null! } });
        Mock.Get(_builder).Setup(x => x.BuildingMethods).Returns(new Dictionary<string, IMethodSymbol>
            { { existingMethod, null! } });
        // Act
        var result = generator.GeneratePropertiesCode();

        // Assert
        result.Should().NotContain($"{_typedSymbol.TypeFullName} {_typedSymbol.UnderScoreName}");
        result.Should().NotContain(existingMethod);
    }

    [Fact]
    public void GeneratePropertiesCode_ShouldGenerateMethodDefinitionForMockableTypedSymbols()
    {
        // Arrange
            
        var properties = new[] { _typedSymbol };

        var generator = new PropertiesStringGenerator(_builder, _entity);
        Mock.Get(_entity).Setup(x => x.GetAllUniqueSettablePropertiesAndParameters()).Returns(properties);
        Mock.Get(_typedSymbol).Setup(x => x.IsMockable()).Returns(true);

        // Act
        var result = generator.GeneratePropertiesCode();

        // Assert
        result.Should().Contain(DefaultConstants.SetupActionLiteral);
    }
        
    [Fact]
    public void GeneratePropertiesCode_ShouldGenerateMethodDefinitionForNonMockableTypedSymbols()
    {
        // Arrange
        var properties = new[] { _typedSymbol };

        var generator = new PropertiesStringGenerator(_builder, _entity);
        Mock.Get(_entity).Setup(x => x.GetAllUniqueSettablePropertiesAndParameters()).Returns(properties);
        Mock.Get(_typedSymbol).Setup(x => x.IsMockable()).Returns(false);

        // Act
        var result = generator.GeneratePropertiesCode();

        // Assert
        result.Should().Contain(DefaultConstants.NullBox);
        result.Should().Contain(DefaultConstants.ValueLiteral);
    }

    [Fact]
    public void GeneratePropertiesCode_ShouldGenerateMethodDefinitionWithBuilderFullName()
    {
        // Arrange
        var properties = new[] { _typedSymbol };

        var generator = new PropertiesStringGenerator(_builder, _entity);
        Mock.Get(_entity).Setup(x => x.GetAllUniqueSettablePropertiesAndParameters()).Returns(properties);

        // Act
        var result = generator.GeneratePropertiesCode();

        // Assert
        result.Should().Contain(_builder.FullName);
    }

    [Fact]
    public void GeneratePropertiesCode_ShouldGenerateMethodDefinitionWithGeneratedMethodParameterDefinition()
    {
        // Arrange
        var properties = new[] { _typedSymbol };

        var generator = new PropertiesStringGenerator(_builder, _entity);
        Mock.Get(_entity).Setup(x => x.GetAllUniqueSettablePropertiesAndParameters()).Returns(properties);

        // Act
        var result = generator.GeneratePropertiesCode();

        // Assert
        result.Should().Contain(_typedSymbol.GenerateMethodParameterDefinition());
    }
}