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
}
