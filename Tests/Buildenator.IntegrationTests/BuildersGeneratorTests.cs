using AutoFixture.Xunit2;
using Buildenator.IntegrationTests.SharedEntities;
using Buildenator.IntegrationTests.Source.Builders;
using Buildenator.IntegrationTests.SharedEntities.DifferentNamespace;
using FluentAssertions;
using Xunit;
using PostBuildEntityBuilder = Buildenator.IntegrationTests.Source.Builders.PostBuildEntityBuilder;
using Newtonsoft.Json.Linq;

namespace Buildenator.IntegrationTests;

public class BuildersGeneratorTests
{
    [Fact]
    public void BuildersGenerator_GeneratesPostBuildMethod()
    {
        var builder = PostBuildEntityBuilder.PostBuildEntity;

        var result = builder.Build();

        _ = result.Entry.Should().Be(-1);
    }

    [Fact]
    public void BuildersGenerator_HasStaticBuilderFactory()
    {
        var builder = EntityBuilder.Entity;

        _ = builder.Should().NotBeNull();
    }

    [Fact]
    public void BuildersGenerator_HasImplicitCast()
    {
        var builder = ImplicitCastBuilder.Entity;
        Entity result = builder;

        _ = result.Should().NotBeNull();
    }

    [Fact]
    public void BuildersGenerator_BuilderNameNotReflectingTargetClassName_ShouldCompile()
    {
        var builder = DifferentNameBuilder.Entity;

        _ = builder.Should().NotBeNull();
    }

