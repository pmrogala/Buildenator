namespace Buildenator.Generators
{
    internal interface IAdditionalNamespacesProvider
    {
        string[] AdditionalNamespaces { get; }
    }
}