using Microsoft.CodeAnalysis;

namespace Buildenator.Diagnostics;

public sealed class BuildenatorDiagnostic
{
    public BuildenatorDiagnostic(DiagnosticDescriptor descriptor, Location location, params object?[]? messageArgs)
    {
        Descriptor = descriptor;
        Location = location;
        MessageArgs = messageArgs;
    }

    public DiagnosticDescriptor Descriptor { get; }
    public Location Location { get; }
    public object?[]? MessageArgs { get; }

    public static implicit operator Diagnostic(BuildenatorDiagnostic buildenatorDiagnostic)
        => Diagnostic.Create(
            buildenatorDiagnostic.Descriptor,
            buildenatorDiagnostic.Location,
            buildenatorDiagnostic.MessageArgs);

}
