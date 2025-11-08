namespace Buildenator.IntegrationTests.SharedEntities;

public class BaseClassWithPrivateSetter
{
    public string AProperty { get; private set; }
}

public class DerivedClassFromBaseWithPrivateSetter : BaseClassWithPrivateSetter
{
    public int DerivedProperty { get; set; }
}
