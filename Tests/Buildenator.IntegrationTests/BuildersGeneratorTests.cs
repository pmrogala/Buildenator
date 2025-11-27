using AutoFixture.Xunit2;
using Buildenator.IntegrationTests.SharedEntities;
using Buildenator.IntegrationTests.Source.Builders;
using Buildenator.IntegrationTests.SharedEntities.DifferentNamespace;
using FluentAssertions;
using Xunit;
using PostBuildEntityBuilder = Buildenator.IntegrationTests.Source.Builders.PostBuildEntityBuilder;
using PreBuildEntityBuilder = Buildenator.IntegrationTests.Source.Builders.PreBuildEntityBuilder;
using Newtonsoft.Json.Linq;

namespace Buildenator.IntegrationTests;

public class BuildersGeneratorTests
{
    [Fact]
    public void BuildersGenerator_GeneratesPreBuildMethod()
    {
        var builder = PreBuildEntityBuilder.PreBuildEntity;

        // PreBuild should not be called in the constructor
        _ = builder.GetPreBuildCalledCount().Should().Be(0);

        // PreBuild should be called once when Build() is called
        _ = builder.Build();
        _ = builder.GetPreBuildCalledCount().Should().Be(1);

        // PreBuild should be called again on second Build()
        _ = builder.Build();
        _ = builder.GetPreBuildCalledCount().Should().Be(2);
    }

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
        var results = CustomBuildManyEntityBuilder.CustomBuildManyEntity.BuildMany(3).ToList();

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

    [Theory]
    [AutoData]
    public void BuildersGenerator_CollectionProperty_AddToMethodShouldAddMultipleItems(string item1, string item2, string item3)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;

        // Act
        var result = builder
            .AddToEnumerableItems(item1, item2, item3)
            .Build();

        // Assert
        _ = result.EnumerableItems.Should().HaveCount(3);
        _ = result.EnumerableItems.Should().ContainInOrder(item1, item2, item3);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_CollectionProperty_AddToMethodCanBeCombinedWithOtherMethods(string item, int id, string name)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;

        // Act
        var result = builder
            .WithId(id)
            .WithName(name)
            .AddToEnumerableItems(item)
            .Build();

