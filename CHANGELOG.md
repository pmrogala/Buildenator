# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 8.7.0.0 - 2025-11-27

### Added
- **useChildBuilders option**: New configuration option that enables generating additional `With` methods that accept `Func<ChildBuilder, ChildBuilder>` for properties that have their own builders
  - **Default: `true`** - This feature is enabled by default
  - Available at assembly level via `[BuildenatorConfiguration(useChildBuilders: false)]` to disable
  - Available at builder level via `[MakeBuilder(typeof(MyClass), useChildBuilders: false)]` to disable
  - When enabled, properties whose types have builders will get an additional method signature:
    - `WithProperty(Func<PropertyBuilder, PropertyBuilder> configure)` in addition to `WithProperty(PropertyType value)`
  - This allows for fluent nested builder configuration:
    ```csharp
    var parent = ParentBuilder.Parent
        .WithChild(child => child
            .WithName("John")
            .WithValue(42))
        .Build();
    ```
  - The generator automatically discovers all builders in the compilation and maps them to their entity types
  - Works with both constructor parameters and settable properties
- **Collection child builders support**: When `useChildBuilders: true` is set, collections containing entities with builders also get `AddTo` methods that accept `Func<ChildBuilder, ChildBuilder>` configuration:
  - Works alongside existing `AddTo(params T[])` and `With(collection)` methods
  - Allows fluent configuration of collection items:
    ```csharp
    var parent = ParentBuilder.Parent
        .AddToChildren(
            child => child.WithName("Child1").WithValue(1),
            child => child.WithName("Child2").WithValue(2))
        .Build();
    ```
  - Supports both interface and concrete collection types

### Changed
- **`initializeCollectionsWithEmpty` default changed to `true`**: Collection fields are now initialized with empty collections by default instead of null. This prevents `NullReferenceException` when iterating over unset collections. To restore the previous behavior, set `initializeCollectionsWithEmpty: false`.

## 8.6.0.0 - 2025-11-26

### Added
- `PreBuild()` hook method: Similar to `PostBuild()`, this is an instance method that gets called in the Build method of the generated builder. This allows the builder a chance for further configuration before the object is built.
  - To override it, simply define your own implementation: `public void PreBuild() { /*your code here*/ }`
- **NullBox DebuggerDisplay**: Added `DebuggerDisplay` attribute and `ToString()` override to `NullBox<T>` struct for improved debugging experience
  - Values inside `NullBox<T>` are now visible at a glance in the debugger without expanding the object
  - `ToString()` returns the string representation of the contained value, or "null" if the value is null
- **initializeCollectionsWithEmpty option**: New configuration option for initializing collection fields with empty collections instead of null
  - Available at assembly level via `[BuildenatorConfiguration(initializeCollectionsWithEmpty: true)]`
  - Available at builder level via `[MakeBuilder(typeof(MyClass), initializeCollectionsWithEmpty: true)]`
  - Supports all collection types: `IEnumerable<T>`, `IList<T>`, `ICollection<T>`, `IReadOnlyList<T>`, `List<T>`, `HashSet<T>`, etc.
  - Supports dictionary types: `Dictionary<K,V>`, `IDictionary<K,V>`, `IReadOnlyDictionary<K,V>`
  - When enabled, calling `Build()` without setting collection values will result in empty collections instead of null
  - This helps avoid `NullReferenceException` when the entity iterates over collections
- **Default field initialization support**: User-defined default values in builders are now used to initialize generated fields
  - Define a static field or constant with the naming convention `Default{PropertyName}` (e.g., `DefaultName` for a `Name` property)
  - The generator will use this value to initialize the corresponding field in the generated builder
  - Example: `public const string DefaultName = "DefaultValue";` will generate `private NullBox<string>? _name = new NullBox<string>(DefaultName);`
  - Works with constants (`const`), static readonly fields (`static readonly`), and static properties (`static`)
  - When `With{PropertyName}` is called, it overrides the default value

## 8.5.0.2 - 2025-11-26

### Fixed
- Generator: fixed dictionary types (`Dictionary<K,V>`, `IDictionary<K,V>`, `IReadOnlyDictionary<K,V>`) handling in code generation. Previously, dictionary types were incorrectly treated as collections of `KeyValuePair<K,V>`, causing `BuildDefault` to generate `List<KeyValuePair<K,V>>` parameters and invalid `AddTo` methods. Now:
  - `BuildDefault` correctly uses dictionary types as parameters
  - `AddTo` methods for dictionaries accept `KeyValuePair<K,V>[]` and use indexer syntax `dictionary[item.Key] = item.Value`
  - Interface dictionary types (`IDictionary<K,V>`, `IReadOnlyDictionary<K,V>`) create proper `Dictionary<K,V>` instances

