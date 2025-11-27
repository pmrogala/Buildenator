namespace Buildenator.IntegrationTests.SharedEntities;

/// <summary>
/// A parent entity that contains a child entity, used to test UseChildBuilders feature.
/// </summary>
public class ParentWithChildEntity
{
    public ParentWithChildEntity(ChildForParentEntity child, int parentValue)
    {
        Child = child;
        ParentValue = parentValue;
    }

    public ChildForParentEntity Child { get; }
    public int ParentValue { get; }
    public ChildForParentEntity OptionalChild { get; set; }
}