        // Assert
        _ = result.Id.Should().Be(id);
        _ = result.Name.Should().Be(name);
        _ = result.EnumerableItems.Should().ContainSingle().Which.Should().Be(item);
    }

    [Fact]
    public void BuildersGenerator_CollectionProperty_BuildingWithoutAddingItems_ShouldUseWithValue()
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        var emptyList = new List<string>();

        // Act - Build without using AddTo, but use With to set empty collection
        var result = builder
            .WithId(1)
            .WithName("Test")
            .WithEnumerableItems(emptyList)
            .Build();

        // Assert - Collection should be empty (same values as what we provided)
        _ = result.EnumerableItems.Should().BeEmpty();
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_CollectionConstructorParameter_AddToMethodShouldPopulateConstructorParameter(string item1, string item2)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;

        // Act
        var result = builder
            .AddToEnumerableConstructorItems(item1, item2)
            .Build();

        // Assert
        _ = result.EnumerableConstructorItems.Should().HaveCount(2);
        _ = result.EnumerableConstructorItems.Should().ContainInOrder(item1, item2);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_IReadOnlyListProperty_AddToMethodShouldAddItems(int item1, int item2, int item3)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;

        // Act
        var result = builder
            .AddToReadOnlyListItems(item1, item2, item3)
            .Build();

        // Assert
        _ = result.ReadOnlyListItems.Should().HaveCount(3);
        _ = result.ReadOnlyListItems.Should().ContainInOrder(item1, item2, item3);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_ICollectionProperty_AddToMethodShouldAddItems(double item1, double item2)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;

        // Act
        var result = builder
            .AddToCollectionItems(item1, item2)
            .Build();

        // Assert
        _ = result.CollectionItems.Should().HaveCount(2);
        _ = result.CollectionItems.Should().Contain(new[] { item1, item2 });
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_IListProperty_AddToMethodShouldAddItems(bool item1, bool item2, bool item3, bool item4)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;

        // Act
        var result = builder
            .AddToListItems(item1, item2, item3, item4)
            .Build();

        // Assert
        _ = result.ListItems.Should().HaveCount(4);
        _ = result.ListItems.Should().ContainInOrder(item1, item2, item3, item4);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_MultipleCollectionTypes_AddToMethodsShouldWorkTogether(
        string strItem1, string strItem2,
        int intItem1, int intItem2,
        double doubleItem,
        bool boolItem)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;

        // Act
        var result = builder
            .AddToEnumerableItems(strItem1, strItem2)
            .AddToReadOnlyListItems(intItem1, intItem2)
            .AddToCollectionItems(doubleItem)
            .AddToListItems(boolItem)
            .Build();

        // Assert
        _ = result.EnumerableItems.Should().HaveCount(2);
        _ = result.EnumerableItems.Should().ContainInOrder(strItem1, strItem2);
        
        _ = result.ReadOnlyListItems.Should().HaveCount(2);
        _ = result.ReadOnlyListItems.Should().ContainInOrder(intItem1, intItem2);
        
        _ = result.CollectionItems.Should().ContainSingle().Which.Should().Be(doubleItem);
        
        _ = result.ListItems.Should().ContainSingle().Which.Should().Be(boolItem);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_CollectionWithThenAddTo_ShouldReplaceAndThenAdd(string item1, string item2, string item3)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        var initialCollection = new[] { item1 };
        
        // Act - With should replace, AddTo should append
        var result = builder
            .WithEnumerableItems(initialCollection)
            .AddToEnumerableItems(item2, item3)
            .Build();
        
        // Assert - Should have all three items
        _ = result.EnumerableItems.Should().HaveCount(3);
        _ = result.EnumerableItems.Should().ContainInOrder(item1, item2, item3);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_CollectionAddToThenWith_WithShouldReplaceEverything(string item1, string item2, string item3)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        var replacementCollection = new[] { item3 };
        
        // Act - AddTo first, then With should completely replace
        var result = builder
            .AddToEnumerableItems(item1, item2)
            .WithEnumerableItems(replacementCollection)
            .Build();
        
        // Assert - Should only have item3, With replaces everything
        _ = result.EnumerableItems.Should().ContainSingle().Which.Should().Be(item3);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_CollectionWithArrayThenAddTo_ShouldWorkWithDifferentCollectionTypes(string item1, string item2, string item3)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        var arrayCollection = new[] { item1 };
        
        // Act - With accepts array, AddTo should still work
        var result = builder
            .WithEnumerableItems(arrayCollection)
            .AddToEnumerableItems(item2, item3)
            .Build();
        
        // Assert
        _ = result.EnumerableItems.Should().HaveCount(3);
        _ = result.EnumerableItems.Should().ContainInOrder(item1, item2, item3);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_CollectionWithListThenAddTo_ShouldWorkWithList(string item1, string item2)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        var listCollection = new List<string> { item1 };
        
        // Act - With accepts List, AddTo should still work
        var result = builder
            .WithEnumerableItems(listCollection)
            .AddToEnumerableItems(item2)
            .Build();
        
        // Assert
        _ = result.EnumerableItems.Should().HaveCount(2);
        _ = result.EnumerableItems.Should().ContainInOrder(item1, item2);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_CollectionMultipleWithAndAddTo_LastOperationsWin(string item1, string item2, string item3, string item4)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        
        // Act - Complex scenario with multiple With and AddTo calls
        var result = builder
            .WithEnumerableItems(new[] { item1 })
            .AddToEnumerableItems(item2)
            .WithEnumerableItems(new[] { item3 }) // This replaces everything
            .AddToEnumerableItems(item4) // This adds to the replaced collection
            .Build();
        
        // Assert - Should only have item3 and item4
        _ = result.EnumerableItems.Should().HaveCount(2);
        _ = result.EnumerableItems.Should().ContainInOrder(item3, item4);
    }
    
    [Fact]
    public void BuildersGenerator_CollectionWithNullThenAddTo_ShouldStartFresh()
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        
        // Act - With(null) should clear, then AddTo should add
        var result = builder
            .WithEnumerableItems(null)
            .AddToEnumerableItems("item1", "item2")
            .Build();
        
        // Assert
        _ = result.EnumerableItems.Should().HaveCount(2);
        _ = result.EnumerableItems.Should().ContainInOrder("item1", "item2");
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_ReadOnlyListWithThenAddTo_ShouldWorkWithReadOnlyList(int item1, int item2, int item3)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        var readOnlyList = new List<int> { item1 }.AsReadOnly();
        
        // Act - With accepts IReadOnlyList, AddTo should still work
        var result = builder
            .WithReadOnlyListItems(readOnlyList)
            .AddToReadOnlyListItems(item2, item3)
            .Build();
        
        // Assert
        _ = result.ReadOnlyListItems.Should().HaveCount(3);
        _ = result.ReadOnlyListItems.Should().ContainInOrder(item1, item2, item3);
    }
    
    // ===== Tests for Concrete Collection Types (List<T>, HashSet<T>, etc.) =====
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_ConcreteListProperty_AddToMethodShouldAddItems(string item1, string item2, string item3)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        
        // Act
        var result = builder
            .AddToConcreteListItems(item1, item2, item3)
            .Build();
        
        // Assert
        _ = result.ConcreteListItems.Should().HaveCount(3);
        _ = result.ConcreteListItems.Should().ContainInOrder(item1, item2, item3);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_ConcreteHashSetProperty_AddToMethodShouldAddItems(int item1, int item2, int item3)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        
        // Act
        var result = builder
            .AddToConcreteHashSetItems(item1, item2, item3)
            .Build();
        
        // Assert
        _ = result.ConcreteHashSetItems.Should().HaveCount(3);
        _ = result.ConcreteHashSetItems.Should().Contain(new[] { item1, item2, item3 });
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_ConcreteListProperty_WithThenAddTo_ShouldAppendItems(string item1, string item2, string item3)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        
        // Act - With sets initial value, AddTo appends
        var result = builder
            .WithConcreteListItems(new List<string> { item1 })
            .AddToConcreteListItems(item2, item3)
            .Build();
        
        // Assert
        _ = result.ConcreteListItems.Should().HaveCount(3);
        _ = result.ConcreteListItems.Should().ContainInOrder(item1, item2, item3);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_ConcreteListProperty_AddToThenWith_ShouldReplaceCompletely(string item1, string item2, string item3)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        
        // Act - AddTo first, then With replaces everything
        var result = builder
            .AddToConcreteListItems(item1, item2)
            .WithConcreteListItems(new List<string> { item3 })
            .Build();
        
        // Assert - Should only have item3
        _ = result.ConcreteListItems.Should().ContainSingle().Which.Should().Be(item3);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_ConcreteListConstructorParameter_AddToMethodShouldPopulateConstructorParameter(char item1, char item2)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        
        // Act
        var result = builder
            .AddToConcreteListConstructorItems(item1, item2)
            .Build();
        
        // Assert
        _ = result.ConcreteListConstructorItems.Should().HaveCount(2);
        _ = result.ConcreteListConstructorItems.Should().ContainInOrder(item1, item2);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_ConcreteListProperty_WithNullThenAddTo_ShouldStartFresh(string item1, string item2)
    {
        // Arrange
        var builder = EntityWithCollectionAndAddMethodBuilder.EntityWithCollectionAndAddMethod;
        
        // Act - With(null) clears, AddTo starts fresh
        var result = builder
            .WithConcreteListItems(null)
            .AddToConcreteListItems(item1, item2)
            .Build();
        
        // Assert - Should only have the AddTo items
        _ = result.ConcreteListItems.Should().HaveCount(2);
        _ = result.ConcreteListItems.Should().ContainInOrder(item1, item2);
    }

    // ===== Tests for Dictionary Types =====
    // These tests verify that dictionary types are handled correctly as collections with key-value pairs
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_DictionaryProperty_WithMethodShouldSetDictionary(string key1, int value1, string key2, int value2)
    {
        // Arrange
        var builder = EntityWithDictionaryBuilder.EntityWithDictionary;
        var scores = new Dictionary<string, int> { { key1, value1 }, { key2, value2 } };
        
        // Act
        var result = builder
            .WithScores(scores)
            .Build();
        
        // Assert
        _ = result.Scores.Should().NotBeNull();
        _ = result.Scores.Should().HaveCount(2);
        _ = result.Scores[key1].Should().Be(value1);
        _ = result.Scores[key2].Should().Be(value2);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_IDictionaryConstructorParameter_WithMethodShouldPassThroughCorrectly(string key, string value)
    {
        // Arrange
        var builder = EntityWithDictionaryBuilder.EntityWithDictionary;
        var metadata = new Dictionary<string, string> { { key, value } };
        
        // Act - IDictionary constructor parameter should be passed correctly
        var result = builder
            .WithMetadata(metadata)
            .Build();
        
        // Assert - The IReadOnlyDictionary Metadata property should have the value
        _ = result.Metadata.Should().NotBeNull();
        _ = result.Metadata.Should().HaveCount(1);
        _ = result.Metadata[key].Should().Be(value);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_IReadOnlyDictionaryProperty_WithMethodShouldSetValue(string key, object value)
    {
        // Arrange
        var builder = EntityWithDictionaryBuilder.EntityWithDictionary;
        var settings = new Dictionary<string, object> { { key, value } };
        
        // Act
        var result = builder
            .WithSettings(settings)
            .Build();
        
        // Assert
        _ = result.Settings.Should().NotBeNull();
        _ = result.Settings.Should().HaveCount(1);
        _ = result.Settings[key].Should().Be(value);
    }
    
    [Fact]
    public void BuildersGenerator_DictionaryProperty_BuildDefaultMethodShouldHaveCorrectParameterTypes()
    {
        // Arrange & Act - Verify BuildDefault has correct parameter types (not List<KeyValuePair<...>>)
        var method = typeof(EntityWithDictionaryBuilder).GetMethod("BuildDefault", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        
        // Assert
        _ = method.Should().NotBeNull();
        var parameters = method.GetParameters();
        
        // Check that IDictionary parameters are correct
        var metadataParam = parameters.FirstOrDefault(p => p.Name == "_metadata");
        _ = metadataParam.Should().NotBeNull();
        _ = metadataParam.ParameterType.Should().BeAssignableTo<IDictionary<string, string>>();
        
        var itemsParam = parameters.FirstOrDefault(p => p.Name == "_items");
        _ = itemsParam.Should().NotBeNull();
        _ = itemsParam.ParameterType.Should().BeAssignableTo<IDictionary<int, string>>();
        
        // Check that concrete Dictionary parameter is correct
        var scoresParam = parameters.FirstOrDefault(p => p.Name == "_scores");
        _ = scoresParam.Should().NotBeNull();
        _ = scoresParam.ParameterType.Should().Be(typeof(Dictionary<string, int>));
        
        // Check that IReadOnlyDictionary parameter is correct
        var settingsParam = parameters.FirstOrDefault(p => p.Name == "_settings");
        _ = settingsParam.Should().NotBeNull();
        _ = settingsParam.ParameterType.Should().BeAssignableTo<IReadOnlyDictionary<string, object>>();
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_DictionaryProperty_BuildDefaultShouldCreateEntityWithDictionaryValues(
        string metaKey, string metaValue, int itemKey, string itemValue)
    {
        // Arrange
        var metadata = new Dictionary<string, string> { { metaKey, metaValue } };
        var items = new Dictionary<int, string> { { itemKey, itemValue } };
        
        // Act - Use BuildDefault with dictionary parameters
        var result = EntityWithDictionaryBuilder.BuildDefault(
            _metadata: metadata,
            _items: items);
        
        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Metadata.Should().NotBeNull();
        _ = result.Metadata[metaKey].Should().Be(metaValue);
        _ = result.Items.Should().NotBeNull();
        _ = result.Items[itemKey].Should().Be(itemValue);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_MultipleDictionaryProperties_ShouldAllBeSetCorrectly(
        string metaKey, string metaValue, 
        int itemKey, string itemValue, 
        string scoreKey, int scoreValue,
        string settingKey, object settingValue,
        string name)
    {
        // Arrange
        var builder = EntityWithDictionaryBuilder.EntityWithDictionary;
        var metadata = new Dictionary<string, string> { { metaKey, metaValue } };
        var items = new Dictionary<int, string> { { itemKey, itemValue } };
        var scores = new Dictionary<string, int> { { scoreKey, scoreValue } };
        var settings = new Dictionary<string, object> { { settingKey, settingValue } };
        
        // Act
        var result = builder
            .WithMetadata(metadata)
            .WithItems(items)
            .WithScores(scores)
            .WithSettings(settings)
            .WithName(name)
            .Build();
        
        // Assert
        _ = result.Metadata.Should().NotBeNull();
        _ = result.Metadata[metaKey].Should().Be(metaValue);
        
        _ = result.Items.Should().NotBeNull();
        _ = result.Items[itemKey].Should().Be(itemValue);
        
        _ = result.Scores.Should().NotBeNull();
        _ = result.Scores[scoreKey].Should().Be(scoreValue);
        
        _ = result.Settings.Should().NotBeNull();
        _ = result.Settings[settingKey].Should().Be(settingValue);
        
        _ = result.Name.Should().Be(name);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_DictionaryProperty_AddToMethodShouldAddKeyValuePairs(string key1, int value1, string key2, int value2)
    {
        // Arrange
        var builder = EntityWithDictionaryBuilder.EntityWithDictionary;
        
        // Act - Use AddTo method with KeyValuePair
        var result = builder
            .AddToScores(new KeyValuePair<string, int>(key1, value1), new KeyValuePair<string, int>(key2, value2))
            .Build();
        
        // Assert
        _ = result.Scores.Should().NotBeNull();
        _ = result.Scores.Should().HaveCount(2);
        _ = result.Scores[key1].Should().Be(value1);
        _ = result.Scores[key2].Should().Be(value2);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_IDictionaryProperty_AddToMethodShouldAddKeyValuePairs(string key1, string value1, string key2, string value2)
    {
        // Arrange
        var builder = EntityWithDictionaryBuilder.EntityWithDictionary;
        
        // Act - Use AddTo method on IDictionary property
        var result = builder
            .AddToMetadata(new KeyValuePair<string, string>(key1, value1), new KeyValuePair<string, string>(key2, value2))
            .Build();
        
        // Assert - The IReadOnlyDictionary Metadata property should have the values
        _ = result.Metadata.Should().NotBeNull();
        _ = result.Metadata.Should().HaveCount(2);
        _ = result.Metadata[key1].Should().Be(value1);
        _ = result.Metadata[key2].Should().Be(value2);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_IReadOnlyDictionaryProperty_AddToMethodShouldAddKeyValuePairs(string key, object value)
    {
        // Arrange
        var builder = EntityWithDictionaryBuilder.EntityWithDictionary;
        
        // Act - Use AddTo method on IReadOnlyDictionary property
        var result = builder
            .AddToSettings(new KeyValuePair<string, object>(key, value))
            .Build();
        
        // Assert
        _ = result.Settings.Should().NotBeNull();
        _ = result.Settings.Should().ContainSingle();
        _ = result.Settings[key].Should().Be(value);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_DictionaryProperty_WithThenAddToShouldAppendItems(string key1, int value1, string key2, int value2)
    {
        // Arrange
        var builder = EntityWithDictionaryBuilder.EntityWithDictionary;
        var initialScores = new Dictionary<string, int> { { key1, value1 } };
        
        // Act - With sets initial value, AddTo appends
        var result = builder
            .WithScores(initialScores)
            .AddToScores(new KeyValuePair<string, int>(key2, value2))
            .Build();
        
        // Assert
        _ = result.Scores.Should().NotBeNull();
        _ = result.Scores.Should().HaveCount(2);
        _ = result.Scores[key1].Should().Be(value1);
        _ = result.Scores[key2].Should().Be(value2);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_DictionaryProperty_AddToThenWithShouldReplaceCompletely(string key1, int value1, string key2, int value2)
    {
        // Arrange
        var builder = EntityWithDictionaryBuilder.EntityWithDictionary;
        var replacementScores = new Dictionary<string, int> { { key2, value2 } };
        
        // Act - AddTo first, then With replaces everything
        var result = builder
            .AddToScores(new KeyValuePair<string, int>(key1, value1))
            .WithScores(replacementScores)
            .Build();
        
        // Assert - Should only have key2
        _ = result.Scores.Should().ContainSingle();
        _ = result.Scores[key2].Should().Be(value2);
    }
    
    [Fact]
    public void BuildersGenerator_DictionaryProperty_AddToMethodsShouldExist()
    {
        // Dictionary types SHOULD have AddTo methods generated
        
        // Assert - Verify AddTo methods exist for dictionary properties
        var methods = typeof(EntityWithDictionaryBuilder).GetMethods();
        
        var addToScores = methods.FirstOrDefault(m => m.Name == "AddToScores");
        _ = addToScores.Should().NotBeNull("Dictionary properties should have AddTo methods generated");
        
        var addToMetadata = methods.FirstOrDefault(m => m.Name == "AddToMetadata");
        _ = addToMetadata.Should().NotBeNull("IDictionary properties should have AddTo methods generated");
        
        var addToItems = methods.FirstOrDefault(m => m.Name == "AddToItems");
        _ = addToItems.Should().NotBeNull("IDictionary properties should have AddTo methods generated");
        
        var addToSettings = methods.FirstOrDefault(m => m.Name == "AddToSettings");
        _ = addToSettings.Should().NotBeNull("IReadOnlyDictionary properties should have AddTo methods generated");
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_OverloadedMethods_ShouldNotBreakGeneration(int aValue)
    {
        // Arrange - Using builder with overloaded WithValue methods
        var builder = EntityWithOverloadedMethodsBuilder.Default(aValue);
        
        // Act - Build should work without throwing
        var result = builder.Build();
        
        // Assert - The property should have been set via the constructor
        _ = result.AValue.Should().Be(aValue);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_OverloadedMethods_CanUseIntOverload(int originalValue, int newValue)
    {
        // Arrange
        var builder = EntityWithOverloadedMethodsBuilder.Default(originalValue);
        
        // Act - Use the overloaded method with int parameter
        var builderReturned = builder.WithValue(newValue);
        
        // Assert - The fluent pattern should work
        _ = builderReturned.Should().Be(builder);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_OverloadedMethods_CanUseStringOverload(int originalValue, string stringValue)
    {
        // Arrange
        var builder = EntityWithOverloadedMethodsBuilder.Default(originalValue);
        
        // Act - Use the overloaded method with string parameter
        var builderReturned = builder.WithValue(stringValue);
        
        // Assert - The fluent pattern should work
        _ = builderReturned.Should().Be(builder);
    }

    // ===== Tests for initializeCollectionsWithEmpty feature =====
    
    [Fact]
    public void BuildersGenerator_InitializeCollectionsWithEmpty_CollectionsShouldBeEmptyNotNull()
    {
        // Arrange - Build without setting any collection values
        var builder = EntityWithCollectionsForEmptyInitBuilder.EntityWithCollectionsForEmptyInit;
        
        // Act
        var result = builder.Build();
        
        // Assert - All collections should be empty, not null
        _ = result.EnumerableItems.Should().NotBeNull().And.BeEmpty();
        _ = result.ConcreteListItems.Should().NotBeNull().And.BeEmpty();
        _ = result.DictionaryItems.Should().NotBeNull().And.BeEmpty();
        _ = result.ReadOnlyListProperty.Should().NotBeNull().And.BeEmpty();
        _ = result.HashSetProperty.Should().NotBeNull().And.BeEmpty();
        _ = result.ReadOnlyDictProperty.Should().NotBeNull().And.BeEmpty();
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_InitializeCollectionsWithEmpty_CanAddToCollections(string item1, string item2)
    {
        // Arrange
        var builder = EntityWithCollectionsForEmptyInitBuilder.EntityWithCollectionsForEmptyInit;
        
        // Act - Use AddTo methods to add items
        var result = builder
            .AddToEnumerableItems(item1, item2)
            .Build();
        
        // Assert - The AddTo method should have added items to the collection
        _ = result.EnumerableItems.Should().HaveCount(2);
        _ = result.EnumerableItems.Should().ContainInOrder(item1, item2);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_InitializeCollectionsWithEmpty_WithMethodReplacesEmptyCollection(string item1, string item2, string item3)
    {
        // Arrange
        var builder = EntityWithCollectionsForEmptyInitBuilder.EntityWithCollectionsForEmptyInit;
        var items = new List<string> { item1, item2, item3 };
        
        // Act - With method should replace the empty collection
        var result = builder
            .WithEnumerableItems(items)
            .Build();
        
        // Assert
        _ = result.EnumerableItems.Should().HaveCount(3);
        _ = result.EnumerableItems.Should().ContainInOrder(item1, item2, item3);
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_InitializeCollectionsWithEmpty_NonCollectionPropertiesStillWorkNormally(string name, int value)
    {
        // Arrange
        var builder = EntityWithCollectionsForEmptyInitBuilder.EntityWithCollectionsForEmptyInit;
        
        // Act
        var result = builder
            .WithName(name)
            .WithValue(value)
            .Build();
        
        // Assert - Non-collection properties should work as before
        _ = result.Name.Should().Be(name);
        _ = result.Value.Should().Be(value);
        
        // Collections should still be empty
        _ = result.EnumerableItems.Should().BeEmpty();
    }
    
    [Theory]
    [AutoData]
    public void BuildersGenerator_InitializeCollectionsWithEmpty_DictionariesWorkCorrectly(string key, string value)
    {
        // Arrange
        var builder = EntityWithCollectionsForEmptyInitBuilder.EntityWithCollectionsForEmptyInit;
        
        // Act - Add to dictionary
        var result = builder
            .AddToDictionaryItems(new KeyValuePair<string, string>(key, value))
            .Build();
        
        // Assert
        _ = result.DictionaryItems.Should().HaveCount(1);
        _ = result.DictionaryItems[key].Should().Be(value);
    }

    // ===== Tests for Default Field Initialization =====
    // These tests verify that user-defined Default{PropertyName} values are used as field initializers

    [Fact]
    public void BuildersGenerator_DefaultFieldValue_ShouldUseConstDefaultForName()
    {
        // Arrange - Use builder with DefaultName constant defined
        var builder = EntityWithDefaultValueBuilder.EntityWithDefaultValue;
        
        // Act - Build without setting Name, should use the DefaultName constant
        var result = builder.Build();
        
        // Assert - Name should be the DefaultName value
        _ = result.Name.Should().Be(EntityWithDefaultValueBuilder.DefaultName);
    }

    [Fact]
    public void BuildersGenerator_DefaultFieldValue_ShouldUseConstDefaultForCount()
    {
        // Arrange - Use builder with DefaultCount constant defined
        var builder = EntityWithDefaultValueBuilder.EntityWithDefaultValue;
        
        // Act - Build without setting Count, should use the DefaultCount constant
        var result = builder.Build();
        
        // Assert - Count should be the DefaultCount value
        _ = result.Count.Should().Be(EntityWithDefaultValueBuilder.DefaultCount);
    }

    [Fact]
    public void BuildersGenerator_DefaultFieldValue_ShouldUseStaticReadonlyDefault()
    {
        // Arrange - Use builder with DefaultOptionalValue static readonly field defined
        var builder = EntityWithDefaultValueBuilder.EntityWithDefaultValue;
        
        // Act - Build without setting OptionalValue, should use the DefaultOptionalValue field
        var result = builder.Build();
        
        // Assert - OptionalValue should be the DefaultOptionalValue value
        _ = result.OptionalValue.Should().Be(EntityWithDefaultValueBuilder.DefaultOptionalValue);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_DefaultFieldValue_WithMethodShouldOverrideDefault(string customName)
    {
        // Arrange - Use builder with default values
        var builder = EntityWithDefaultValueBuilder.EntityWithDefaultValue;
        
        // Act - Use WithName to override the default
        var result = builder.WithName(customName).Build();
        
        // Assert - Name should be the custom value, not the default
        _ = result.Name.Should().Be(customName);
        _ = result.Count.Should().Be(EntityWithDefaultValueBuilder.DefaultCount); // Other defaults still apply
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_DefaultFieldValue_AllPropertiesCanBeOverridden(string name, int count, string optionalValue)
    {
        // Arrange - Use builder with default values
        var builder = EntityWithDefaultValueBuilder.EntityWithDefaultValue;
        
        // Act - Override all properties
        var result = builder
            .WithName(name)
            .WithCount(count)
            .WithOptionalValue(optionalValue)
            .Build();
        
        // Assert - All values should be the custom values
        _ = result.Name.Should().Be(name);
        _ = result.Count.Should().Be(count);
        _ = result.OptionalValue.Should().Be(optionalValue);
    }

    [Fact]
    public void BuildersGenerator_DefaultFieldValue_BuildManyShouldUseDefaultsForEachInstance()
    {
        // Arrange - Use builder with default values
        var builder = EntityWithDefaultValueBuilder.EntityWithDefaultValue;
        
        // Act - Build multiple instances
        var results = builder.BuildMany(3).ToList();
        
        // Assert - All instances should have the default values
        results.Should().OnlyContain(r => r.Name == EntityWithDefaultValueBuilder.DefaultName);
        results.Should().OnlyContain(r => r.Count == EntityWithDefaultValueBuilder.DefaultCount);
        results.Should().OnlyContain(r => r.OptionalValue == EntityWithDefaultValueBuilder.DefaultOptionalValue);
    }

    // ===== Tests for UseChildBuilders feature =====
    // These tests verify that child builder methods are generated when useChildBuilders is enabled

    [Theory]
    [AutoData]
    public void BuildersGenerator_UseChildBuilders_ShouldGenerateChildBuilderMethod(string childName, int childValue, int parentValue)
    {
        // Arrange - ParentWithChildEntityBuilder has useChildBuilders: true
        var builder = ParentWithChildEntityBuilder.ParentWithChildEntity;

        // Act - Use the generated child builder method with Func<ChildBuilder, ChildBuilder>
        var result = builder
            .WithChild(child => child
                .WithName(childName)
                .WithValue(childValue))
            .WithParentValue(parentValue)
            .Build();

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Child.Should().NotBeNull();
        _ = result.Child.Name.Should().Be(childName);
        _ = result.Child.Value.Should().Be(childValue);
        _ = result.ParentValue.Should().Be(parentValue);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_UseChildBuilders_DirectWithMethodStillWorks(string childName, int childValue, int parentValue)
    {
        // Arrange - Create a child entity directly
        var childEntity = new ChildForParentEntity(childName, childValue);
        var builder = ParentWithChildEntityBuilder.ParentWithChildEntity;

        // Act - Use the direct With method (not the child builder method)
        var result = builder
            .WithChild(childEntity)
            .WithParentValue(parentValue)
            .Build();

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Child.Should().Be(childEntity);
        _ = result.ParentValue.Should().Be(parentValue);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_UseChildBuilders_SettablePropertyShouldAlsoHaveChildBuilderMethod(string childName, int childValue, int parentValue)
    {
        // Arrange - ParentWithChildEntity has an OptionalChild settable property
        var builder = ParentWithChildEntityBuilder.ParentWithChildEntity;
        var childEntity = new ChildForParentEntity("constructor-child", 42);

        // Act - Use the generated child builder method for the settable property
        var result = builder
            .WithChild(childEntity)
            .WithParentValue(parentValue)
            .WithOptionalChild(child => child
                .WithName(childName)
                .WithValue(childValue))
            .Build();

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.OptionalChild.Should().NotBeNull();
        _ = result.OptionalChild.Name.Should().Be(childName);
        _ = result.OptionalChild.Value.Should().Be(childValue);
    }

    // ===== Tests for UseChildBuilders with Collections =====
    // These tests verify that child builder methods are generated for collections when useChildBuilders is enabled

    [Theory]
    [AutoData]
    public void BuildersGenerator_UseChildBuildersWithCollection_AddToMethodShouldAcceptFuncParameter(
        string childName1, int childValue1, string childName2, int childValue2, int parentValue)
    {
        // Arrange - ParentWithChildCollectionEntityBuilder has useChildBuilders: true
        var builder = ParentWithChildCollectionEntityBuilder.ParentWithChildCollectionEntity;

        // Act - Use the generated AddTo method with Func<ChildBuilder, ChildBuilder> parameters
        var result = builder
            .AddToChildren(
                child => child.WithName(childName1).WithValue(childValue1),
                child => child.WithName(childName2).WithValue(childValue2))
            .WithParentValue(parentValue)
            .Build();

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Children.Should().HaveCount(2);
        _ = result.Children[0].Name.Should().Be(childName1);
        _ = result.Children[0].Value.Should().Be(childValue1);
        _ = result.Children[1].Name.Should().Be(childName2);
        _ = result.Children[1].Value.Should().Be(childValue2);
        _ = result.ParentValue.Should().Be(parentValue);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_UseChildBuildersWithCollection_SettablePropertyAddToShouldAcceptFuncParameter(
        string childName1, int childValue1, string childName2, int childValue2, int parentValue)
    {
        // Arrange
        var builder = ParentWithChildCollectionEntityBuilder.ParentWithChildCollectionEntity;

        // Act - Use the AddTo method for the settable OptionalChildren property
        var result = builder
            .AddToChildren(child => child.WithName("constructor-child").WithValue(0))
            .WithParentValue(parentValue)
            .AddToOptionalChildren(
                child => child.WithName(childName1).WithValue(childValue1),
                child => child.WithName(childName2).WithValue(childValue2))
            .Build();

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.OptionalChildren.Should().HaveCount(2);
        _ = result.OptionalChildren[0].Name.Should().Be(childName1);
        _ = result.OptionalChildren[0].Value.Should().Be(childValue1);
        _ = result.OptionalChildren[1].Name.Should().Be(childName2);
        _ = result.OptionalChildren[1].Value.Should().Be(childValue2);
    }

    [Theory]
    [AutoData]
    public void BuildersGenerator_UseChildBuildersWithCollection_MultipleAddToCalls_ShouldAccumulate(
        string childName1, int childValue1, string childName2, int childValue2, int parentValue)
    {
        // Arrange
        var builder = ParentWithChildCollectionEntityBuilder.ParentWithChildCollectionEntity;

        // Act - Use AddTo multiple times - items should accumulate
        var result = builder
            .AddToChildren(child => child.WithName(childName1).WithValue(childValue1))
            .AddToChildren(child => child.WithName(childName2).WithValue(childValue2))
            .WithParentValue(parentValue)
            .Build();

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Children.Should().HaveCount(2);
        _ = result.Children[0].Name.Should().Be(childName1);
        _ = result.Children[1].Name.Should().Be(childName2);
    }

    [Fact]
    public void BuildersGenerator_UseChildBuildersWithCollection_ShouldNotHaveWithMethodForCollectionWithBuilders()
    {
        // The WithChildren(IEnumerable<ChildForParentEntity>) method should NOT be generated 
        // because we only want the Func<> version for collections with buildable elements
        var methods = typeof(ParentWithChildCollectionEntityBuilder).GetMethods();
        
        // Check that there is no WithChildren method that takes the raw collection type
        var withChildrenMethods = methods.Where(m => m.Name == "WithChildren").ToList();
        
        // Should only have the Func<> version, not the raw entity collection version
        // Note: We might have no WithChildren methods at all if it's only AddTo
        _ = withChildrenMethods.Should().NotContain(m => 
            m.GetParameters().Length == 1 && 
            m.GetParameters()[0].ParameterType.Name.Contains("IEnumerable") ||
            m.GetParameters()[0].ParameterType.Name.Contains("IReadOnlyList"));
    }
}
