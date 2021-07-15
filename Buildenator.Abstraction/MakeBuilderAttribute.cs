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
        public MakeBuilderAttribute(Type typeForBuilder, string buildingMethodsPrefix = "With")
        {
            TypeForBuilder = typeForBuilder;
            BuildingMethodsPrefix = buildingMethodsPrefix;
        }

        public Type TypeForBuilder { get; }
        public string BuildingMethodsPrefix { get; }
    }
}
