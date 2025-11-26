using System.Collections.Generic;

namespace Buildenator.IntegrationTests.SharedEntities;

/// <summary>
/// Test entity for the initializeCollectionsWithEmpty feature.
/// All collection properties/parameters should be initialized to empty collections 
/// when the builder uses initializeCollectionsWithEmpty = true.
/// </summary>
public class EntityWithCollectionsForEmptyInit
{
    // Constructor parameters (collections)
    public IEnumerable<string> EnumerableItems { get; }
    public List<int> ConcreteListItems { get; }
    public IDictionary<string, string> DictionaryItems { get; }
    
    // Properties (collections with setters)
    public IReadOnlyList<double> ReadOnlyListProperty { get; set; } = new List<double>();
    public HashSet<string> HashSetProperty { get; set; } = new HashSet<string>();
    public IReadOnlyDictionary<int, string> ReadOnlyDictProperty { get; set; } = new Dictionary<int, string>();
    
    // Non-collection properties for comparison
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }

    public EntityWithCollectionsForEmptyInit(
        IEnumerable<string> enumerableItems,
        List<int> concreteListItems,
        IDictionary<string, string> dictionaryItems)
    {
        EnumerableItems = enumerableItems;
        ConcreteListItems = concreteListItems;
        DictionaryItems = dictionaryItems;
    }
}
