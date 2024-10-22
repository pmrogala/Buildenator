using Buildenator.Configuration;
using Microsoft.CodeAnalysis;

namespace Buildenator.Diagnostics;

internal static class BuildenatorDiagnosticDescriptors
{
    internal static readonly DiagnosticDescriptor AbstractDiagnostic = new(
        "BDN001",
        "Cannot generate a builder for an abstract class",
        "Cannot generate a builder for the {0} abstract class",
        "Buildenator",
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor NoPublicConstructorsDiagnostic = new(
        "BDN002",
        "Build method in the builder is missing for a class with no public constructor",
        "Cannot generate a \"public {0} " + DefaultConstants.BuildMethodName + "() {/* content */}\" method for the {0} class that does not have public constructor. You have to add it by yourself.",
        "Buildenator",
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor BuildMethodOverridenDiagnostic = new(
        "BDN003",
        "You overriden the default " + DefaultConstants.BuildMethodName + "() method",
        "You overriden the default " + DefaultConstants.BuildMethodName + "() method. if it's not on purpose, please remove it.",
        "Buildenator",
        DiagnosticSeverity.Info,
        true);

    internal static readonly DiagnosticDescriptor DefaultConstructorOverridenDiagnostic = new(
        "BDN004",
        "You overriden the default constructor method",
        "You overriden the default constructor method. If it's not on purpose, please remove it.",
        "Buildenator",
        DiagnosticSeverity.Info,
        true);

    internal static readonly DiagnosticDescriptor DefaultParametersSetupOverridenDiagnostic = new(
        "BDN005",
        "You overriden the default method for setting up properties",
        "You overriden the default method {0} for setting up properties. If it's not on purpose, please remove it.",
        "Buildenator",
        DiagnosticSeverity.Info,
        true);
}