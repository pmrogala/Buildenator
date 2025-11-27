namespace Buildenator.IntegrationTests.SharedEntities;

/// <summary>
/// A child entity used in the parent entity to test UseChildBuilders feature.
/// </summary>
public class ChildForParentEntity
{
    public ChildForParentEntity(string name, int value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public int Value { get; }
}
