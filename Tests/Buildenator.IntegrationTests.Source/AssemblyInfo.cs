using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.Abstraction.Moq;

[assembly: AutoFixtureConfiguration(fixtureTypeName: "Buildenator.IntegrationTests.Source.Fixtures.CustomFixture")]
[assembly: BuildenatorConfiguration(nullableStrategy: NullableStrategy.Default, generateStaticPropertyForBuilderCreation: true)]
[assembly: MoqConfiguration]