    [Fact]
    public void BuildersGenerator_DefaultBuilderTrue_ShouldHaveDefaultBuilder()
    {
        _ = typeof(DifferentNameBuilder).GetMethod("BuildDefault", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Should().NotBeNull();
    }

    [Fact]
    public void BuildersGenerator_DefaultBuilderFalse_ShouldNotHaveDefaultBuilder()
    {
        _ = typeof(EntityBuilder).GetMethod("BuildDefault", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Should().BeNull();
    }

    [Fact]
    public void BuildersGenerator_StaticFactoryMethod_ShouldUseTheFactoryMethodForConstructing()
    {
        var builder = new EntityWithStaticFactoryMethodBuilder();

        var entity = builder.WithDifferentNamespaceId(10).Build();
        var subEntity = entity.EntityInDifferentNamespace;
        _ = subEntity.Should().NotBeNull();
        _ = subEntity.Id.Should().Be(10);
    }

    [Fact]
    public void BuildersGenerator_StaticFactoryMethod_ShouldUseTheFactoryMethodForDefaultConstructing()
    {
        var entity = EntityWithStaticFactoryMethodBuilder.BuildDefault(_differentNamespaceId: 10);

        var subEntity = entity.EntityInDifferentNamespace;
        _ = subEntity.Should().NotBeNull();
        _ = subEntity.Id.Should().Be(10);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_OneConstructorWithoutSettableProperties_CreatedWithMethodsByConstructorParametersNames(int value, string str)
    {
        var builder = EntityBuilder.Entity;

        var entity = builder.WithPropertyIntGetter(value).WithPropertyStringGetter(str).Build();
        _ = entity.PropertyIntGetter.Should().Be(value);
        _ = entity.PropertyGetter.Should().Be(str);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_DifferentPrefixSet_MethodsNamesChanged(int value, string str)
    {
        var builder = SetEntityBuilder.Entity;

        var entity = builder.SetPropertyIntGetter(value).SetPropertyStringGetter(str).Build();
        _ = entity.PropertyIntGetter.Should().Be(value);
        _ = entity.PropertyGetter.Should().Be(str);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_ZeroConstructorsWithSettableProperties_CreatedWithMethodsBySettablePropertiesNames(int value, string str)
    {
        var builder = SettableEntityWithoutConstructorBuilder.SettableEntityWithoutConstructor;

        var entity = builder.WithPropertyIntGetter(value).WithPropertyGetter(str).Build();
        _ = entity.PropertyIntGetter.Should().Be(value);
        _ = entity.PropertyGetter.Should().Be(str);

    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_OneConstructorWithSettableProperties_CreatedWithMethodsBySettablePropertiesNamesIfInBothPlaces(
        int value, string str, string[] strings, int[] arr)
    {
        var builder = SettableEntityWithConstructorBuilder.SettableEntityWithConstructor;

        var entity = builder.WithPropertyInt(value).WithProperty(str)
            .WithNoConstructorProperty(strings).WithPrivateField(arr).Build();
        _ = entity.PropertyInt.Should().Be(value);
        _ = entity.Property.Should().Be(str);
        _ = entity.NoConstructorProperty.Should().BeSameAs(strings);
        _ = entity.GetPrivateField().Should().BeSameAs(arr);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_OneLevelInheritance_MoqAllInterfaces_AllFieldsCreatedAndInterfacesMocked(
        ChildEntity childEntity)
    {
        var builder = ChildEntityBuilder.ChildEntity;

        var result = builder
            .WithEntityInDifferentNamespace(childEntity.EntityInDifferentNamespace)
            .WithPrivateField(mock => mock.Setup(a => a.GetEnumerator()).Returns(childEntity.GetPrivateField().GetEnumerator()))
            .WithPropertyStringGetter(childEntity.PropertyGetter)
            .WithProtectedProperty(childEntity.GetProtectedProperty())
            .WithByteProperty(childEntity.ByteProperty)
            .WithPropertyIntGetter(childEntity.PropertyIntGetter)
            .Build();

        _ = result.Should().BeEquivalentTo(childEntity);
        _ = result.GetPrivateField().GetEnumerator().Should().BeEquivalentTo(childEntity.GetPrivateField().GetEnumerator());
        _ = result.GetProtectedProperty().Should().BeEquivalentTo(childEntity.GetProtectedProperty());
    }

    [Theory]
    [CustomAutoData]
    public void BuildersGenerator_TwoLevelsInheritance_AllFieldsCreated(
        GrandchildEntity grandchildEntity)
    {
        var builder = GrandchildEntityBuilder.GrandchildEntity;

        var result = builder
            .WithEntityInDifferentNamespace(grandchildEntity.EntityInDifferentNamespace)
            .WithPrivateField(grandchildEntity.GetPrivateField())
            .WithPropertyStringGetter(grandchildEntity.PropertyGetter)
            .WithProtectedProperty(grandchildEntity.GetProtectedProperty())
            .WithByteProperty(grandchildEntity.ByteProperty)
            .WithPropertyIntGetter(grandchildEntity.PropertyIntGetter)
            .Build();


        _ = typeof(GrandchildEntityBuilder).Should().HaveMethod(nameof(ChildEntityBuilder.WithProtectedProperty), [typeof(List<string>)]);
        _ = result.Should().BeEquivalentTo(grandchildEntity);
        _ = result.GetPrivateField().Should().BeEquivalentTo(grandchildEntity.GetPrivateField());
        _ = result.GetProtectedProperty().Should().BeEquivalentTo(grandchildEntity.GetProtectedProperty());
    }

    [Theory]
    [CustomAutoData]
    public void BuildersGenerator_CustomMethods_CustomMethodsAreCalled(
        GrandchildEntity grandchildEntity, string interfaceProperty)
    {
        var builder = EntityBuilderWithCustomMethods.GrandchildEntity;

        var result = builder
            .WithEntityInDifferentNamespace(grandchildEntity.EntityInDifferentNamespace)
            .WithPrivateField(grandchildEntity.GetPrivateField())
            .WithPropertyStringGetter(grandchildEntity.PropertyGetter)
            .WithByteProperty(grandchildEntity.ByteProperty)
            .WithPropertyIntGetter(grandchildEntity.PropertyIntGetter)
            .WithInterfaceType(mock => mock.Setup(x => x.Property).Returns(interfaceProperty))
            .Build();

        _ = typeof(EntityBuilderWithCustomMethods).Should().HaveMethod(nameof(ChildEntityBuilder.WithProtectedProperty), [typeof(List<string>)])
            .Which.Should().HaveAccessModifier(FluentAssertions.Common.CSharpAccessModifier.Private);
        _ = result.PropertyIntGetter.Should().Be(grandchildEntity.PropertyIntGetter / 2);
        _ = result.PropertyGetter.Should().Be(grandchildEntity.PropertyGetter + "custom");
        _ = result.InterfaceType.Property.Should().Be(interfaceProperty);
    }

    [Theory]
    [CustomAutoData]
    public void BuildersGenerator_NoMockingAndFakingAndInterfaceSetup_InterfacesShouldBeClean(
        GrandchildEntity grandchildEntity)
    {
        var builder = GrandchildEntityNoMoqBuilder.GrandchildEntity;

        var result = builder
            .WithEntityInDifferentNamespace(grandchildEntity.EntityInDifferentNamespace)
            .WithPrivateField(grandchildEntity.GetPrivateField())
            .WithPropertyStringGetter(grandchildEntity.PropertyGetter)
            .WithByteProperty(grandchildEntity.ByteProperty)
            .WithPropertyIntGetter(grandchildEntity.PropertyIntGetter)
            .WithInterfaceType(grandchildEntity.InterfaceType)
            .Build();

        _ = result.PropertyIntGetter.Should().Be(grandchildEntity.PropertyIntGetter);
        _ = result.PropertyGetter.Should().Be(grandchildEntity.PropertyGetter);
        _ = result.InterfaceType.Should().Be(grandchildEntity.InterfaceType);
    }

    [Fact]
    public void BuildersGenerator_NoMockingAndFaking_InterfacesShouldBeNull()
    {
        var builder = GrandchildEntityNoMoqBuilder.GrandchildEntity;

        var result = builder
            .Build();

        _ = result.InterfaceType.Should().BeNull();
        _ = result.PropertyGetter.Should().NotBeNull();
        _ = result.PropertyIntGetter.Should().NotBe(default);
        _ = result.EntityInDifferentNamespace.Should().NotBeNull();
        _ = result.ByteProperty.Should().NotBeNull();
        _ = result.GetPrivateField().Should().BeNull();
        _ = result.GetProtectedProperty().Should().NotBeNull();
    }

    [Fact]
    public void BuildersGenerator_FakingAndMocking_AllFieldsShouldBeDifferentFromDefault()
    {
        var builder = GrandchildEntityBuilder.GrandchildEntity;

        var result = builder.Build();

        _ = result.PropertyGetter.Should().NotBeNullOrEmpty();
        _ = result.PropertyIntGetter.Should().NotBe(default);
        _ = result.EntityInDifferentNamespace.Should().NotBeNull();
        _ = result.ByteProperty.Should().NotBeNullOrEmpty();
        _ = result.GetPrivateField().Should().NotBeNullOrEmpty();
        _ = result.GetProtectedProperty().Should().NotBeNullOrEmpty();
        _ = result.InterfaceType.Should().NotBeNull();
    }

    [Fact]
    public void BuildersGenerator_GenerateDefaultBuildMethod_ShouldCreateWithAllDefaultValues()
    {
        var result = GrandchildEntityBuilder.BuildDefault();

        _ = result.PropertyGetter.Should().BeNull();
        _ = result.PropertyIntGetter.Should().Be(default);
        _ = result.EntityInDifferentNamespace.Should().BeNull();
        _ = result.ByteProperty.Should().BeNull();
        _ = result.GetPrivateField().Should().BeNull();
        _ = result.GetProtectedProperty().Should().BeNull();
        _ = result.InterfaceType.Should().NotBeNull();
    }

    [Fact]
    public void BuildersGenerator_GenericClass_ShouldCreateGenericBuilder()
    {
        var result = GenericGrandchildEntityBuilder<int, EntityInDifferentNamespace>.BuildDefault();

        _ = result.PropertyGetter.Should().BeNull();
        _ = result.PropertyIntGetter.Should().Be(default);
        _ = result.EntityInDifferentNamespace.Should().BeNull();
        _ = result.ByteProperty.Should().BeNull();
        _ = result.GetPrivateField().Should().BeNull();
        _ = result.GetProtectedProperty().Should().BeNull();
    }

    [Fact]
    public void BuildersGenerator_BuildMany_ShouldCreateRandomValuesForEachObject()
    {
        var results = GrandchildEntityBuilder.GrandchildEntity.BuildMany(3).ToList();

        _ = results[0].Should().NotBeEquivalentTo(results[1]);
        _ = results[1].Should().NotBeEquivalentTo(results[2]);
        _ = results[0].Should().NotBeEquivalentTo(results[2]);
    }

    [Fact]
    public void BuildersGenerator_CustomBuildMany_ShouldUseCustomImplementation()
    {
        var results = CustomBuildManyEntityBuilder.Entity.BuildMany(3).ToList();

        _ = results.Should().HaveCount(3);
        _ = results[0].Id.Should().BeGreaterThan(0);
        _ = results[1].Id.Should().BeGreaterThan(results[0].Id);
        _ = results[2].Id.Should().BeGreaterThan(results[1].Id);
    }

    [Theory]
    [CustomAutoData]
    public void BuildersGenerator_ReadOnlyProperty_ShouldCreateMethodForSettingItsValue(int[] privateField)
    {
        var builder = ReadOnlyEntityWithConstructorBuilder.ReadOnlyEntityWithConstructor;
        _ = builder.WithPrivateField(privateField);
        _ = builder.Build().PrivateField.Should().BeEquivalentTo(privateField);
    }

    [Fact]
    public void BuildersGenerator_DefaultPublicConstructor_ShouldCreateBuildMethod()
    {
        _ = new EntityWithDefaultConstructorBuilder().Build().Should().NotBeNull();
    }


    [Fact]
    public void BuildersGenerator_PrivateConstructor_ShouldNotGenerateBuildMethod()
    {
        var lamda = () => new EntityWithPrivateConstructorBuilder().Build();
        lamda.Should().ThrowExactly<InvalidOperationException>().Which.Message.Should().Be("It is a test!");
    }

    [Fact]
    public void BuildersGenerator_PrivateConstructor_ShouldCreateWithProperties()
    {
        var builder = new EntityWithPrivateConstructorBuilder();
        _ = builder.WithPropertyIntGetter(1);
        _ = builder.WithPropertyGetter("1");
        _ = builder.WithEntityInDifferentNamespace(null!);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_InheritedPropertyWithPrivateSetter_ShouldBuildSuccessfully(string aProperty, int derivedProperty)
    {
        var builder = DerivedClassFromBaseWithPrivateSetterBuilder.DerivedClassFromBaseWithPrivateSetter;

        var result = builder
            .WithAProperty(aProperty)
            .WithDerivedProperty(derivedProperty)
            .Build();

        _ = result.AProperty.Should().Be(aProperty);
        _ = result.DerivedProperty.Should().Be(derivedProperty);
    }

    [Fact]
    public void BuildersGenerator_GetOnlyProperties_ShouldNotThrowExceptionWhenBuilding()
    {
        // Arrange
        var builder = DerivedClassFromBaseWithPrivateSetterBuilder.DerivedClassFromBaseWithPrivateSetter;

        // Act - This should not throw an exception even though there are get-only properties
        var result = builder.Build();

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.ABool.Should().BeTrue();
        _ = result.ReadOnlyString.Should().Be("readonly");
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_GetOnlyProperties_WithSettableProperty_ShouldSetOnlySettableProperties(int derivedProperty)
    {
        // Arrange
        var builder = DerivedClassFromBaseWithPrivateSetterBuilder.DerivedClassFromBaseWithPrivateSetter;

        // Act - Set the settable property, get-only properties should be ignored in Build
        var result = builder.WithDerivedProperty(derivedProperty).Build();

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.DerivedProperty.Should().Be(derivedProperty);
        _ = result.ABool.Should().BeTrue();
        _ = result.ReadOnlyString.Should().Be("readonly");
        _ = result.ComputedValue.Should().Be(derivedProperty * 2);
    }
}