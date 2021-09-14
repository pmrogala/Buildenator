using Buildenator.Abstraction.AutoFixture;
using Buildenator.Abstraction.Moq;
using Buildenator.IntegrationTests.Source.Fixtures;

[assembly: AutoFixtureConfiguration(fixtureTypeName: "Buildenator.IntegrationTests.Source.Fixtures.CustomFixture")]
[assembly: MoqConfiguration()]
