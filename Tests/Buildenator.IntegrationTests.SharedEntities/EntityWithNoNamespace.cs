using Buildenator.IntegrationTests.SharedEntities;
using Buildenator.IntegrationTests.SharedEntities.DifferentNamespace;

public class EntityWithNoNamespace: Entity
{
    public EntityWithNoNamespace(int propertyIntGetter, string propertyStringGetter, EntityInDifferentNamespace entityInDifferentNamespace)
        :base(propertyIntGetter, propertyStringGetter, entityInDifferentNamespace)
    {
    }

    public new static EntityWithNoNamespace CreateEntity(int propertyIntGetter, string propertyStringGetter, int differentNamespaceId)
    {
        return new EntityWithNoNamespace(propertyIntGetter, propertyStringGetter, new EntityInDifferentNamespace() { Id = differentNamespaceId });
    }
}