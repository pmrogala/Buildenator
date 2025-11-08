# Buildenator - GitHub Copilot Instructions

## Project Overview

Buildenator is a C# source generator that automatically creates builder classes for test data generation. It generates fluent builder APIs from target classes, enabling developers to easily construct test objects with minimal boilerplate code.

## Primary Use Cases

1. **Creating Test Builders**: Generate builder classes that provide fluent APIs for constructing test objects
2. **Mocking Dependencies**: Support various mocking strategies (Auto, Explicit, Disabled) for test scenarios
3. **Generating Test Data**: Create instances with sensible defaults and allow selective property customization
4. **Reducing Boilerplate**: Eliminate manual test data setup code through source generation

## Architecture & Key Concepts

### Source Generator Pipeline

The generator follows this flow:
1. **Syntax Receivers**: Collect candidate classes marked with `[Buildable]` or assembly-level attributes
2. **Semantic Analysis**: Validate and extract metadata from candidates
3. **Code Generation**: Generate builder classes with fluent APIs
4. **Output**: Emit generated code into the compilation

### Configuration Hierarchy

Configuration follows a priority order:
1. **Assembly-level attributes** (`[assembly: BuilderDefaults(...)]`) - Global defaults
2. **Class-level attributes** (`[Buildable(...)]`) - Per-class overrides
3. **Built-in defaults** - Framework fallbacks

**Reasoning**: This allows developers to set project-wide conventions while maintaining flexibility for special cases.

### Mocking Strategies

- **Auto**: Automatically mock interface/abstract dependencies using NSubstitute
- **Explicit**: Require explicit values for all properties
- **Disabled**: No automatic mocking, use defaults for value types

## Performance Considerations

**CRITICAL**: Code generation must be fast. Source generators run during compilation and directly impact build times.

### Performance Guidelines

1. **Minimize allocations** in hot paths
2. **Cache semantic model queries** - they're expensive
3. **Use `StringBuilder` efficiently** for code generation
4. **Avoid LINQ in tight loops** where possible
5. **Leverage incremental generation** features when available
6. **Be mindful of Roslyn API costs** - especially symbol resolution

### Performance Testing

- Always run benchmarks in `Buildenator.Benchmarks` after changes
- Monitor generator execution time in diagnostic mode
- Check memory allocations using benchmark reports

## Code Structure

### Core Projects

**Buildenator** (Main Generator)
- **Purpose**: Core source generator implementation
- **Key Areas**:
  - `Configuration/` - Attribute definitions, default values, and configuration models
  - `Generators/` - Source generator pipeline, syntax receivers, code emitters
  - `Extensions/` - Roslyn API helpers for symbol/type analysis
  - `CodeAnalysis/` - Semantic analysis utilities
  - `Diagnostics/` - Error/warning reporting
  - `Exceptions/` - Configuration and validation exceptions

**Buildenator.Abstraction**
- **Purpose**: Public API contracts and attributes consumed by user projects
- **Key Components**:
  - Configuration attributes (`[Buildable]`, `[BuilderDefaults]`, etc.)
  - Strategy enums (MockingStrategy, NullableStrategy)
  - Helper utilities for configuration
- **Note**: This assembly is referenced by both the generator and user code

**Buildenator.Abstraction.AutoFixture**
- **Purpose**: AutoFixture integration attributes
- **Use Case**: Enables builders to integrate with AutoFixture test data generation
- **Key Attributes**: `[AutoFixtureConfiguration]`, `[AutoFixtureWithMoqConfiguration]`

**Buildenator.Abstraction.Moq**
- **Purpose**: Moq mocking framework integration
- **Use Case**: Provides Moq-specific configuration and helpers
- **Located**: `Mocking/Buildenator.Abstraction.Moq/`

### Test Projects

**Buildenator.IntegrationTests**
- **Purpose**: End-to-end generation scenarios (PRIORITY 1 for testing)
- **Structure**:
  - Contains test classes that consume generated builders
  - Uses xUnit, FluentAssertions, and AutoFixture
  - References Source projects to access generated builders

**Buildenator.IntegrationTests.Source**
- **Purpose**: Integration tests with specific source configurations
- **Use Case**: Tests generation against realistic project structures

**Buildenator.IntegrationTests.SourceNullable**
- **Purpose**: Nullable reference type scenarios
- **Use Case**: Validates nullable context handling

**Buildenator.IntegrationTests.SourceCore**
- **Purpose**: .NET Core-specific integration scenarios
- **Use Case**: Tests framework-specific behaviors

**Buildenator.IntegrationTests.SourceWithoutAssemblyInfo**
- **Purpose**: Tests generation without assembly-level configuration
- **Use Case**: Validates fallback to built-in defaults

**Buildenator.IntegrationTests.SharedEntities**
- **Purpose**: Shared test entity classes used across multiple test projects
- **Use Case**: Consistency in testing scenarios

**Buildenator.IntegrationTests.SharedEntitiesNullable**
- **Purpose**: Shared entities with nullable reference types enabled
- **Use Case**: Cross-project nullable testing

**Buildenator.UnitTests**
- **Purpose**: Unit tests for extensions, helpers, and utilities
- **Focus**: Configuration logic, validation, parsing, Roslyn helpers

**Buildenator.Benchmarks**
- **Purpose**: Performance testing and profiling
- **Key Metrics**: Generation time, memory allocations, compilation impact
- **Run After**: Any changes to generator logic

