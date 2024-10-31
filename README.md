# Buildenator
A test data **Builder Generator** for .net 5 and later.

Versioning:
N.X.Y.Z
N - minimum version of .net required.
X.Y.Z - standard semantic versioning.

## A simple usage example

The following code:
```csharp
using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using SampleProject;

namespace SampleTestProject.Builders
{
    [MakeBuilder(typeof(DomainEntity))]
    [AutoFixtureConfiguration()]
    public partial class DomainEntityBuilder
    {
    }
}
```
Will generate something very close to this source code:

```csharp
using System;
using System.Linq;
using Buildenator.Abstraction.Helpers;
using SampleProject;
using AutoFixture;


namespace SampleTestProject.Builders
{
    public partial class DomainEntityBuilder
    {
        private readonly Fixture _fixture = new Fixture();

        public DomainEntityBuilder()
        {

        }

        private Nullbox<int>? _propertyIntGetter;
        private Nullbox<string>? _propertyStringGetter;


        public DomainEntityBuilder WithPropertyIntGetter(int value)
        {
            _propertyIntGetter = new Nullbox<int>(value);
            return this;
        }


        public DomainEntityBuilder WithPropertyStringGetter(string value)
        {
            _propertyStringGetter = new Nullbox<string>(value);
            return this;
        }

        public DomainEntity Build()
        {
            return new DomainEntity((_propertyIntGetter.HasValue ? _propertyIntGetter.Value : new Nullbox<int>(_fixture.Create<int>())).Object, (_propertyStringGetter.HasValue ? _propertyStringGetter.Value : new Nullbox<string>(_fixture.Create<string>())).Object)
            {

            };
        }

        public static DomainEntityBuilder DomainEntity => new DomainEntityBuilder();

        public System.Collections.Generic.IEnumerable<DomainEntity> BuildMany(int count = 3)
        {
            return Enumerable.Range(0, count).Select(_ => Build());
        }

        public static DomainEntity BuildDefault(int _propertyIntGetter = default(int), string _propertyStringGetter = default(string))
        {
            return new DomainEntity(_propertyIntGetter, _propertyStringGetter)
            {

            };
        }

    }
}
```

Check ```Buildenator.IntegrationTests``` for more examples.


Feel free to contribute!
