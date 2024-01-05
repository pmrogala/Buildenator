using Buildenator.IntegrationTests.SharedEntitiesNullable.DifferentNamespace;

namespace Buildenator.IntegrationTests.SharedEntitiesNullable
{
    public class Entity
    {
        public Entity(int propertyIntGetter, string propertyStringGetter, EntityInDifferentNamespace entityInDifferentNamespace)
        {
            PropertyIntGetter = propertyIntGetter;
            PropertyGetter = propertyStringGetter;
            EntityInDifferentNamespace = entityInDifferentNamespace;
        }

        public int PropertyIntGetter { get; }
        public string PropertyGetter { get; }
        public EntityInDifferentNamespace EntityInDifferentNamespace { get; }
    }
}