### Sample Projects

**SampleProject**
- **Purpose**: Demonstrates typical usage patterns
- **Located**: `Samples/SampleProject/`
- **Use Case**: Reference implementation for documentation

**SampleTestProject**
- **Purpose**: Shows how to use generated builders in tests
- **Located**: `Samples/SampleTestProject/`
- **Use Case**: Test code examples and patterns

**BuildersInTheSameProjectAsEntities**
- **Purpose**: Demonstrates in-project builder generation
- **Use Case**: Tests scenario where builders are generated alongside entities

## Testing Strategy

### Integration Tests (Priority #1)

Integration tests verify end-to-end generation scenarios. These are the **most important** tests.

**How integration tests work:**
- Test entities are defined in `Buildenator.IntegrationTests.Source` (or similar projects)
- Entities are marked with `[Buildable]` or assembly-level attributes
- Builders are generated during compilation of those projects
- Tests in `Buildenator.IntegrationTests` consume the generated builders
- Tests verify builder behavior, generated methods, and built objects

**How to write integration tests:**
```csharp
[Fact]
public void BuildersGenerator_YourScenario_ExpectedBehavior()
{
    // Arrange - Use the generated builder from Source project
    var builder = YourEntityBuilder.YourEntity;

    // Act - Use fluent API to configure and build
    var result = builder
        .WithPropertyName(value)
        .WithAnotherProperty(anotherValue)
        .Build();

    // Assert - Verify the built object
    result.PropertyName.Should().Be(value);
    result.AnotherProperty.Should().Be(anotherValue);
}
```

**Example testing builder features:**
```csharp
[Theory]
[AutoData]
public void BuildersGenerator_BuildMany_CreatesMultipleInstances(int count)
{
    var results = EntityBuilder.Entity.BuildMany(count).ToList();
    
    results.Should().HaveCount(count);
    results.Should().OnlyHaveUniqueItems();
}
```

**When to add integration tests:**
- New configuration options (add test entity to Source project)
- New property types (collections, nullables, etc.)
- Edge cases in code generation (inheritance, generics, etc.)
- Bug fixes (add regression test with problematic entity)
- Custom methods or post-build hooks

### Unit Tests

Use unit tests for:
- Helper methods and extensions
- Configuration logic
- Validation rules
- Parsing utilities

**Coverage goal**: Aim for comprehensive unit test coverage of utility classes, while relying on integration tests for generation logic.

## Common Patterns

### Nullable Handling

The generator respects nullable reference types:
- Nullable properties (`string?`) generate optional builder methods
- Non-nullable properties may require explicit values depending on mocking strategy

### Collection Support

Collections receive special handling:
- `Add{PropertyName}()` methods for single items
- `With{PropertyName}()` for replacing entire collection
- Support for `List<T>`, `IEnumerable<T>`, arrays, etc.

### With Pattern

All builders follow the "With" pattern:
```csharp
builder.WithPropertyName(value)
       .WithAnotherProperty(value)
       .Build();
```

## Common Pitfalls

1. **Forgetting Performance**: Always consider compilation time impact
2. **Not Testing Edge Cases**: Test nullable, collections, inheritance scenarios
3. **Breaking Changes**: Consider backward compatibility when changing attributes
4. **Incomplete Semantic Analysis**: Validate symbols exist before generating code
5. **String Concatenation**: Use `StringBuilder` for generated code
6. **Missing Diagnostics**: Report helpful errors when generation fails

## Development Workflow

### Making Changes

1. Identify the area: Configuration, Generation, or Extensions
2. Write failing integration test first
3. Implement the change
4. Verify all integration tests pass
5. Run benchmarks to check performance impact
6. Add unit tests for new utility code

### Debugging Generator Issues

Use the integration test setup:
```csharp
// Add test entity to Buildenator.IntegrationTests.Source
[Buildable]
public class YourTestEntity 
{ 
    public string Name { get; init; }
}

// Reference generated builder in Buildenator.IntegrationTests
// Set breakpoint in generator code, then run test
[Fact]
public void Test_YourScenario()
{
    var builder = YourTestEntityBuilder.YourTestEntity;
    var result = builder.Build();
}
```

Or attach debugger to a test project consuming Buildenator.

### Code Style

- Follow existing patterns in the codebase
- Keep methods focused and small
- Add XML documentation for public APIs
- Use descriptive variable names (generator code is read more than written)

## Key Files

- `BuildableAttribute.cs` - Main attribute for marking buildable classes
- `BuilderDefaultsAttribute.cs` - Assembly-level configuration
- `BuilderGenerator.cs` - Core generator implementation

## Extension Points

The generator is designed to be extended through:
- Custom attributes (add new configuration options)
- Mocking strategies (add support for new mocking frameworks)
- Property type handlers (add support for new collection types)

## Resources

- Roslyn Source Generators: https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview
- Incremental Generators: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md

## Questions to Ask When Contributing

- Does this change impact build performance?
- Do I have integration test coverage?
- Are there edge cases I haven't considered?
- Is the generated code idiomatic and readable?
- Does this work with nullable reference types?
- Have I updated documentation/examples?

## Support

For questions or issues, check:
1. Integration tests for examples
2. Generated code output in test projects
3. Existing GitHub issues
4. Source code comments and XML docs