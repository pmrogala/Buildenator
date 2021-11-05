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
        /// <param name="defaultStaticCreator">The resulting builder will have a special static building method with default parameters. true/false/null</param>
        /// <param name="nullableStrategy">Change nullable context behaviour. Use the <see cref="NullableStrategy"/> enum.</param>
        public MakeBuilderAttribute(
            Type typeForBuilder,
            string? buildingMethodsPrefix = "With",
            object? defaultStaticCreator = null,
            object? nullableStrategy = null)
        {
        }
    }
}
