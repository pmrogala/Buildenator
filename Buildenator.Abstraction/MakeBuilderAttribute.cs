using System;
// ReSharper disable UnusedParameter.Local

namespace Buildenator.Abstraction;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class MakeBuilderAttribute : Attribute
{
    /// <summary>
    /// Marking by this attribute will generate building methods in a separate partial class file
    /// </summary>
    /// <param name="typeForBuilder">What type of an object this builder is creating.</param>
    /// <param name="buildingMethodsPrefix">How the builder methods should be named.</param>
    /// <param name="generateDefaultBuildMethod">The resulting builder will have a DefaultBuild method with default parameters passed to the entity. true/false/null</param>
    /// <param name="nullableStrategy">Change nullable context behaviour. Use the <see cref="NullableStrategy"/> enum.</param>
    /// <param name="implicitCast">Should the builder have implicit cast to the target type.</param>
    /// <param name="generateMethodsForUnreachableProperties">It will create methods for setting up properties that does not have public setter.</param>
    /// <param name="staticFactoryMethodName">if you want to use a static factory method for constructing an entity, you can bring here the name.</param>
    /// <param name="generateStaticPropertyForBuilderCreation">If you want to generate static property that will return a new builder instance.</param>
    public MakeBuilderAttribute(
        Type typeForBuilder,
        string? buildingMethodsPrefix = "With",
        object? generateDefaultBuildMethod = null,
        object? nullableStrategy = null,
        object? generateMethodsForUnreachableProperties = null,
        object? implicitCast = null,
        string? staticFactoryMethodName = null,
        object? generateStaticPropertyForBuilderCreation = null
    )
    {
        }
}