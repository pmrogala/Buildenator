using Microsoft.CodeAnalysis;

namespace Buildenator
{
    internal class TypedSymbol
    {
        public TypedSymbol(IPropertySymbol symbol)
        {
            Symbol = symbol;
            Type = symbol.Type;
        }

        public TypedSymbol(IParameterSymbol symbol)
        {
            Symbol = symbol;
            Type = symbol.Type;
        }

        public void Deconstruct(out ISymbol symbol, out ITypeSymbol typeSymbol)
        {
            symbol = Symbol;
            typeSymbol = Type;
        }

        public virtual ISymbol Symbol { get; }
        public ITypeSymbol Type { get; }
    }
}