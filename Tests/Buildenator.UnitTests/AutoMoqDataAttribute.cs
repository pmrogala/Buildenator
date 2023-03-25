using AutoFixture.Xunit2;

namespace Buildenator.UnitTests;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute() : base(MoqFixture.Create)
    {
    }
}