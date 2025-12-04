namespace Buildenator.IntegrationTests.SharedEntities;

/// <summary>
/// A parent entity that contains arrays of child entities, used to test UseChildBuilders feature with arrays.
/// </summary>
public class ParentWithChildArrayEntity
{
    public ParentWithChildArrayEntity(ChildForParentEntity[] children, int parentValue)
    {
        Children = children;
        ParentValue = parentValue;
    }

    public ChildForParentEntity[] Children { get; }
    public int ParentValue { get; }
    
    /// <summary>
    /// A settable property with an array of children for testing AddTo methods with child builder configuration.
    /// </summary>
    public ChildForParentEntity[] OptionalChildren { get; set; } = [];
}
