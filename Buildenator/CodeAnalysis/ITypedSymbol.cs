namespace Buildenator.CodeAnalysis
{
    internal interface ITypedSymbol
    {
        string SymbolName { get; }
        string SymbolPascalName { get; }
        string TypeFullName { get; }
        string TypeName { get; }
        string UnderScoreName { get; }

        bool IsFakeable();
        bool IsMockable();
    }
}