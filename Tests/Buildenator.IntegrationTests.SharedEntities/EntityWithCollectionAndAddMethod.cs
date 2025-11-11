using System.Collections.Generic;

namespace Buildenator.IntegrationTests.SharedEntities;

public class EntityWithCollectionAndAddMethod
{
    public IEnumerable<string> EnumerableItems { get; set; } = new List<string>();
    public IEnumerable<string> EnumerableConstructorItems { get; }
    public IReadOnlyList<int> ReadOnlyListItems { get; set; } = new List<int>();
    public ICollection<double> CollectionItems { get; set; } = new List<double>();
    public IList<bool> ListItems { get; set; } = new List<bool>();

    public EntityWithCollectionAndAddMethod(IEnumerable<string> enumerableConstructorItems)
    {
        EnumerableConstructorItems = enumerableConstructorItems;
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
