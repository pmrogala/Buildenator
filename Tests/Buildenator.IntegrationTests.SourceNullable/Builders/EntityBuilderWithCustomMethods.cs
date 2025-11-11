using System.Collections.Generic;
using Buildenator.Abstraction;
using Buildenator.Abstraction.Helpers;
using Buildenator.IntegrationTests.SharedEntitiesNullable;

namespace Buildenator.IntegrationTests.SourceNullable.Builders
{
    [MakeBuilder(typeof(GrandchildEntity))]
    public partial class EntityBuilderWithCustomMethods
    {
        private NullBox<int>? _propertyIntGetter;
        public EntityBuilderWithCustomMethods WithPropertyIntGetter(int value)
        {
            _propertyIntGetter = value / 2;
            return this;
        }

        public EntityBuilderWithCustomMethods WithPropertyStringGetter(string value)
        {
            _propertyStringGetter = value + "custom";
            return this;
        }

        private EntityBuilderWithCustomMethods WithProtectedProperty(List<string> value)
        {
            var list = _protectedProperty.HasValue 
                ? new List<string>(_protectedProperty.Value.Object) 
                : new List<string>();
            list.AddRange(value);
            _protectedProperty = new NullBox<List<string>>(list);
            return this;
        }
    }
}
