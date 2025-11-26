using System;
// ReSharper disable UnusedParameter.Local

namespace Buildenator.Abstraction;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class BuildenatorConfigurationAttribute : Attribute
{
    /// <summary>
    /// An assembly attribute to configure globally the buildenator generation
    /// </summary>
    /// <param name="buildingMethodsPrefix">How the builder methods should be named.</param>
    /// <param name="generateDefaultBuildMethod">The resulting builder will have a special static building method with default parameters.</param>
    /// <param name="nullableStrategy">Change nullable context behaviour.</param>
    /// <param name="generateMethodsForUnreachableProperties"></param>
    /// <param name="implicitCast">Should the builder have implicit cast to the target type.</param>
    /// <param name="generateStaticPropertyForBuilderCreation">If you want to generate static property that will return a new builder instance.</param>
    /// <param name="initializeCollectionsWithEmpty">If true, collection fields will be initialized with empty collections in the constructor instead of null.</param>
    public BuildenatorConfigurationAttribute(
        string buildingMethodsPrefix = "With",
        bool generateDefaultBuildMethod = true,
        NullableStrategy nullableStrategy = NullableStrategy.Default,
        bool generateMethodsForUnreachableProperties = false,
        bool implicitCast = false,
        bool generateStaticPropertyForBuilderCreation = false,
        bool initializeCollectionsWithEmpty = false)
    {
        }
}