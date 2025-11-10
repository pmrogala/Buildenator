namespace Buildenator.IntegrationTests.SharedEntities;

public class BaseClassWithPrivateSetter
{
    public string AProperty { get; private set; }
}

public class DerivedClassFromBaseWithPrivateSetter : BaseClassWithPrivateSetter
{
    public int DerivedProperty { get; set; }
    
    // Expression-bodied get-only property
    public bool ABool => true;
    
    // Get-only property with backing field
    public string ReadOnlyString { get; } = "readonly";
    
    // Computed get-only property
    public int ComputedValue => DerivedProperty * 2;
}
