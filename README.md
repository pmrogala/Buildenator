# Buildenator
A test data builders source generator for .net 5 and later.


## A simple usage example

The following code:
```csharp
using Buildenator.Abstraction;
using SampleProject;

namespace SampleTestProject.Builders
{
    [MakeBuilder(typeof(DomainEntity))]
    [FixtureConfiguration(typeof(AutoFixture.Fixture))]
    public partial class DomainEntityBuilder
    {
    }
}
```
Will generate somthign very close to this source code:

```csharp
using System;
using AutoFixture;
using SampleProject;

namespace SampleTestProject.Builders
{
    public partial class DomainEntityBuilder
    {
        private readonly Fixture _fixture = new Fixture();

        public DomainEntityBuilder()
        {
            _propertyIntGetter = _fixture.Create<int>();
            _propertyStringGetter = _fixture.Create<string>();
        }

        private int _propertyIntGetter;

        public DomainEntityBuilder WithPropertyIntGetter(int value)
        {
            _propertyIntGetter = value;
            return this;
        }


        private string _propertyStringGetter;

        public DomainEntityBuilder WithPropertyStringGetter(string value)
        {
            _propertyStringGetter = value;
            return this;
        }

        public DomainEntity Build()
        {
            return new DomainEntity(_propertyIntGetter, _propertyStringGetter);
        }
        
        public static DomainEntityBuilder DomainEntity => new DomainEntityBuilder();
    }
}
```

Check ```Buildenator.IntegrationTests``` for more examples.
