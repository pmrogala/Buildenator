using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;
using System.Collections.Generic;

namespace Buildenator.IntegrationTests.Source.Builders
{
    [MakeBuilder(typeof(GrandchildEntity))]
    public partial class EntityBuilderWithCustomMethods
    {
        private int _propertyIntGetter;
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
            _protectedProperty.AddRange(value);
            return this;
        }
    }
}
