using System.Linq;
using AutoFixture;
using Buildenator.Extensions;
using FluentAssertions;
using Xunit;

namespace Buildenator.UnitTests.Extensions;

public class EnumerableExtensionsTests
{
	[Fact]
	public void Split_ShouldReturnLeftAndRightLists()
	{
		// Arrange
		var fixture = new Fixture();
		var source = fixture.CreateMany<int>(10).ToList();
		var expectedLeft = source.Where(x => x % 2 == 0).ToList();
		var expectedRight = source.Where(x => x % 2 != 0).ToList();

		// Act
		var result = source.AsEnumerable().Split(x => x % 2 == 0);

		// Assert
		result.Left.Should().BeEquivalentTo(expectedLeft);
		result.Right.Should().BeEquivalentTo(expectedRight);
	}

	[Fact]
	public void ToLists_ShouldReturnLists()
	{
		// Arrange
		var fixture = new Fixture();
		var source = fixture.CreateMany<int>(10).ToList();
		var expectedLeft = source.Where(x => x % 2 == 0).ToList();
		var expectedRight = source.Where(x => x % 2 != 0).ToList();
		var input = (expectedLeft.AsEnumerable(), expectedRight.AsEnumerable());

		// Act
		var result = input.ToLists();

		// Assert
		result.Left.Should().BeEquivalentTo(expectedLeft);
		result.Right.Should().BeEquivalentTo(expectedRight);
	}
}