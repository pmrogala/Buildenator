using AutoFixture.Xunit2;
using Buildenator.IntegrationTests.SharedEntities;
using Buildenator.IntegrationTests.Source.Builders;
using Buildenator.IntegrationTests.SharedEntities.DifferentNamespace;
using FluentAssertions;
using Xunit;
using PostBuildEntityBuilder = Buildenator.IntegrationTests.Source.Builders.PostBuildEntityBuilder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Buildenator.IntegrationTests
{
    public class BuildersGeneratorTests
    {
        [Fact]
        public void BuildersGenerator_GeneratesPostBuildMethod()
        {
            var builder = PostBuildEntityBuilder.PostBuildEntity;

            var result = builder.Build();

            result.Entry.Should().Be(-1);
        }

        [Fact]
        public void BuildersGenerator_HasStaticBuilderFactory()
        {
            var builder = EntityBuilder.Entity;

            builder.Should().NotBeNull();
        }

        [Fact]
        public void BuildersGenerator_HasImplicitCast()
        {
            var builder = ImplicitCastBuilder.Entity;
            Entity result = builder;

            result.Should().NotBeNull();
        }

        [Fact]
        public void BuildersGenerator_BuilderNameNotReflectingTargetClassName_ShouldCompile()
        {
            var builder = DifferentNameBuilder.Entity;

            builder.Should().NotBeNull();
        }

        [Fact]
        public void BuildersGenerator_DefaultBuilderTrue_ShouldHaveDefaultBuilder()
        {
            typeof(DifferentNameBuilder).GetMethod("BuildDefault", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Should().NotBeNull();
        }

        [Fact]
        public void BuildersGenerator_DefaultBuilderFalse_ShouldNotHaveDefaultBuilder()
        {
            typeof(EntityBuilder).GetMethod("BuildDefault", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Should().BeNull();
        }

        [Theory]
        [AutoData]
        public void BuildersGenerator_OneConstructorWithoutSettableProperties_CreatedWithMethodsByConstructorParametersNames(int value, string str)
        {
            var builder = EntityBuilder.Entity;

            var entity = builder.WithPropertyIntGetter(value).WithPropertyStringGetter(str).Build();
            entity.PropertyIntGetter.Should().Be(value);
            entity.PropertyGetter.Should().Be(str);
        }

        [Theory]
        [AutoData]
        public void BuildersGenerator_DifferentPrefixSet_MethodsNamesChanged(int value, string str)
        {
            var builder = SetEntityBuilder.Entity;

            var entity = builder.SetPropertyIntGetter(value).SetPropertyStringGetter(str).Build();
            entity.PropertyIntGetter.Should().Be(value);
            entity.PropertyGetter.Should().Be(str);
        }

        [Theory]
        [AutoData]
        public void BuildersGenerator_ZeroConstructorsWithSettableProperties_CreatedWithMethodsBySettablePropertiesNames(int value, string str)
        {
            var builder = SettableEntityWithoutConstructorBuilder.SettableEntityWithoutConstructor;

            var entity = builder.WithPropertyIntGetter(value).WithPropertyGetter(str).Build();
            entity.PropertyIntGetter.Should().Be(value);
            entity.PropertyGetter.Should().Be(str);

        }

        [Theory]
        [AutoData]
        public void BuildersGenerator_OneConstructorWithSettableProperties_CreatedWithMethodsBySettablePropertiesNamesIfInBothPlaces(
            int value, string str, string[] strings, int[] arr)
        {
            var builder = SettableEntityWithConstructorBuilder.SettableEntityWithConstructor;

            var entity = builder.WithPropertyInt(value).WithProperty(str)
                .WithNoConstructorProperty(strings).WithPrivateField(arr).Build();
            entity.PropertyInt.Should().Be(value);
            entity.Property.Should().Be(str);
            entity.NoConstructorProperty.Should().BeSameAs(strings);
            entity.GetPrivateField().Should().BeSameAs(arr);
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

            result.Should().BeEquivalentTo(childEntity);
            result.GetPrivateField().GetEnumerator().Should().BeEquivalentTo(childEntity.GetPrivateField().GetEnumerator());
            result.GetProtectedProperty().Should().BeEquivalentTo(childEntity.GetProtectedProperty());
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


            typeof(GrandchildEntityBuilder).Should().HaveMethod(nameof(ChildEntityBuilder.WithProtectedProperty), new[] { typeof(List<string>) });
            result.Should().BeEquivalentTo(grandchildEntity);
            result.GetPrivateField().Should().BeEquivalentTo(grandchildEntity.GetPrivateField());
            result.GetProtectedProperty().Should().BeEquivalentTo(grandchildEntity.GetProtectedProperty());
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

            typeof(EntityBuilderWithCustomMethods).Should().HaveMethod(nameof(ChildEntityBuilder.WithProtectedProperty), new[] { typeof(List<string>) })
                .Which.Should().HaveAccessModifier(FluentAssertions.Common.CSharpAccessModifier.Private);
            result.PropertyIntGetter.Should().Be(grandchildEntity.PropertyIntGetter / 2);
            result.PropertyGetter.Should().Be(grandchildEntity.PropertyGetter + "custom");
            result.InterfaceType.Property.Should().Be(interfaceProperty);
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

            result.PropertyIntGetter.Should().Be(grandchildEntity.PropertyIntGetter);
            result.PropertyGetter.Should().Be(grandchildEntity.PropertyGetter);
            result.InterfaceType.Should().Be(grandchildEntity.InterfaceType);
        }

        [Fact]
        public void BuildersGenerator_NoMockingAndFaking_InterfacesShouldBeNull()
        {
            var builder = GrandchildEntityNoMoqBuilder.GrandchildEntity;

            var result = builder
                .Build();

            result.InterfaceType.Should().BeNull();
            result.PropertyGetter.Should().NotBeNull();
            result.PropertyIntGetter.Should().NotBe(default);
            result.EntityInDifferentNamespace.Should().NotBeNull();
            result.ByteProperty.Should().NotBeNull();
            result.GetPrivateField().Should().BeNull();
            result.GetProtectedProperty().Should().NotBeNull();
        }

        [Fact]
        public void BuildersGenerator_FakingAndMocking_AllFieldsShouldBeDifferentFromDefault()
        {
            var builder = GrandchildEntityBuilder.GrandchildEntity;

            var result = builder.Build();

            result.PropertyGetter.Should().NotBeNullOrEmpty();
            result.PropertyIntGetter.Should().NotBe(default);
            result.EntityInDifferentNamespace.Should().NotBeNull();
            result.ByteProperty.Should().NotBeNullOrEmpty();
            result.GetPrivateField().Should().NotBeNullOrEmpty();
            result.GetProtectedProperty().Should().NotBeNullOrEmpty();
            result.InterfaceType.Should().NotBeNull();
        }

        [Fact]
        public void BuildersGenerator_DefaultStaticCreator_ShouldCreateWithAllDefaultValues()
        {
            var result = GrandchildEntityBuilder.BuildDefault();

            result.PropertyGetter.Should().BeNull();
            result.PropertyIntGetter.Should().Be(default);
            result.EntityInDifferentNamespace.Should().BeNull();
            result.ByteProperty.Should().BeNull();
            result.GetPrivateField().Should().BeNull();
            result.GetProtectedProperty().Should().BeNull();
            result.InterfaceType.Should().BeNull();
        }

        [Fact]
        public void BuildersGenerator_GenericClass_ShouldCreateGenericBuilder()
        {
            var result = GenericGrandchildEntityBuilder<int, EntityInDifferentNamespace>.BuildDefault();

            result.PropertyGetter.Should().BeNull();
            result.PropertyIntGetter.Should().Be(default);
            result.EntityInDifferentNamespace.Should().BeNull();
            result.ByteProperty.Should().BeNull();
            result.GetPrivateField().Should().BeNull();
            result.GetProtectedProperty().Should().BeNull();
        }

        [Fact]
        public void BuildersGenerator_BuildMany_ShouldCreateRandomValuesForEachObject()
        {
            var results = GrandchildEntityBuilder.GrandchildEntity.BuildMany(3).ToList();

            results[0].Should().NotBeEquivalentTo(results[1]);
            results[1].Should().NotBeEquivalentTo(results[2]);
            results[0].Should().NotBeEquivalentTo(results[2]);
        }

        [Theory]
        [CustomAutoData]
        public void BuildersGenerator_ReadOnlyProperty_ShouldCreateMethodForSettingItsValue(int[] privateField)
        {
            var builder = ReadOnlyEntityWithConstructorBuilder.ReadOnlyEntityWithConstructor;
            builder.WithPrivateField(privateField);
            builder.Build().PrivateField.Should().BeEquivalentTo(privateField);
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
    }
}