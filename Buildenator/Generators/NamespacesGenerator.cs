using System.Linq;
using System.Text;

namespace Buildenator.Generators;

internal static class NamespacesGenerator
{
    internal static string GenerateNamespaces(params IAdditionalNamespacesProvider?[] additionalNamespacesProviders)
    {
        var list = new[]
        {
            "System",
            "System.Linq",
            "Buildenator.Abstraction.Helpers"
        }.Concat(additionalNamespacesProviders.SelectMany(a => a?.AdditionalNamespaces ?? []));

        list = list.Distinct();

        var output = new StringBuilder();
        foreach (var @namespace in list)
        {
            output = output.Append("using ").Append(@namespace).AppendLine(";");
        }
        return output.ToString();
    }
}