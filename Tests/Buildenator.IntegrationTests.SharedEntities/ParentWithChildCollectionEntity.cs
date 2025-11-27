using System.Collections.Generic;
using System.Linq;

namespace Buildenator.IntegrationTests.SharedEntities;

/// <summary>
/// A parent entity that contains a collection of child entities, used to test UseChildBuilders feature with collections.
/// </summary>
public class ParentWithChildCollectionEntity
{
    public ParentWithChildCollectionEntity(IEnumerable<ChildForParentEntity> children, int parentValue)
    {
        Children = children.ToList();
        ParentValue = parentValue;
    }

    public IReadOnlyList<ChildForParentEntity> Children { get; }
    public int ParentValue { get; }
    
    /// <summary>
    /// A settable property with a list of children for testing AddTo methods with child builder configuration.
    /// </summary>
    public List<ChildForParentEntity> OptionalChildren { get; set; } = new();
}
