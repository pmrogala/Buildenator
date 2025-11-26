using System.Collections.Generic;

namespace Buildenator.IntegrationTests.SharedEntities;

/// <summary>
/// Entity used to test dictionary handling in generated builders.
/// Tests various dictionary types as constructor parameters and properties.
/// </summary>
public class EntityWithDictionary
{
    private readonly Dictionary<string, string> _metadata;
    private readonly IDictionary<int, string> _items;

    public EntityWithDictionary(
        IDictionary<string, string> metadata = null,
        IDictionary<int, string> items = null)
    {
        _metadata = metadata == null ? null : new Dictionary<string, string>(metadata);
        _items = items;
    }

    /// <summary>
    /// IReadOnlyDictionary property backed by constructor parameter
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata => _metadata;

    /// <summary>
    /// IDictionary property backed by constructor parameter
    /// </summary>
    public IDictionary<int, string> Items => _items;

    /// <summary>
    /// Settable Dictionary property
    /// </summary>
    public Dictionary<string, int> Scores { get; set; }

    /// <summary>
    /// Settable IReadOnlyDictionary property
    /// </summary>
    public IReadOnlyDictionary<string, object> Settings { get; set; }

    /// <summary>
    /// Simple non-dictionary property for comparison
    /// </summary>
    public string Name { get; set; }
}
