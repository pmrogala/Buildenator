using System;

namespace Buildenator.Abstraction
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class MakeBuilderAttribute : Attribute
    {
        public MakeBuilderAttribute(Type typeForBuilder)
        {
            TypeForBuilder = typeForBuilder;
        }

        public Type TypeForBuilder { get; }
    }
}
