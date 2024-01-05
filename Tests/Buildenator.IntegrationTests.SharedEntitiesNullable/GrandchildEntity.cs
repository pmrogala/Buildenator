using System.Collections.Generic;
using Buildenator.IntegrationTests.SharedEntitiesNullable.DifferentNamespace;

namespace Buildenator.IntegrationTests.SharedEntitiesNullable
{
    public class GrandchildEntity : ChildEntity
    {
        public GrandchildEntity(
            int propertyIntGetter,
            string propertyStringGetter,
            EntityInDifferentNamespace entityInDifferentNamespace,
            List<string> protectedProperty,
            IEnumerable<int> privateField)
            : base(propertyIntGetter, propertyStringGetter, entityInDifferentNamespace, protectedProperty, privateField)
        {
        }

        // ReSharper disable once RedundantOverriddenMember
        protected override List<string> ProtectedProperty => base.ProtectedProperty;

        public IInterfaceType? InterfaceType { get; set; }
    }

    public class GrandchildEntity<T, TK> : ChildEntity
        where T : struct
        where TK : notnull, EntityInDifferentNamespace, new()
    {
        public GrandchildEntity(int propertyIntGetter, string propertyStringGetter, EntityInDifferentNamespace entityInDifferentNamespace, List<string> protectedProperty, IEnumerable<int> privateField, TK secondGeneric) : base(propertyIntGetter, propertyStringGetter, entityInDifferentNamespace, protectedProperty, privateField)
        {
            SecondGeneric = secondGeneric;
        }

        // ReSharper disable once RedundantOverriddenMember
        protected override List<string> ProtectedProperty => base.ProtectedProperty;

        public T GenericType { get; set; }
        public TK SecondGeneric { get; }
    }
}