## 8.5.0.1 - 2025-11-26

### Fixed
- Generator: fixed crash when builders contain overloaded methods (methods with the same name but different parameter types). Previously, this would cause an `ArgumentException: An item with the same key has already been added` error. The generator now correctly handles multiple method overloads under the same name.

## 8.5.0.0 - 2025-11-11

### Added
- **AddTo methods for collection properties**: Builders now generate `AddTo<PropertyName>(params T[] items)` methods for collection properties and constructor parameters
  - Works with interface collection types: `IEnumerable<T>`, `IReadOnlyList<T>`, `ICollection<T>`, `IList<T>`, etc.
  - Works with concrete collection types: `List<T>`, `HashSet<T>`, and any class implementing `ICollection<T>`
  - Allows incremental addition of items: `.AddToItems("a", "b", "c")`
  - `With` methods continue to replace entire collection, `AddTo` methods append items
  - Respects user-defined AddTo methods (won't generate if custom implementation exists)
- `BuildMany` method can now be overridden by users

### Changed
### Removed

## 8.4.0.2 - 2025-11-10

### Fixed
- Generator: fixed exception when building entities with get-only properties (e.g. `public bool ABool => true;` or `public string Name { get; }`) when `generateMethodsForUnreachableProperties` is true. The generated Build method now uses reflection with nullable operators (`SetMethod?.Invoke()`) to safely skip properties without setters, while properties with private setters continue to work correctly.

## 8.4.0.1 - 2025-11-08

### Fixed
- Generator: when generating methods for properties without public setters (e.g. inherited properties with private setters), the generator now resolves the property's declaring type before calling SetValue. This fixes "Property set method not found" errors when setting inherited, non-public setters from generated builders.

## 8.4.0.0 - 2024-10-31

### Changed

**Warning**
This version brings many breaking changes.

- Default static builder constructor is not generated by default anymore.
   - To enable it you have to use `generateStaticPropertyForBuilderCreation: true` either globally or on any builder level.
- Changed `defaultStaticCreator` to `generateDefaultBuildMethod` to better reflect the intention.
- Removed generating the namespace of an entity that is build by a builder to handle edge cases.
   - It shouldn't generate any regression, but it's a breaking change anyway.
- **Improved performance 3x**


## 8.3.0.0 - 2024-10-30

### Added
- Possibility to use static method for constructing an object instead of normal constructors.
   - simple usage ```[MakeBuilder(typeof(Entity), staticFactoryMethodName: nameof(Entity.CreateEntity))]```
   - It may be useful when your entity has private constructors and you create it by factory methods.


## 8.2.1.0 - 2024-10-30

### Changed
- Fixed the default nullable strategy - it should inherit the option from the project the builder is in.
   - *Warning* It may be a breaking change if someone is accustomed to previous behavior, but it was a bug.
- Changed the format of versioning: The biggest number is only to reflect minimum version of .net. The rest is like in the Semantic Versioning

## 8.2.0 - 2024-10-23

### Added
- Diagnostics for
  - replacing the default Build method
  - replacing the default constructor
  - a builder without the build method error
- Possibility to replace default Build method

## 6.1.3 & 8.1.3 - 2024-10-11

### Changed

- Fixed regression for default constructors - the build method should appear again
- Added generation of methods for properties from private constructors, keeping the Build method removed.

## 6.1.2 & 8.1.2 - 2024-10-11

### Changed

- Very weird issue only reproducible after creating and using nuget package. Filtering implicitly created constructors helped by some reason.
- Versioning number to the generation string

## 6.1.1 & 8.1.1 - 2024-10-11

### Changed

- Fixing code generation for the scenario with an entity having only private constructors. For now it will not generate automatically the build methods.
   - In other words, an user must define their own Build method without parameters returning the entity. In the future, more meaningful error will be added.

## 6.1.0 & 8.1.0 - 2024-01-05

### Changed

- Fixing code generation for some scenarios for the `NullableStrategy.Enabled` in the nullable context enabled project.

## 8.0.0 - 2023-12-24

### Changed

- Changed to the incremental generator. Should improve overall performance. Additionally it's a forced standard from .net 8 forward.
- IT'S COMPATIBLE ONLY WITH .NET8+

## 6.0.0 - 2023-03-25

### Added

- Bunch of unit tests generated by the chatGPT4

### Changed

- *Breaking change* It's not compatible with the .net 5 sdk anymore. Only .net 6+
- *Breaking change* Fixed many typos, and renaming of some properties names
- Restructured the code to smaller chunks so it's easier to unit test

## 5.2.2 - 2023-02-04

### Changed

- Fixed problem with having two builders with the same name but in different namespaces, ending up with the filename conflict.
    - Duplicates will receive numbers at the end of the file names. The incremental is shared among all builders.
    - Therefore ascending-sorting by names of builders and then by namespaces has been added, to make it deterministic

## 5.2.1 - 2023-01-12

### Hotfix

- `Debuggger.Launch()` removed...


## 5.2.0 - 2023-01-12

### Added

- `implicitCast` - possibility to generate implicit cast operator.


## 5.1.0 - 2022-12-16

### Added

- `generateMethodsForUnreachableProperties` - possibility to force generating builder methods for properties that don't have public setters.

## 5.0.0 - 2022-08-06

### Changed

- `createSingleFormat`, for generating properties by your fixture, has more arguments to use, and you also must use the fixture instance name now.
That's why it is a breaking change.
    - Example:
        - before: `Create<{0}>()` 
        - now: `{2}.Create<{0}>()`
    - you can also use the name of a property with `{1}`

## 4.3.0 - 2022-08-02

### Changed

- Possibility to write your own default constructor. It will prevent generating the generator's default one 

## 4.2.0 - 2022-08-01

### Added

- a method `PostBuild`, so you can do whatever you want after the default building method process.
  - to "override" it, you just simple write your definition: `public void PostBuild(<<className>> buildResult) { /*your code here*/ }`

### Changed

- Moving stuff around, preparing for unit testing the solution
- The Code build with the sdk 6

## 4.1.4 - 2021-12-14

### Added

- Auto-Generated comment

## 4.1.3 - 2021-12-14

### Changed

- Fixture auto moq fix
- Added possibility to use fixture name when create the additionalConfiguration for a fixture

## 4.1.2 - 2021-11-25

### Changed

- Full type name fix
- Global attribute fix

## 4.1.1 - 2021-11-05

### Added

- Reverse to older CodeAnalysis package

## 4.1.0 - 2021-11-05

### Added

- Nullable strategy: you can now force any nullable context upon the generated builder.
- Errors for trying to create builder for an abstract class


## 4.0.0 - 2021-10-01

### Added

- Now you can configure the global settings for all your builders in an assembly, by ```BuildenatorConfigurationAttribute```
    - By default null values are passed to ```MakeBuilderAttribute```, so then the global attribute has priority over the all builders.

### Changed

- *Breaking change* Now values are generated on build, i.e. the lazy approach rather than the eager one
    - it helps with generating unique objects on each build call
- *Breaking change* Static default builder is enabled by default



## 3.3.0 - 2021-09-21

### Added

- Handling generic types


## 3.2.0 - 2021-09-14

### Added

- Static default builder entity method generation
    - an example of a generated method: 
    ```public static Entity BuildDefault(int _param1 = default(int), string _param2 = default(string)) { return new Entity(_param1, _param2); }```


## 3.1.2 - 2021-09-14

### Changed

- Fix of the autofixture generation


## 3.1.1 - 2021-08-05

### Changed

- Fix of the trailing whitespace


## 3.1.0 - 2021-08-05

### Changed

- Fix nuget packaking


## 3.0.0 - 2021-07-15

### Changed

- Possibility to add a fixture fuctionality
  - faking strategies introduced
  - possibility to add custom facking providers
  - AutoFixture extension


## 2.0.0 - 2021-07-15

### Added

- Possibility to add a mocking fuctionality for interfaces
  - mocking strategies introduced
  - possibility to add custom mocking providers
  - Moq extension
- Faking strategies


## 1.2.0 - 2021-07-09

### Added

- Now the generator detected user defined "with" methods, so it does not generate duplicates causing compilation errors
- .NetBenchmark with some simple scenarios

### Changed

- a bit of refactoring of the code

## 1.1.0 - 2021-06-23

### Added

- Enabled fixtures configuration on the assembly and class levels.
  - It is not a breaking change.
- Fixing bug of rare possibility to generate some duplicated usings.

## 1.0.0 - 2021-06-07

### Added

- Initial projects
  - A basic builder source generator
     - the source generator creates the "with" methods basing on constructor parameters and settable properties.
       - if there is duplication in naming, a constructor parameter has higher priority than the property.
  - Sample projects
  - IntegrationTests
- a .gitignore file
