using System;

namespace Buildenator.Abstraction.Moq;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false)]
public class MoqConfigurationAttribute : MockingConfigurationAttribute
{
    public MoqConfigurationAttribute(
        MockingInterfacesStrategy mockingInterfacesStrategy = MockingInterfacesStrategy.WithoutGenericCollection, 
        string typeDeclarationFormat = "Mock<{0}>",
        string fieldDefaultValueAssignmentFormat = "new Mock<{0}>()",                  
        string returnObjectFormat = "{0}?.Object",
        string additionalNamespaces = "Moq")
        : base(mockingInterfacesStrategy, typeDeclarationFormat, fieldDefaultValueAssignmentFormat, returnObjectFormat, additionalNamespaces)
    {
        }
}