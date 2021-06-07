namespace SampleProject
{
    public class DomainEntity
    {
        public DomainEntity(int propertyIntGetter, string propertyStringGetter)
        {
            PropertyIntGetter = propertyIntGetter;
            PropertyStringGetter = propertyStringGetter;
        }

        public int PropertyIntGetter { get; private set; }
        public string PropertyStringGetter { get; private set; }

        public void DoMagic(int valInt, string valStr)
        {
            PropertyIntGetter = valStr.Length;
            PropertyStringGetter = valInt.ToString();
        }
    }
}
