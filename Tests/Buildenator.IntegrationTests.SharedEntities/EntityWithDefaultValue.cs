namespace Buildenator.IntegrationTests.SharedEntities;

public class EntityWithDefaultValue
{
    public EntityWithDefaultValue(string name, int count, string optionalValue)
    {
        Name = name;
        Count = count;
        OptionalValue = optionalValue;
    }

    public string Name { get; }
    public int Count { get; }
    public string OptionalValue { get; }
}
