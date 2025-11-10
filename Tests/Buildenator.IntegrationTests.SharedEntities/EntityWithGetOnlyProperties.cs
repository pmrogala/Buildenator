namespace Buildenator.IntegrationTests.SharedEntities
{
    public class EntityWithGetOnlyProperties
    {
        public EntityWithGetOnlyProperties(int settableProperty)
        {
            SettableProperty = settableProperty;
        }

        public int SettableProperty { get; set; }
        
        // Expression-bodied get-only property
        public bool ABool => true;
        
        // Get-only property with backing field
        public string ReadOnlyString { get; } = "readonly";
        
        // Computed get-only property
        public int ComputedValue => SettableProperty * 2;
    }
}
