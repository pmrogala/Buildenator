# Buildenator
A test data **Builder Generator** for .net 6 and later.

Versioning:
N.X.Y.Z

- N - minimum version of .net required.
- X.Y.Z - standard semantic versioning.

## Features

- **Fluent API builder generation**: Automatically generates `With<PropertyName>` methods for all constructor parameters and settable properties
- **Collection support with AddTo methods**: Generate `AddTo<PropertyName>(params T[] items)` methods for collection properties
  - Works with interface types: `IEnumerable<T>`, `IReadOnlyList<T>`, `ICollection<T>`, `IList<T>`
  - Works with concrete types: `List<T>`, `HashSet<T>`, and any class implementing `ICollection<T>`
  - `With` methods replace entire collection, `AddTo` methods append items incrementally
- **Fixture integration**: Optional AutoFixture support for automatic test data generation
- **Mocking support**: Built-in integration with mocking frameworks (NSubstitute, Moq)
- **BuildMany method**: Generate multiple test objects with a single call
- **Customizable**: Override default behavior by implementing your own methods
- **Performance optimized**: Uses incremental source generators for fast compilation

## A simple usage example

The following code:
```csharp
using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using SampleProject;

namespace SampleTestProject.Builders
{
    [MakeBuilder(typeof(DomainEntity))]
    /* AutoFixture is optional. By adding it, the builder will use random data generator 
       for filling in not set up properties. */
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

## Collection Support with AddTo Methods

Buildenator automatically generates `AddTo<PropertyName>` methods for collection properties, allowing you to incrementally add items to collections:

```csharp
// Entity with collection properties
public class Order
{
    public IEnumerable<string> Items { get; set; }
    public List<decimal> Prices { get; set; }
}

// Generated builder usage
var order = OrderBuilder.Order
    .AddToItems("Product A", "Product B", "Product C")  // Add multiple items at once
    .AddToItems("Product D")                             // Or add one at a time
    .AddToPrices(9.99m, 19.99m)
    .Build();
// order.Items = ["Product A", "Product B", "Product C", "Product D"]
// order.Prices = [9.99, 19.99]

// With methods replace entire collection, AddTo methods append
var order2 = OrderBuilder.Order
    .WithItems(new[] { "Initial" })       // Set initial collection
    .AddToItems("Additional")             // Append to it
    .WithItems(new[] { "Replaced" })      // Replace everything
    .Build();
// order2.Items = ["Replaced"]
```

**Supported collection types:**
- Interface types: Anything that inherits: `IEnumerable<T>`
- Concrete types: `List<T>`, `HashSet<T>`, and any class implementing `ICollection<T>`


Feel free to contribute!
