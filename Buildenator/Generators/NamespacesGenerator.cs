using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Text;

namespace Buildenator.Generators
{
    internal static class NamespacesGenerator
    {
        internal static string GenerateNamespaces(params IAdditionalNamespacesProvider?[] additionalNamespacesProviders)
        {
            var list = new string[]
            {
                "System",
                "System.Linq",
                "Buildenator.Abstraction.Helpers"
            }.Concat(additionalNamespacesProviders.SelectMany(a => a?.AdditionalNamespaces ?? Array.Empty<string>()));

            list = list.Distinct();

            var output = new StringBuilder();
            foreach (var @namespace in list)
            {
                output.Append("using ").Append(@namespace).AppendLine(";");
            }
            return output.ToString();
        }
    }
}