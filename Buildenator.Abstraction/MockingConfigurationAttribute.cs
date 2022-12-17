using System;

namespace Buildenator.Abstraction
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false)]
    public abstract class MockingConfigurationAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="typeDeclarationFormat"></param>
        /// <param name="fieldDeafultValueAssigmentFormat"></param>
        /// <param name="returnObjectFormat"></param>
        /// <param name="additionalUsings">List all the additional namespaces that are important for the fixture; separate them by comma ','. 
        /// An example: "Namespace1,Namespace2.Subspace"</param>
        public MockingConfigurationAttribute(
            MockingInterfacesStrategy strategy,
            string typeDeclarationFormat,
            string fieldDeafultValueAssigmentFormat,
            string returnObjectFormat,
            string? additionalUsings = null)
        {
            TypeDeclarationFormat = typeDeclarationFormat;
            FieldDeafultValueAssigmentFormat = fieldDeafultValueAssigmentFormat;
            ReturnObjectFormat = returnObjectFormat;
            Strategy = strategy;
            AdditionalUsings = additionalUsings;
        }

        public string TypeDeclarationFormat { get; }
        public string FieldDeafultValueAssigmentFormat { get; }
        public string ReturnObjectFormat { get; }
        public MockingInterfacesStrategy Strategy { get; }
        public string? AdditionalUsings { get; }
    }
}
