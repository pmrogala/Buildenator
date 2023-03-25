using AutoFixture;
using AutoFixture.AutoMoq;

namespace Buildenator.UnitTests;

public static class MoqFixture
{
    public static IFixture Create() => new Fixture().Customize(new AutoMoqCustomization
        {
            GenerateDelegates = true
        });
}