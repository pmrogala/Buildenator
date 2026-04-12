using Buildenator.Abstraction;
using Buildenator.CodeAnalysis;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Buildenator.UnitTests.CodeAnalysis;

public class TypedSymbolTests
{
    [Fact]
    public void GenerateLazyFieldValueReturn_WithDefaultValueName_ShouldUseDefaultValueInsteadOfDefaultType()
    {
        // Arrange - create a TypedSymbol for a parameter named "title"
        // with DefaultTitle in the default value names, no fixture configured
        var parameterSymbol = CreateMockParameterSymbol("title", "string");
        var defaultValueNames = new HashSet<string> { "DefaultTitle" };

        var sut = new TypedSymbol(
            parameterSymbol,
            mockingInterfaceStrategy: null,
            fixtureConfiguration: null,
            nullableStrategy: NullableStrategy.Default,
            defaultValueNames: defaultValueNames);

        // Act
        var result = sut.GenerateLazyFieldValueReturn();

        // Assert - should contain DefaultTitle, not default(string)
        _ = result.Should().Contain("DefaultTitle");
        _ = result.Should().NotContain("default(string)");
    }

    [Fact]
    public void GenerateLazyFieldValueReturn_WithoutDefaultValueName_ShouldUseDefaultType()
    {
        // Arrange - no default value names defined
        var parameterSymbol = CreateMockParameterSymbol("title", "string");

        var sut = new TypedSymbol(
            parameterSymbol,
            mockingInterfaceStrategy: null,
            fixtureConfiguration: null,
            nullableStrategy: NullableStrategy.Default,
            defaultValueNames: null);

        // Act
        var result = sut.GenerateLazyFieldValueReturn();

        // Assert - should use default(string) as fallback
        _ = result.Should().Contain("default(string)");
        _ = result.Should().NotContain("DefaultTitle");
    }

    [Fact]
    public void GenerateLazyFieldValueReturn_WithNonMatchingDefaultValueName_ShouldUseDefaultType()
    {
        // Arrange - default value names exist but don't match this parameter
        var parameterSymbol = CreateMockParameterSymbol("title", "string");
        var defaultValueNames = new HashSet<string> { "DefaultName" }; // doesn't match "title"

        var sut = new TypedSymbol(
            parameterSymbol,
            mockingInterfaceStrategy: null,
            fixtureConfiguration: null,
            nullableStrategy: NullableStrategy.Default,
            defaultValueNames: defaultValueNames);

        // Act
        var result = sut.GenerateLazyFieldValueReturn();

        // Assert - should use default(string) since DefaultTitle doesn't exist
        _ = result.Should().Contain("default(string)");
    }

    private static IParameterSymbol CreateMockParameterSymbol(string name, string typeDisplayString)
    {
        var typeMock = new Mock<ITypeSymbol>();
        typeMock.Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns(typeDisplayString);
        typeMock.Setup(t => t.ToDisplayString()).Returns(typeDisplayString);
        typeMock.Setup(t => t.Name).Returns(typeDisplayString);
        typeMock.Setup(t => t.TypeKind).Returns(TypeKind.Class);
        typeMock.Setup(t => t.AllInterfaces).Returns(System.Collections.Immutable.ImmutableArray<INamedTypeSymbol>.Empty);

        var paramMock = new Mock<IParameterSymbol>();
        paramMock.Setup(p => p.Name).Returns(name);
        paramMock.Setup(p => p.Type).Returns(typeMock.Object);

        return paramMock.Object;
    }
}
