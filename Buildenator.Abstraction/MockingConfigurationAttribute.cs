using System;

namespace Buildenator.Abstraction;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false)]
public abstract class MockingConfigurationAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strategy"></param>
    /// <param name="typeDeclarationFormat"></param>
    /// <param name="fieldDefaultValueAssignmentFormat"></param>
    /// <param name="returnObjectFormat"></param>
    /// <param name="additionalNamespaces">List all the additional namespaces that are important for the fixture; separate them by comma ','. 
    /// An example: "Namespace1,Namespace2.Subspace"</param>
    public MockingConfigurationAttribute(
        MockingInterfacesStrategy strategy,
        string typeDeclarationFormat,
        string fieldDefaultValueAssignmentFormat,
        string returnObjectFormat,
        string? additionalNamespaces = null)
    {
            TypeDeclarationFormat = typeDeclarationFormat;
            FieldDefaultValueAssignmentFormat = fieldDefaultValueAssignmentFormat;
            ReturnObjectFormat = returnObjectFormat;
            Strategy = strategy;
            AdditionalNamespaces = additionalNamespaces;
        }

    public string TypeDeclarationFormat { get; }
    public string FieldDefaultValueAssignmentFormat { get; }
    public string ReturnObjectFormat { get; }
    public MockingInterfacesStrategy Strategy { get; }
    public string? AdditionalNamespaces { get; }
}