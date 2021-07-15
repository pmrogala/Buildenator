namespace Buildenator.IntegrationTests.SharedEntities
{
    public class SettableEntityWithConstructor
    {
        public SettableEntityWithConstructor(int propertyInt, string property, int[] privateField)
        {
            PropertyInt = propertyInt;
            Property = property;
            _privateField = privateField;
        }

        public int PropertyInt { get; set; }
        public string Property { get; set; }
        public string[] NoConstructorProperty { get; set; }

        private readonly int[] _privateField;

        public int[] GetPrivateField() => _privateField;
    }
}
