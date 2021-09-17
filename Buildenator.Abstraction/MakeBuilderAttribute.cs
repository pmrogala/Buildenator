using System;

namespace Buildenator.Abstraction
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class MakeBuilderAttribute : Attribute
    {
        /// <summary>
        /// Marking by this attribute will generate building methods in a separate partial class file
        /// </summary>
        /// <param name="typeForBuilder">What type of an object this builder is creating.</param>
        /// <param name="buildingMethodsPrefix">How the builder methods should be named.</param>
        /// <param name="defaultStaticCreator">The resulting builder will have a special static building method with default parameters.</param>
        public MakeBuilderAttribute(
            Type typeForBuilder,
            string buildingMethodsPrefix = "With",
            bool defaultStaticCreator = false)
        {
            TypeForBuilder = typeForBuilder;
            BuildingMethodsPrefix = buildingMethodsPrefix;
            StaticCreator = defaultStaticCreator;
        }

        public Type TypeForBuilder { get; }
        public string BuildingMethodsPrefix { get; }
        public bool StaticCreator { get; }
    }
}
