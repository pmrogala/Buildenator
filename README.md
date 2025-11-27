# Buildenator

[![NuGet](https://img.shields.io/nuget/v/Buildenator.svg)](https://www.nuget.org/packages/Buildenator/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A powerful **test data builder generator** for .NET 6 and later that uses C# source generators to automatically create fluent builder APIs for your test data.

**Why Buildenator?**
- 🚀 **Zero boilerplate** - Automatically generates builder classes from your entities
- 🎯 **Type-safe** - Compile-time validation of builder methods
- 🔄 **Fast** - Uses incremental source generators for minimal compilation overhead
- 🧪 **Test-friendly** - Built-in support for AutoFixture and mocking frameworks
- 📦 **Flexible** - Works with constructors, properties, collections, and more

---

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Features](#features)
- [Configuration](#configuration)
- [Advanced Usage](#advanced-usage)
- [Mocking Integration](#mocking-integration)
- [AutoFixture Integration](#autofixture-integration)
- [Examples](#examples)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

---

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package Buildenator
```

Or via Package Manager Console:

```powershell
Install-Package Buildenator
```

**Optional packages for additional features:**

```bash
# For AutoFixture integration
dotnet add package Buildenator.Abstraction.AutoFixture

# For Moq mocking framework integration
dotnet add package Buildenator.Abstraction.Moq
```

---

## Quick Start

### 1. Create Your Entity

```csharp
namespace YourProject
{
    public class User
    {
        public User(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
        public string? Email { get; set; }
    }
}
```

### 2. Create a Builder Class

In your test project, create a partial builder class:

```csharp
using Buildenator.Abstraction;
using YourProject;

namespace YourTestProject.Builders
{
    [MakeBuilder(typeof(User))]
    public partial class UserBuilder
    {
    }
}
```

### 3. Use the Generated Builder

```csharp
using YourTestProject.Builders;
using Xunit;

public class UserTests
{
    [Fact]
    public void User_ShouldHaveCorrectName()
    {
        // Arrange
        var user = new UserBuilder()
            .WithId(1)
            .WithName("John Doe")
            .WithEmail("john@example.com")
            .Build();

        // Assert
        Assert.Equal("John Doe", user.Name);
        Assert.Equal(1, user.Id);
    }
}
```

That's it! Buildenator automatically generates:
- `WithId()`, `WithName()`, `WithEmail()` fluent methods
- `Build()` method to create the instance
- `BuildMany(count)` method to create multiple instances
- Optional static factory property and default build method

---

## Versioning

This project uses a modified semantic versioning scheme:

**Format:** `N.X.Y.Z`

- **N** - Minimum version of .NET required (e.g., 8 = .NET 8+)
- **X.Y.Z** - Standard semantic versioning (Major.Minor.Patch)

## Features

### Core Features

#### 🎨 **Fluent API Builder Generation**
Automatically generates `With<PropertyName>` methods for all constructor parameters and settable properties, enabling a clean, readable fluent interface.

```csharp
var user = UserBuilder.User
    .WithId(42)
    .WithName("Jane Doe")
    .WithEmail("jane@example.com")
    .Build();
```

#### 📚 **Collection Support with AddTo Methods**
Generate `AddTo<PropertyName>(params T[] items)` methods for collection properties, allowing incremental item addition.

**Supported collection types:**
- Interface types: `IEnumerable<T>`, `IReadOnlyList<T>`, `ICollection<T>`, `IList<T>`
- Concrete types: `List<T>`, `HashSet<T>`, and any class implementing `ICollection<T>`

**Behavior:**
- `With` methods **replace** the entire collection
- `AddTo` methods **append** items incrementally

```csharp
var order = OrderBuilder.Order
    .AddToItems("Product A", "Product B")  // Append items
    .AddToItems("Product C")                // Append more
    .WithPrices(new[] { 9.99m, 19.99m })   // Replace entire collection
    .Build();
```

#### 🎲 **BuildMany Method**
Generate multiple test objects with a single call, each with unique values (when using AutoFixture).

```csharp
var users = UserBuilder.User.BuildMany(10).ToList();
// Creates 10 unique User instances
```

#### 🧩 **Mocking Framework Integration**
Built-in integration with popular mocking frameworks:
- **Moq** - via `Buildenator.Abstraction.Moq`
- **NSubstitute** - automatic mocking of interface dependencies
- Configurable mocking strategies (All, None, WithoutGenericCollection)

#### 🎯 **AutoFixture Integration**
Optional AutoFixture support for automatic test data generation with random, realistic values.

```csharp
[MakeBuilder(typeof(User))]
[AutoFixtureConfiguration()]
public partial class UserBuilder { }

// All properties not explicitly set will have random values
var user = UserBuilder.User.Build();
```

#### ⚙️ **Customizable & Extensible**
Override default behavior by implementing your own methods:
- Custom `Build()` method
- Custom `BuildMany()` method
- Custom `PreBuild()` hook for pre-build setup
- Custom `PostBuild()` hook for post-processing
- Custom `With*` methods for specific properties

```csharp
[MakeBuilder(typeof(User))]
public partial class UserBuilder
{
    // Custom PreBuild hook - called at the start of Build()
    public void PreBuild()
    {
        // Perform setup before building starts
        // e.g., configure default values, initialize state
    }

    // Custom PostBuild hook
    public void PostBuild(User user)
    {
        // Perform additional setup after building
        user.CreatedAt = DateTime.UtcNow;
    }
}
```

#### 🎯 **Default Field Initialization**
Define default values for builder fields using the `Default{PropertyName}` naming convention. The generator will automatically use these values to initialize fields.

```csharp
public class User
{
    public User(string name, int age) { Name = name; Age = age; }
    public string Name { get; }
    public int Age { get; }
}

[MakeBuilder(typeof(User))]
public partial class UserBuilder
{
    // Define default values using the Default{PropertyName} naming convention
    public const string DefaultName = "John Doe";
    public const int DefaultAge = 25;
}

// Usage - builds with default values when not explicitly set
var user = new UserBuilder().Build();
// user.Name = "John Doe", user.Age = 25

// Override defaults when needed
var customUser = new UserBuilder()
    .WithName("Jane Doe")
    .Build();
// customUser.Name = "Jane Doe", customUser.Age = 25 (default)
```

**Supported member types:**
- `const` fields
- `static readonly` fields  
- `static` properties

#### ⚡ **Performance Optimized**
Uses incremental source generators for fast compilation with minimal build-time impact. See [performance benchmarks](Tests/Buildenator.Benchmarks).

#### 🔧 **Advanced Capabilities**
- **Nullable reference type support** - Respects C# nullable contexts
- **Generic type support** - Works with generic classes
- **Private constructor handling** - Use static factory methods
- **Implicit casting** - Optional implicit conversion to target type
- **Static factory methods** - Use custom factory methods instead of constructors
- **Properties without public setters** - Generate methods for inherited or private setters
- **Multiple namespaces** - Builders can be in different namespaces than entities

---

## Configuration

Buildenator offers flexible configuration at multiple levels:

### Configuration Hierarchy

1. **Assembly-level** (global defaults) - Highest priority
2. **Class-level** (per builder) - Overrides assembly defaults
3. **Built-in defaults** - Framework fallbacks

### Assembly-Level Configuration

Apply settings globally to all builders in your test project:

```csharp
using Buildenator.Abstraction;
using Buildenator.Abstraction.AutoFixture;
using Buildenator.Abstraction.Moq;

// Global configuration for all builders
[assembly: BuildenatorConfiguration(
    buildingMethodsPrefix: "With",
    generateDefaultBuildMethod: true,
    nullableStrategy: NullableStrategy.Default,
    generateMethodsForUnreachableProperties: false,
    implicitCast: false,
    generateStaticPropertyForBuilderCreation: true,
    initializeCollectionsWithEmpty: false  // Initialize collections with empty instead of null
)]

// Optional: AutoFixture configuration
[assembly: AutoFixtureConfiguration(
    fixtureTypeName: "YourNamespace.CustomFixture"
)]

// Optional: Moq configuration
[assembly: MoqConfiguration]
```

### Class-Level Configuration

Override global settings for specific builders:

```csharp
[MakeBuilder(
    typeof(User),
    buildingMethodsPrefix: "Set",              // Use "Set" instead of "With"
    generateDefaultBuildMethod: true,          // Generate BuildDefault() method
    nullableStrategy: NullableStrategy.Enabled, // Force nullable context
    generateMethodsForUnreachableProperties: true, // Generate methods for private setters
    implicitCast: true,                        // Enable implicit casting
    staticFactoryMethodName: nameof(User.CreateUser), // Use static factory method
    generateStaticPropertyForBuilderCreation: true, // Generate static User property
    initializeCollectionsWithEmpty: true       // Initialize collections with empty instead of null
)]
public partial class UserBuilder { }
```

### Configuration Options Explained

#### `buildingMethodsPrefix` (default: `"With"`)
Prefix for generated methods:
- `"With"` → `WithName()`, `WithEmail()`
- `"Set"` → `SetName()`, `SetEmail()`

#### `generateDefaultBuildMethod` (default: `false`)
Generates a static `BuildDefault()` method with all parameters having default values:

```csharp
var user = UserBuilder.BuildDefault(id: 1, name: "Default");
```

#### `nullableStrategy`
Controls nullable reference type handling:
- `Default` - Inherit from project settings (recommended)
- `Enabled` - Force nullable context enabled
- `Disabled` - Force nullable context disabled

#### `generateMethodsForUnreachableProperties` (default: `false`)
When `true`, generates methods for properties without public setters (e.g., inherited properties with private setters).

#### `implicitCast` (default: `false`)
When `true`, enables implicit conversion from builder to entity:

```csharp
[MakeBuilder(typeof(User), implicitCast: true)]
public partial class UserBuilder { }

User user = UserBuilder.User.WithName("John"); // Implicit cast, no .Build() needed
```

#### `staticFactoryMethodName`
Use a static factory method instead of the constructor:

```csharp
public class User
{
    private User(int id, string name) { /* ... */ }
    
    public static User CreateUser(int id, string name) => new User(id, name);
}

[MakeBuilder(typeof(User), staticFactoryMethodName: nameof(User.CreateUser))]
public partial class UserBuilder { }
```

#### `generateStaticPropertyForBuilderCreation` (default: `false`)
Generates a static property for fluent builder creation:

```csharp
// When true:
var user = UserBuilder.User.WithName("John").Build();

// When false:
var user = new UserBuilder().WithName("John").Build();
```

#### `initializeCollectionsWithEmpty` (default: `false`)
When `true`, collection fields are initialized with empty collections in the builder constructor instead of null. This prevents `NullReferenceException` when the entity iterates over collections that weren't explicitly set.

**Supported collection types:**
- Interface types: `IEnumerable<T>`, `IList<T>`, `ICollection<T>`, `IReadOnlyList<T>`, etc.
- Concrete types: `List<T>`, `HashSet<T>`, and any class implementing `ICollection<T>`
- Dictionary types: `Dictionary<K,V>`, `IDictionary<K,V>`, `IReadOnlyDictionary<K,V>`

```csharp
// Assembly-level
[assembly: BuildenatorConfiguration(initializeCollectionsWithEmpty: true)]

// Or per-builder
[MakeBuilder(typeof(Order), initializeCollectionsWithEmpty: true)]
public partial class OrderBuilder { }
```

**Usage:**

```csharp
public class Order
{
    public Order(List<string> items)
    {
        Items = items.ToList(); // Would throw if items is null
    }
    public IReadOnlyList<string> Items { get; }
}

// Without initializeCollectionsWithEmpty: calling Build() without setting items → NullReferenceException
// With initializeCollectionsWithEmpty: items is automatically initialized as empty list
var order = OrderBuilder.Order.Build(); // Safe - Items is empty, not null
foreach (var item in order.Items) { } // No exception
```

#### `useChildBuilders` (default: `false`)
When `true`, generates additional `With` methods that accept `Func<ChildBuilder, ChildBuilder>` for properties that have their own builders. This enables fluent nested builder configuration.

**How it works:**
- The generator discovers all builders in the compilation
- For each property whose type has a corresponding builder, an additional method is generated
- The original `With(PropertyType value)` method is still available

```csharp
// Entity classes
public class Parent
{
    public Parent(Child child, int value) { Child = child; Value = value; }
    public Child Child { get; }
    public int Value { get; }
    public Child OptionalChild { get; set; }
}

public class Child
{
    public Child(string name, int age) { Name = name; Age = age; }
    public string Name { get; }
    public int Age { get; }
}

// Builders
[MakeBuilder(typeof(Child))]
public partial class ChildBuilder { }

[MakeBuilder(typeof(Parent), useChildBuilders: true)]
public partial class ParentBuilder { }
```

**Generated methods for ParentBuilder:**
```csharp
// Direct value method (always generated)
public ParentBuilder WithChild(Child value) { ... }

// Child builder method (generated when useChildBuilders: true)
public ParentBuilder WithChild(Func<ChildBuilder, ChildBuilder> configureChild) { ... }
```

**Usage:**
```csharp
// Using the child builder method for fluent nested configuration
var parent = ParentBuilder.Parent
    .WithChild(child => child
        .WithName("John")
        .WithAge(25))
    .WithValue(100)
    .WithOptionalChild(child => child
        .WithName("Jane")
        .WithAge(30))
    .Build();

// The direct method still works
var child = new Child("Bob", 35);
var parent2 = ParentBuilder.Parent
    .WithChild(child)
    .WithValue(200)
    .Build();
```

---

## Advanced Usage

### Working with Collections

Buildenator provides powerful collection handling with both `With` and `AddTo` methods.

```csharp
public class Order
{
    public IEnumerable<string> Items { get; set; }
    public List<decimal> Prices { get; set; }
    public IReadOnlyList<int> Quantities { get; set; }
}

[MakeBuilder(typeof(Order), generateStaticPropertyForBuilderCreation: true)]
public partial class OrderBuilder { }
```

**Usage examples:**

```csharp
// Adding items incrementally
var order = OrderBuilder.Order
    .AddToItems("Product A", "Product B")
    .AddToItems("Product C")  // Continues adding
    .Build();
// order.Items = ["Product A", "Product B", "Product C"]

// Replacing entire collection, then adding
var order2 = OrderBuilder.Order
    .WithItems(new[] { "Initial" })
    .AddToItems("Additional")
    .Build();
// order2.Items = ["Initial", "Additional"]

// Replacing after adding (With replaces everything)
var order3 = OrderBuilder.Order
    .AddToItems("Item 1", "Item 2")
    .WithItems(new[] { "Replaced" })
    .Build();
// order3.Items = ["Replaced"]

// Multiple collections
var order4 = OrderBuilder.Order
    .AddToItems("Widget")
    .AddToPrices(9.99m, 19.99m)
    .AddToQuantities(1, 2, 5)
    .Build();
```

### Custom Build Logic with PreBuild

Add custom logic that executes at the start of each `Build()` call:

```csharp
[MakeBuilder(typeof(User))]
public partial class UserBuilder
{
    private int _buildCount = 0;

    public void PreBuild()
    {
        // Called at the start of each Build() call
        _buildCount++;
        
        // Set up state before the entity is created
        // This runs every time Build() is called
    }
}
```

The `PreBuild()` method is called automatically at the start of the `Build()` method, before the entity is created.

### Custom Build Logic with PostBuild

Add custom logic after the default build process:

```csharp
[MakeBuilder(typeof(User))]
public partial class UserBuilder
{
    public void PostBuild(User user)
    {
        // Custom initialization
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;
        
        // Validation
        if (string.IsNullOrEmpty(user.Email))
        {
            user.Email = $"{user.Name.Replace(" ", "")}@example.com";
        }
    }
}
```

### Custom BuildMany Implementation

Override the default `BuildMany` behavior:

```csharp
[MakeBuilder(typeof(User), generateStaticPropertyForBuilderCreation: true)]
public partial class UserBuilder
{
    public IEnumerable<User> BuildMany(int count = 3)
    {
        // Custom implementation - e.g., sequential IDs
        for (int i = 1; i <= count; i++)
        {
            yield return WithId(i).Build();
        }
    }
}

var users = UserBuilder.User.BuildMany(5).ToList();
// Creates users with IDs 1, 2, 3, 4, 5
```

### Working with Inheritance

Buildenator handles inherited properties correctly:

```csharp
public class BaseEntity
{
    public int Id { get; private set; }
    protected string CreatedBy { get; protected set; }
}

public class User : BaseEntity
{
    public string Name { get; set; }
}

[MakeBuilder(typeof(User), generateMethodsForUnreachableProperties: true)]
public partial class UserBuilder { }

var user = new UserBuilder()
    .WithId(1)           // Works with private setter
    .WithCreatedBy("Admin")  // Works with protected property
    .WithName("John")
    .Build();
```

### Generic Types

Buildenator supports generic classes:

```csharp
public class Result<T>
{
    public T Value { get; set; }
    public bool IsSuccess { get; set; }
}

[MakeBuilder(typeof(Result<int>))]
public partial class IntResultBuilder { }

var result = new IntResultBuilder()
    .WithValue(42)
    .WithIsSuccess(true)
    .Build();
```

### Static Factory Methods

If your constructor is private or you have many of them and you want to use the one you want,
Use custom factory methods instead of constructors:

```csharp
public class User
{
    private User(int id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public static User CreateUser(int id, string name)
    {
        var user = new User(id, name);
        // Additional initialization
        user.CreatedAt = DateTime.UtcNow;
        return user;
    }
    
    public int Id { get; }
    public string Name { get; }
    public DateTime CreatedAt { get; private set; }
}

[MakeBuilder(typeof(User), staticFactoryMethodName: nameof(User.CreateUser))]
public partial class UserBuilder { }
```

### Custom With Methods

Define your own `With` methods for special logic:

```csharp
[MakeBuilder(typeof(User))]
public partial class UserBuilder
{
    // Buildenator won't generate WithEmail if you define it
    public UserBuilder WithEmail(string email)
    {
        // Custom validation
        if (!email.Contains("@"))
        {
            throw new ArgumentException("Invalid email format");
        }
        return WithEmail(email);
    }
    
    // Convenience method
    public UserBuilder WithAdminRole()
    {
        return WithRole("Admin").WithIsActive(true);
    }
}
```

---
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

## Mocking Integration

Buildenator integrates seamlessly with popular mocking frameworks to automatically mock interface dependencies.

### Using Moq

```bash
dotnet add package Buildenator.Abstraction.Moq
```

**Assembly-level configuration:**

```csharp
using Buildenator.Abstraction.Moq;

[assembly: MoqConfiguration]
```

**Usage:**

```csharp
public interface IUserRepository
{
    User GetById(int id);
}

public class UserService
{
    public UserService(IUserRepository repository)
    {
        Repository = repository;
    }
    
    public IUserRepository Repository { get; }
}

[MakeBuilder(typeof(UserService))]
public partial class UserServiceBuilder { }

// IUserRepository is automatically mocked with Moq
var service = new UserServiceBuilder().Build();
Assert.NotNull(service.Repository); // Mocked instance
```

### Mocking Strategies

Control which interfaces get mocked:

```csharp
public enum MockingInterfacesStrategy
{
    None = 0,                      // No automatic mocking
    All = 1,                       // Mock all interfaces
    WithoutGenericCollection = 2   // Mock all except generic collections (IEnumerable<T>, etc.)
}
```

**Custom mocking configuration:**

```csharp
[assembly: MoqConfiguration(strategy: MockingInterfacesStrategy.WithoutGenericCollection)]
```

### Manual Mocking

You can always override automatic mocking:

```csharp
var mockRepo = new Mock<IUserRepository>();
mockRepo.Setup(r => r.GetById(1)).Returns(new User { Id = 1, Name = "John" });

var service = new UserServiceBuilder()
    .WithRepository(mockRepo.Object)
    .Build();
```

---

## AutoFixture Integration

AutoFixture integration provides automatic random data generation for properties not explicitly set.

### Setup

```bash
dotnet add package Buildenator.Abstraction.AutoFixture
```

**Assembly-level configuration:**

```csharp
using Buildenator.Abstraction.AutoFixture;

[assembly: AutoFixtureConfiguration()]
```

**Or with a custom fixture:**

```csharp
using AutoFixture;

namespace YourTestProject
{
    public class CustomFixture : Fixture
    {
        public CustomFixture()
        {
            // Custom AutoFixture configuration
            this.Register(() => DateTime.UtcNow);
            this.Customize(new AutoMoqCustomization());
        }
    }
}

[assembly: AutoFixtureConfiguration(
    fixtureTypeName: "YourTestProject.CustomFixture"
)]
```

### Class-Level Configuration

```csharp
[MakeBuilder(typeof(User))]
[AutoFixtureConfiguration()]
public partial class UserBuilder { }
```

### Usage Examples

```csharp
// Properties not set explicitly get random values
var user = new UserBuilder()
    .WithId(1)  // Explicitly set
    .Build();   // Name, Email, etc. get random values

// Generate multiple unique instances
var users = new UserBuilder().BuildMany(10).ToList();
// All 10 users have different random data
```

### Fixture Strategies

Control what gets auto-generated:

```csharp
public enum FixtureInterfacesStrategy
{
    None = 0,                    // Don't generate any interfaces
    All = 1,                     // Generate all interfaces
    OnlyGenericCollections = 2   // Only generate generic collections (default)
}
```

### Combining AutoFixture with Moq

```csharp
using AutoFixture;
using AutoFixture.AutoMoq;

public class CustomFixture : Fixture
{
    public CustomFixture()
    {
        this.Customize(new AutoMoqCustomization());
    }
}

[assembly: AutoFixtureConfiguration(
    fixtureTypeName: "YourTestProject.CustomFixture"
)]
[assembly: MoqConfiguration]
```

---

## Examples

### Basic Entity Builder

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

[MakeBuilder(typeof(Product), generateStaticPropertyForBuilderCreation: true)]
public partial class ProductBuilder { }

// Usage
var product = ProductBuilder.Product
    .WithId(1)
    .WithName("Widget")
    .WithPrice(9.99m)
    .Build();
```

### Entity with Constructor

```csharp
public class Order
{
    public Order(int id, DateTime orderDate)
    {
        Id = id;
        OrderDate = orderDate;
    }
    
    public int Id { get; }
    public DateTime OrderDate { get; }
    public string CustomerName { get; set; }
}

[MakeBuilder(typeof(Order))]
[AutoFixtureConfiguration()]
public partial class OrderBuilder { }

// Usage - constructor parameters become With methods
var order = new OrderBuilder()
    .WithId(100)
    .WithOrderDate(DateTime.Today)
    .WithCustomerName("Acme Corp")
    .Build();
```

### Complex Domain Model

```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Order> Orders { get; set; } = new();
    public Address BillingAddress { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
}

[MakeBuilder(typeof(Customer), generateStaticPropertyForBuilderCreation: true)]
[AutoFixtureConfiguration()]
public partial class CustomerBuilder { }

[MakeBuilder(typeof(Address), generateStaticPropertyForBuilderCreation: true)]
public partial class AddressBuilder { }

// Usage
var customer = CustomerBuilder.Customer
    .WithId(1)
    .WithName("John Doe")
    .AddToOrders(
        new Order(1, DateTime.Today),
        new Order(2, DateTime.Today.AddDays(-1))
    )
    .WithBillingAddress(
        AddressBuilder.Address
            .WithStreet("123 Main St")
            .WithCity("Springfield")
            .WithPostalCode("12345")
            .Build()
    )
    .Build();
```

### Testing with Multiple Scenarios

```csharp
public class UserServiceTests
{
    private readonly UserBuilder _userBuilder;
    
    public UserServiceTests()
    {
        _userBuilder = new UserBuilder();
    }
    
    [Fact]
    public void Process_ActiveUser_ShouldSucceed()
    {
        // Arrange
        var user = _userBuilder
            .WithIsActive(true)
            .WithRole("User")
            .Build();
        
        // Act & Assert
        // ...
    }
    
    [Fact]
    public void Process_InactiveUser_ShouldFail()
    {
        // Arrange
        var user = _userBuilder
            .WithIsActive(false)
            .Build();
        
        // Act & Assert
        // ...
    }
    
    [Theory]
    [InlineData("Admin")]
    [InlineData("SuperUser")]
    public void Process_PrivilegedUser_ShouldHaveAccess(string role)
    {
        // Arrange
        var user = _userBuilder
            .WithRole(role)
            .Build();
        
        // Act & Assert
        // ...
    }
}
```

---

## Troubleshooting

### Common Issues

#### Builder class not found / Methods not generated

**Problem:** The builder doesn't compile or methods aren't available.

**Solutions:**
1. Ensure you've marked the builder class as `partial`
2. Check that `[MakeBuilder(typeof(YourEntity))]` attribute is correctly applied
3. Rebuild the project completely (`dotnet clean && dotnet build`)
4. Check that Buildenator package is properly installed
5. For Visual Studio users: Close and reopen files or restart IDE

#### Generated code not updating

**Problem:** Changes to entity aren't reflected in builder.

**Solutions:**
1. Clean and rebuild: `dotnet clean && dotnet build`
2. Delete `bin` and `obj` folders manually
3. Restart your IDE

#### Nullable reference warnings

**Problem:** Warnings about nullable reference types.

**Solutions:**
1. Use `nullableStrategy: NullableStrategy.Default` in configuration
2. Or configure explicitly per builder:
   ```csharp
   [MakeBuilder(typeof(User), nullableStrategy: NullableStrategy.Enabled)]
   ```

#### Private constructor - "Build method not found"

**Problem:** Entity has only private constructors.

**Solution:** Use static factory method:
```csharp
[MakeBuilder(typeof(User), staticFactoryMethodName: nameof(User.CreateUser))]
```

#### Private properties not accessible

**Problem:** Properties with a private setter can't be set.

**Solution:** Enable unreachable properties:
```csharp
[MakeBuilder(typeof(DerivedClass), generateMethodsForUnreachableProperties: true)]
```

### Performance Issues

If compilation is slow:
1. Ensure you're using the latest version of Buildenator
2. Check that you're not generating builders for very large object graphs
3. Consider splitting large builders into smaller, focused builders
4. Review [benchmarks](Tests/Buildenator.Benchmarks) for performance characteristics

### Getting Help

- Check existing [integration tests](Tests/Buildenator.IntegrationTests) for examples
- Review the [changelog](CHANGELOG.md) for recent changes
- Open an issue on GitHub with:
  - Buildenator version
  - .NET version
  - Minimal reproduction code
  - Build output / error messages

---

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Development Setup

1. Clone the repository
2. Build: `dotnet build`
3. Run tests: `dotnet test`
4. Run benchmarks: `cd Tests/Buildenator.Benchmarks && dotnet run -c Release`

### Testing

- **Integration tests** (priority #1): Add test entities to `Buildenator.IntegrationTests.Source` and tests to `Buildenator.IntegrationTests`
- **Unit tests**: Add to `Buildenator.UnitTests` for utility code
- See [project documentation](.github/agents/copilot-instructions.md) for detailed guidelines

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## A Simple Usage Example (Generated Code)

The following builder definition:
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

Generates code similar to this:

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
            return new DomainEntity(
                (_propertyIntGetter.HasValue ? _propertyIntGetter.Value : new Nullbox<int>(_fixture.Create<int>())).Object, 
                (_propertyStringGetter.HasValue ? _propertyStringGetter.Value : new Nullbox<string>(_fixture.Create<string>())).Object)
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

**Key points:**
- Properties not explicitly set with `With*` methods get random values from AutoFixture
- Each call to `Build()` generates a new instance with fresh random data
- `BuildMany(count)` creates multiple unique instances
- `BuildDefault()` creates instance with default values (no randomization)
- Static property `DomainEntity` provides fluent API entry point

For more examples, check the [integration tests](Tests/Buildenator.IntegrationTests).

---

**Made with ❤️ for the .NET testing community**
