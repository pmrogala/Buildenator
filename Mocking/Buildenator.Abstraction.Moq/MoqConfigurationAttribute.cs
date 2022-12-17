using System;

namespace Buildenator.Abstraction.Moq
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false)]
    public class MoqConfigurationAttribute : MockingConfigurationAttribute
    {
        public MoqConfigurationAttribute(
            MockingInterfacesStrategy mockingInterfacesStrategy = MockingInterfacesStrategy.WithoutGenericCollection, 
            string typeDeclarationFormat = "Mock<{0}>",
            string fieldDeafultValueAssigmentFormat = "new Mock<{0}>()",                  
            string returnObjectFormat = "{0}?.Object",
            string additionalUsings = "Moq")
            : base(mockingInterfacesStrategy, typeDeclarationFormat, fieldDeafultValueAssigmentFormat, returnObjectFormat, additionalUsings)
        {
        }
    }
}
