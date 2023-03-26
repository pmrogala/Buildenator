using System;
using System.Collections.Immutable;
using Buildenator.Abstraction;
using Buildenator.Configuration;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Buildenator.UnitTests.Configuration;

public class BuilderPropertiesTests
{
    private readonly Mock<INamedTypeSymbol> _builderSymbolMock;

    public BuilderPropertiesTests()
    {
        _builderSymbolMock = new Mock<INamedTypeSymbol>();
        var namespaceSymbol = new Mock<INamespaceSymbol>();
        namespaceSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("Test");
        _builderSymbolMock.Setup(
                x => x.ToDisplayString(
                    It.Is<SymbolDisplayFormat>(a => a.GenericsOptions == SymbolDisplayGenericsOptions.IncludeTypeParameters)))
            .Returns("BuilderFullName");
        _builderSymbolMock.Setup(x => x.ContainingNamespace).Returns(namespaceSymbol.Object);
        _builderSymbolMock.Setup(x => x.Name).Returns("BuilderName");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenBuildingMethodsPrefixIsEmpty()
    {
        // Arrange
        var attributeDataMock = new MakeBuilderAttributeInternal(null!, string.Empty, false,
            NullableStrategy.Enabled, false, true);

        // Act
        var act = () => new BuilderProperties(_builderSymbolMock.Object, attributeDataMock);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldInitializePropertiesAndCollections_WhenValidParametersAreProvided()
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, "Build", false,
            NullableStrategy.Enabled, false, true);
        var methodSymbolMock = new Mock<IMethodSymbol>();
        methodSymbolMock.SetupGet(x => x.Name).Returns("BuildMethod");
        methodSymbolMock.SetupGet(x => x.MethodKind).Returns(MethodKind.Ordinary);
        _builderSymbolMock.Setup(x => x.GetMembers()).Returns(ImmutableArray.Create((ISymbol)methodSymbolMock.Object));

        // Act
        var properties = new BuilderProperties(_builderSymbolMock.Object, attributeDataMock);

        // Assert
        properties.ContainingNamespace.Should().NotBeNullOrEmpty();
        properties.Name.Should().NotBeNullOrEmpty();
        properties.FullName.Should().NotBeNullOrEmpty();
        properties.BuildingMethodsPrefix.Should().Be(attributeDataMock.BuildingMethodsPrefix);
        properties.NullableStrategy.Should().Be(attributeDataMock.NullableStrategy!.Value);
        properties.StaticCreator.Should().Be(attributeDataMock.DefaultStaticCreator!.Value);
        properties.ImplicitCast.Should().Be(attributeDataMock.ImplicitCast!.Value);
        properties.ShouldGenerateMethodsForUnreachableProperties.Should().Be(attributeDataMock.GenerateMethodsForUnreachableProperties!.Value);
        properties.IsPostBuildMethodOverriden.Should().BeFalse();
        properties.IsDefaultConstructorOverriden.Should().BeFalse();
        properties.BuildingMethods.Should().ContainKey(methodSymbolMock.Object.Name).And.ContainValue(methodSymbolMock.Object);
        properties.Fields.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Build", 1, false, Accessibility.Public)]
    [InlineData("Build", 0, false, Accessibility.Public)]
    [InlineData("With", 1, false, Accessibility.Public)]
    [InlineData("Build", 0, true, Accessibility.Public)]
    [InlineData("Build", 1, false, Accessibility.Private)]
    [InlineData("Build", 0, false, Accessibility.Private)]
    [InlineData("With", 1, false, Accessibility.Private)]
    [InlineData("Build", 0, true, Accessibility.Private)]
    [InlineData("Build", 1, false, Accessibility.Protected)]
    [InlineData("Build", 0, false, Accessibility.Protected)]
    [InlineData("With", 1, false, Accessibility.Protected)]
    [InlineData("Build", 0, true, Accessibility.Protected)]
    [InlineData("Build", 1, false, Accessibility.Internal)]
    [InlineData("Build", 0, false, Accessibility.Internal)]
    [InlineData("With", 1, false, Accessibility.Internal)]
    [InlineData("Build", 0, true, Accessibility.Internal)]
    public void Constructor_ShouldOnlyPopulateBuildingMethodsDictionary_WhenOrdinaryBuildingMethodIsFound(
        string prefix, int parametersLength, bool isImplicitlyDeclared, Accessibility accessibility)
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, prefix, false, NullableStrategy.Enabled, false, true);

        var methodSymbolMock = new Mock<IMethodSymbol>();
        methodSymbolMock.SetupGet(x => x.Name).Returns($"{prefix}Method");
        methodSymbolMock.SetupGet(x => x.MethodKind).Returns(MethodKind.Ordinary);
        methodSymbolMock.SetupGet(x => x.DeclaredAccessibility).Returns(accessibility);
        if (parametersLength == 0)
            methodSymbolMock.SetupGet(x => x.Parameters).Returns(ImmutableArray<IParameterSymbol>.Empty);
        else
            methodSymbolMock.SetupGet(x => x.Parameters)
                .Returns(ImmutableArray.Create(new Mock<IParameterSymbol>().Object));
        methodSymbolMock.SetupGet(x => x.IsImplicitlyDeclared).Returns(isImplicitlyDeclared);

