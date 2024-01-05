using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.Abstraction.Moq;

[assembly: AutoFixtureConfiguration(fixtureTypeName: "Buildenator.IntegrationTests.SourceNullable.Fixtures.CustomFixture")]
[assembly: BuildenatorConfiguration(nullableStrategy: NullableStrategy.Enabled)]
[assembly: MoqConfiguration]
