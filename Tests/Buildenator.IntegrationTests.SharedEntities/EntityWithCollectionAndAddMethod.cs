using System.Collections.Generic;

namespace Buildenator.IntegrationTests.SharedEntities;

public class EntityWithCollectionAndAddMethod
{
    // Interface collection types (existing)
    public IEnumerable<string> EnumerableItems { get; set; } = new List<string>();
    public IEnumerable<string> EnumerableConstructorItems { get; }
    public IReadOnlyList<int> ReadOnlyListItems { get; set; } = new List<int>();
    public ICollection<double> CollectionItems { get; set; } = new List<double>();
    public IList<bool> ListItems { get; set; } = new List<bool>();

    // Concrete collection types (new - should also get AddTo methods)
    public List<string> ConcreteListItems { get; set; } = new List<string>();
    public HashSet<int> ConcreteHashSetItems { get; set; } = new HashSet<int>();
    public List<char> ConcreteListConstructorItems { get; }

    public EntityWithCollectionAndAddMethod(
        IEnumerable<string> enumerableConstructorItems,
        List<char> concreteListConstructorItems)
    {
        EnumerableConstructorItems = enumerableConstructorItems;
        ConcreteListConstructorItems = concreteListConstructorItems;
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
