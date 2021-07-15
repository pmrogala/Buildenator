# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
### Changed
### Removed

## 2.0.0 - 2021-07-15

### Added

- Possibility to add a mocking fuctionality for interfaces
  - mocking strategies introduces
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
