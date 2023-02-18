namespace Buildenator.IntegrationTests.SharedEntities
{
#nullable enable
    public class ReadOnlyEntityWithConstructor
    {
        public ReadOnlyEntityWithConstructor(
            int propertyInt,
            string property)
        {
            PropertyInt = propertyInt;
            Property = property;
            PrivateField = new[] { 1, 2 };
        }

        public int PropertyInt { get; set; }
        public string Property { get; set; }
        public string[]? NoConstructorProperty { get; set; }
        public int[] PrivateField { get; private set; }
    }
}
