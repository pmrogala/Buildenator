using Buildenator.Exceptions;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Buildenator.Extensions
{
    public static class AttributeContructorParametersExtensions
    {
        public static T GetOrThrow<T>(this in ImmutableArray<TypedConstant> attributeParameters, int index, string propertyName)
            => (T)(attributeParameters[index].Value ?? throw new ConfigurationException($"{propertyName} cannot be null."));

        public static string GetOrThrow(this in ImmutableArray<TypedConstant> attributeParameters, int index, string propertyName)
            => attributeParameters.GetOrThrow<string>(index, propertyName);
    }
}
