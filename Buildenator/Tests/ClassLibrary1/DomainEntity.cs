namespace Buildenator.UnitTests.Source
{
    public class DomainEntity
    {
        public DomainEntity(int propertyIntGetter, string propertyStringGetter)
        {
            PropertyIntGetter = propertyIntGetter;
            PropertyGetter = propertyStringGetter;
        }

        public int PropertyIntGetter { get; private set; }
        public string PropertyGetter { get; private set; }
    }
}
