namespace Buildenator.IntegrationTests.SharedEntities;

public class EntityWithGetOnlyProperties
{
    public EntityWithGetOnlyProperties(int constructorValue)
    {
        ConstructorValue = constructorValue;
    }

    // This has a constructor parameter, so it should be included
    public int ConstructorValue { get; }

    // Expression-bodied property - should be filtered out
    public bool ExpressionBodiedProperty => true;

    // Get-only property without constructor - should be filtered out
    public string GetOnlyProperty { get; }

    // Settable property - should be included
    public string? SettableProperty { get; set; }
}
