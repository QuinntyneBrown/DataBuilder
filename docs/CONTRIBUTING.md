# Contributing to DataBuilder

Thank you for your interest in contributing to DataBuilder! This document provides guidelines and instructions for contributing.

## Prerequisites

- .NET 9.0 SDK or later
- Node.js and npm (for testing Angular generation)
- Git

## Getting Started

1. Fork and clone the repository:
   ```bash
   git clone https://github.com/QuinntyneBrown/DataBuilder.git
   cd DataBuilder
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run tests:
   ```bash
   dotnet test
   ```

## Development Workflow

### Building and Installing Locally

Use the provided script to build, pack, and install the CLI tool locally:

```bash
eng\scripts\install-tool.bat
```

Or manually:

```bash
dotnet build src/DataBuilder.Cli -c Release
dotnet pack src/DataBuilder.Cli -c Release
dotnet tool install -g QuinntyneBrown.DataBuilder.Cli --add-source src/DataBuilder.Cli/nupkg
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test project
dotnet test tests/DataBuilder.Cli.Tests
```

## Project Structure

```
DataBuilder/
├── src/
│   └── DataBuilder.Cli/           # CLI tool source
│       ├── Commands/              # CLI command definitions
│       ├── Generators/
│       │   ├── Api/               # C# API code generator
│       │   └── Angular/           # Angular code generator
│       ├── Models/                # Entity and property definitions
│       ├── Services/              # Schema parsing, solution generation
│       ├── Templates/             # Scriban templates (.sbn)
│       └── Utilities/             # Naming conventions, type mapping
├── tests/
│   └── DataBuilder.Cli.Tests/     # Unit tests
├── docs/                          # Documentation
├── eng/
│   └── scripts/                   # Build and installation scripts
└── playground/                    # Test projects and sample schemas
```

## Making Changes

### Code Style

- Follow standard C# coding conventions
- Use meaningful variable and method names
- Keep methods focused and reasonably sized
- Add XML documentation comments for public APIs

### Templates

Templates are located in `src/DataBuilder.Cli/Templates/` and use [Scriban](https://github.com/scriban/scriban) syntax:

- `Api/` - C# backend templates (entities, controllers, services, repositories)
- `Angular/` - Angular frontend templates (components, services, models)

When modifying templates:
- Test changes by generating a sample solution
- Ensure generated code compiles without errors
- Verify both API and UI functionality

### Adding New Features

1. Create a feature branch from `main`
2. Implement your changes with appropriate tests
3. Ensure all tests pass: `dotnet test`
4. Update documentation if needed

## Submitting Changes

1. Create a descriptive branch name (e.g., `feature/add-validation-support`)
2. Make focused commits with clear messages
3. Push your branch and open a Pull Request
4. Describe your changes and link any related issues
5. Ensure CI checks pass

## Testing Guidelines

- Write unit tests for new functionality
- Tests are located in `tests/DataBuilder.Cli.Tests/`
- Follow existing test patterns and naming conventions
- Test both success and failure scenarios

## Reporting Issues

When reporting bugs, please include:
- DataBuilder version (`db --version`)
- .NET SDK version (`dotnet --version`)
- Operating system
- Steps to reproduce
- Expected vs actual behavior
- Sample JSON schema (if applicable)

## Questions

If you have questions about contributing, feel free to open an issue for discussion.
