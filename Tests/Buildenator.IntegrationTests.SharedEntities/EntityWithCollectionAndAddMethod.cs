using System.Collections.Generic;

namespace Buildenator.IntegrationTests.SharedEntities;

public class EntityWithCollectionAndAddMethod
{
    private readonly List<string> _items = new();

    public IReadOnlyList<string> Items => _items;

    public void AddItem(string item)
    {
        _items.Add(item);
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
