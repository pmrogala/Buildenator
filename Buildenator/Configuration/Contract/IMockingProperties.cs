using Buildenator.Abstraction;
using Buildenator.Generators;

namespace Buildenator.Configuration.Contract
{
    internal interface IMockingProperties : IAdditionalNamespacesProvider
    {
        string FieldDeafultValueAssigmentFormat { get; }
        string ReturnObjectFormat { get; }
        MockingInterfacesStrategy Strategy { get; }
        string TypeDeclarationFormat { get; }
    }
}