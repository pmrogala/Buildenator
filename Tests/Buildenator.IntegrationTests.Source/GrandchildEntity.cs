using Buildenator.IntegrationTests.Source.DifferentNamespace;
using System.Collections.Generic;

namespace Buildenator.IntegrationTests.Source
{
    public class GrandchildEntity : ChildEntity
    {
        public GrandchildEntity(int propertyIntGetter, string propertyStringGetter, EntityInDifferentNamespace entityInDifferentNamespace, List<string> protectedProperty, IEnumerable<int> privateField) : base(propertyIntGetter, propertyStringGetter, entityInDifferentNamespace, protectedProperty, privateField)
        {
        }

        protected override List<string> ProtectedProperty => base.ProtectedProperty;

        public IInterfaceType InterfaceType { get; set; }
    }
}
