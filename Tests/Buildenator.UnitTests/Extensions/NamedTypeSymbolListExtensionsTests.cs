using System.Collections.Generic;
using Buildenator.Extensions;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Buildenator.UnitTests.Extensions
{
    public class NamedTypeSymbolListExtensionsTests
    {
        [Theory, AutoMoqData]
        public void MakeDeterministicOrderByName_ShouldSortTheListOfNamedTypeSymbols(List<(INamedTypeSymbol Builder, int)> list)
        {
            // Act
            list.MakeDeterministicOrderByName();

            // Assert
            list.Should().BeInAscendingOrder(x => x.Builder.Name)
                .And.BeInAscendingOrder(x => x.Builder.ContainingNamespace.Name);
        }
    }
}
