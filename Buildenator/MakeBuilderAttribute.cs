using System;

namespace Buildenator.Abstraction
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class MakeBuilderAttribute : Attribute
    {
        public MakeBuilderAttribute(Type typeForBuilder, string buildingMethodsPrefix = "With")
        {
            TypeForBuilder = typeForBuilder;
            BuildingMethodsPrefix = buildingMethodsPrefix;
        }

        public Type TypeForBuilder { get; }
        public string BuildingMethodsPrefix { get; }
    }
}
