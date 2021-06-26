using Buildenator.IntegrationTests.Source.DifferentNamespace;
using System;
using System.Collections.Generic;

namespace Buildenator.IntegrationTests.Source
{
    public class ChildEntity : Entity
    {
        public ChildEntity(int propertyIntGetter, string propertyStringGetter, EntityInDifferentNamespace entityInDifferentNamespace, List<string> protectedProperty, IEnumerable<int> privateField)
            : base(propertyIntGetter, propertyStringGetter, entityInDifferentNamespace)
        {
            ProtectedProperty = protectedProperty;
            _privateField = privateField;
        }

        public byte[] ByteProperty { get; set; }
        protected virtual List<string> ProtectedProperty { get; }

        private readonly IEnumerable<int> _privateField;

        public IEnumerable<int> GetPrivateField() => _privateField;

        public List<string> GetProtectedProperty() => ProtectedProperty;
    }
}
