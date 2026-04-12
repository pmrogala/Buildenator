# Buildenator — AI Coding Guide

C# **incremental source generator** that creates fluent builder classes for test data generation. Ships as a NuGet package (`Buildenator`). Users mark classes with `[MakeBuilder(typeof(Entity))]` and the generator emits `With<Property>`, `AddTo<Collection>`, `Build()`, `BuildMany()` methods at compile time.

## Build & Test Commands

```bash
dotnet build                                 # build everything (Debug)
dotnet test                                  # run all tests (unit + integration)
dotnet test Tests/Buildenator.UnitTests      # unit tests only
dotnet test Tests/Buildenator.IntegrationTests  # integration tests only (priority #1)
cd Tests/Buildenator.Benchmarks && dotnet run -c Release  # performance benchmarks
```

## Quality Gates

- **Nullable warnings are errors** — `<WarningsAsErrors>nullable</WarningsAsErrors>` in `Directory.Build.props`
- **CS8602 is an error** — `.editorconfig` promotes dereference-of-possibly-null to error
- Zero warnings required — treat any warning as a build failure

## Solution Structure

| Project | Purpose |
|---|---|
| `Buildenator/` | Core source generator (`netstandard2.0`, `IIncrementalGenerator`) |
| `Buildenator.Abstraction/` | Public attributes/enums consumed by user projects |
| `Buildenator.Abstraction.AutoFixture/` | AutoFixture integration attributes |
| `Mocking/Buildenator.Abstraction.Moq/` | Moq integration attributes |
| `Samples/SampleProject/` | Example domain entities |
| `Samples/SampleTestProject/` | Example test project using generated builders |

### Test Projects

| Project | Purpose |
|---|---|
| `Tests/Buildenator.IntegrationTests/` | End-to-end generation tests — **highest priority** |
| `Tests/Buildenator.IntegrationTests.Source/` | Builders + assembly config (AutoFixture + Moq) |
| `Tests/Buildenator.IntegrationTests.SourceNullable/` | Nullable reference type scenarios |
| `Tests/Buildenator.IntegrationTests.SourceWithoutAssemblyInfo/` | Builders without assembly-level config |
| `Tests/Buildenator.IntegrationTests.SharedEntities/` | Shared entity classes across Source projects |
| `Tests/Buildenator.IntegrationTests.SharedEntitiesNullable/` | Shared entities with nullable enabled |
| `Tests/Buildenator.UnitTests/` | Unit tests for extensions and utilities |
| `Tests/Buildenator.Benchmarks/` | BenchmarkDotNet performance tests |

## Generator Architecture

Entry point: `Buildenator/BuildersGenerator.cs` — `IIncrementalGenerator` using `ForAttributeWithMetadataName`.

Pipeline: attribute reading → `BuilderProperties` (config) → `EntityToBuild` (entity analysis) → `BuilderSourceStringGenerator` (code emission) → `PropertiesStringGenerator` / `ConstructorsGenerator` / `NamespacesGenerator` / `CommentsGenerator`.

Key directories inside `Buildenator/`:
- `Configuration/` — `BuilderProperties`, `EntityToBuild`, `FixtureProperties`, `MockingProperties`, `CollectionMethodDetector`
- `Generators/` — all code emission
- `CodeAnalysis/` — `TypedSymbol`, `Constructor`
- `Extensions/` — Roslyn helper extensions
- `Diagnostics/` — diagnostic descriptors (`BDN001`–`BDN008`)

## How Integration Tests Work

1. Entity classes are defined in `SharedEntities` / `SharedEntitiesNullable` projects
2. Builder stubs with `[MakeBuilder]` live in `Source` / `SourceNullable` / `SourceWithoutAssemblyInfo` projects
3. Source projects reference `Buildenator` as an analyzer — the generator runs during their compilation
4. `IntegrationTests` project references Source projects and consumes the generated builders
5. Tests verify builder behavior, generated methods, and built objects

## Code Conventions

- **SDK**: .NET 8.0.x (enforced by `global.json`)
- **Generator targets** `netstandard2.0` with `LangVersion: preview`
- **Nullable** enabled everywhere; violations are build errors
- File-scoped namespaces (`namespace Foo.Bar;`)
- Primary constructors used extensively
- `readonly struct` for value-like configuration types
- `StringBuilder` for all generated code — never string concatenation
- `is null` / `is not null` pattern — never `== null`
- Private fields prefixed with `_`
- `internal` visibility for generator internals (with `InternalsVisibleTo` for unit tests)
- Diagnostic IDs follow `BDN###` format

## Performance

Source generators run during compilation — **speed matters**. After any generator logic change:
1. Minimize allocations in hot paths
2. Use `StringBuilder` efficiently
3. Avoid LINQ in tight loops
4. Cache semantic model queries
5. Run benchmarks: `cd Tests/Buildenator.Benchmarks && dotnet run -c Release`

## Development Workflow

1. Write a failing integration test first (add entity to Source project, test in IntegrationTests)
2. Implement the change in the generator
3. Verify all tests pass: `dotnet test`
4. Run benchmarks to check performance impact
5. Add unit tests for new utility code

## Testing Conventions

- **xUnit 2.6.4** + **FluentAssertions 6.12.0**
- **AutoFixture** with **AutoData** / **AutoMoq** for test data generation
- Integration tests are the primary safety net — unit tests cover utilities
- Test naming: `BuildersGenerator_Scenario_ExpectedBehavior`

## NuGet Packaging

The generator NuGet packs the DLL as `analyzers/dotnet/cs` (not a normal reference). Abstraction source files are linked directly into the generator project via `<Compile Include=... Link=...>` so users only need one package reference.

## CI/CD

- **PR gate** (`pr-gate.yml`): restore → build → test → pack (runs on `windows-latest`)
- **Publish** (`publish-nugets.yml`): triggered by `v*` tags, pushes to NuGet.org

## Versioning

Format: `N.X.Y.Z` where N = minimum .NET version required, X.Y.Z = semver. Current: `8.8.0.0`.
