namespace Buildenator.IntegrationTests.Source
{
    public class Entity
    {
        public Entity(int propertyIntGetter, string propertyStringGetter)
        {
            PropertyIntGetter = propertyIntGetter;
            PropertyGetter = propertyStringGetter;
        }

        public int PropertyIntGetter { get; }
        public string PropertyGetter { get; }
    }
}
