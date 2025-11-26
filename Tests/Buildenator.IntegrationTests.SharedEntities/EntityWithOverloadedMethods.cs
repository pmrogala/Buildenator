namespace Buildenator.IntegrationTests.SharedEntities;

public class EntityWithOverloadedMethods
{
    public EntityWithOverloadedMethods(int aValue) => AValue = aValue;

    public int AValue { get; set; }
}
