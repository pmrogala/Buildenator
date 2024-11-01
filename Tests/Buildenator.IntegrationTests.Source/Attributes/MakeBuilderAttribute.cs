using System;

namespace Buildenator.IntegrationTests.Source.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class MakeBuilderAttribute : Attribute
    {
    }
}
