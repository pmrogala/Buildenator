# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.2.0]

### Added
### Changed
### Removed


## 5.1.0 - 2022-12-16

### Added

- `GenerateMethodsForUnrechableProperties ` - possibility to force generating builder methods for properties that have not public setters.

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
