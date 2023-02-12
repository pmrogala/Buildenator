namespace Buildenator.CodeAnalysis
{
    internal interface ITypedSymbol
    {
        string SymbolName { get; }
        string SymbolPascalName { get; }
        string TypeFullName { get; }
        string TypeName { get; }
        string UnderScoreName { get; }

        string? GenerateFieldInitialization();
        bool IsFakeable();
        bool IsMockable();
        bool NeedsFieldInit();
    }
}