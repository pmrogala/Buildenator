namespace Buildenator.CodeAnalysis
{
    internal interface ITypedSymbol
    {
        string SymbolName { get; }
        string SymbolPascalName { get; }
        string TypeFullName { get; }
        string TypeName { get; }
        string UnderScoreName { get; }

        string GenerateFieldInitialization();
        string GenerateFieldType();
        string GenerateLazyFieldType();
        string GenerateMethodParameterDefinition();
        bool IsFakeable();
        bool IsMockable();
        bool NeedsFieldInit();
    }
}