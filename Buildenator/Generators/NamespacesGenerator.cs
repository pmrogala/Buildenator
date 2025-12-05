using System;
using System.Collections.Generic;
using System.Text;

namespace Buildenator.Generators;

internal static class NamespacesGenerator
{
    internal static string GenerateNamespaces(params IAdditionalNamespacesProvider?[] additionalNamespacesProviders)
    {
        var output = new StringBuilder();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        Add("System");
        Add("System.Linq");
        Add("Buildenator.Abstraction.Helpers");

        for (var i = 0; i < additionalNamespacesProviders.Length; i++)
        {
            var provider = additionalNamespacesProviders[i];
            var namespaces = provider?.AdditionalNamespaces;
            if (namespaces == null)
                continue;

            for (var j = 0; j < namespaces.Length; j++)
            {
                var additional = namespaces[j];
                if (string.IsNullOrWhiteSpace(additional))
                    continue;

                Add(additional);
            }
        }

        return output.ToString();

        void Add(string @namespace)
        {
            if (seen.Add(@namespace))
            {
                output.Append("using ").Append(@namespace).AppendLine(";");
            }
        }
    }
}