        _builderSymbolMock.Setup(x => x.GetMembers()).Returns(ImmutableArray.Create((ISymbol)methodSymbolMock.Object));

        // Act
        var properties = new BuilderProperties(_builderSymbolMock.Object, attributeDataMock);

        // Assert
        properties.BuildingMethods.Should().ContainKey(methodSymbolMock.Object.Name).And.ContainValue(methodSymbolMock.Object);
        properties.Fields.Should().BeEmpty();
        properties.IsDefaultConstructorOverriden.Should().BeFalse();
        properties.IsPostBuildMethodOverriden.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldOnlySetIsPostBuildMethodOverridenToTrue_WhenPostBuildMethodIsFound()
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, "Build", false, NullableStrategy.Enabled, false, true);

        var methodSymbolMock = new Mock<IMethodSymbol>();
        methodSymbolMock.SetupGet(x => x.Name).Returns(DefaultConstants.PostBuildMethodName);
        methodSymbolMock.SetupGet(x => x.MethodKind).Returns(MethodKind.Ordinary);

        _builderSymbolMock.Setup(x => x.GetMembers()).Returns(ImmutableArray.Create((ISymbol)methodSymbolMock.Object));

        // Act
        var properties = new BuilderProperties(_builderSymbolMock.Object, attributeDataMock);

        // Assert
        properties.IsPostBuildMethodOverriden.Should().BeTrue();
        properties.BuildingMethods.Should().BeEmpty();
        properties.Fields.Should().BeEmpty();
        properties.IsDefaultConstructorOverriden.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldOnlySetIsDefaultConstructorOverridenToTrue_WhenTheDefaultConstructorHasTheSameNameAsThePrefix()
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, "Build", false, NullableStrategy.Enabled, false, true);

        var methodSymbolMock = new Mock<IMethodSymbol>();
        methodSymbolMock.SetupGet(x => x.MethodKind).Returns(MethodKind.Constructor);
        methodSymbolMock.SetupGet(x => x.Parameters).Returns(ImmutableArray<IParameterSymbol>.Empty);
        methodSymbolMock.SetupGet(x => x.Name).Returns(attributeDataMock.BuildingMethodsPrefix!);
        methodSymbolMock.SetupGet(x => x.IsImplicitlyDeclared).Returns(false);

        _builderSymbolMock.Setup(x => x.GetMembers()).Returns(ImmutableArray.Create((ISymbol)methodSymbolMock.Object));

        // Act
        var properties = new BuilderProperties(_builderSymbolMock.Object, attributeDataMock);

        // Assert
        properties.IsDefaultConstructorOverriden.Should().BeTrue();
        properties.IsPostBuildMethodOverriden.Should().BeFalse();
        properties.BuildingMethods.Should().BeEmpty();
        properties.Fields.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldOnlySetIsDefaultConstructorOverridenToTrue_WhenDefaultConstructorIsFound()
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, "Build", false, NullableStrategy.Enabled, false, true);

        var methodSymbolMock = new Mock<IMethodSymbol>();
        methodSymbolMock.SetupGet(x => x.MethodKind).Returns(MethodKind.Constructor);
        methodSymbolMock.SetupGet(x => x.Parameters).Returns(ImmutableArray<IParameterSymbol>.Empty);
        methodSymbolMock.SetupGet(x => x.Name).Returns("Constructor");
        methodSymbolMock.SetupGet(x => x.IsImplicitlyDeclared).Returns(false);

        _builderSymbolMock.Setup(x => x.GetMembers()).Returns(ImmutableArray.Create((ISymbol)methodSymbolMock.Object));

        // Act
        var properties = new BuilderProperties(_builderSymbolMock.Object, attributeDataMock);

        // Assert
        properties.IsDefaultConstructorOverriden.Should().BeTrue();
        properties.IsPostBuildMethodOverriden.Should().BeFalse();
        properties.BuildingMethods.Should().BeEmpty();
        properties.Fields.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldOnlyAddFieldToFieldsDictionary_WhenFieldIsFound()
    {
        // Arrange
        var typeSymbolMock = new Mock<INamedTypeSymbol>();
        var attributeDataMock = new MakeBuilderAttributeInternal(typeSymbolMock.Object, "Build", false, NullableStrategy.Enabled, false, true);

        var fieldSymbolMock = new Mock<IFieldSymbol>();
        fieldSymbolMock.SetupGet(x => x.Name).Returns("SomeField");

        _builderSymbolMock.Setup(x => x.GetMembers()).Returns(ImmutableArray.Create(new[] { (ISymbol)fieldSymbolMock.Object }));

        // Act
        var properties = new BuilderProperties(_builderSymbolMock.Object, attributeDataMock);

        // Assert
        properties.Fields.Should().ContainKey(fieldSymbolMock.Object.Name).And.ContainValue(fieldSymbolMock.Object);
        properties.IsDefaultConstructorOverriden.Should().BeFalse();
        properties.IsPostBuildMethodOverriden.Should().BeFalse();
        properties.BuildingMethods.Should().BeEmpty();
    }
}