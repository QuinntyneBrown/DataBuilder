# DataBuilder

[![NuGet](https://img.shields.io/nuget/v/QuinntyneBrown.DataBuilder.Cli.svg)](https://www.nuget.org/packages/QuinntyneBrown.DataBuilder.Cli/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A CLI tool for scaffolding full-stack applications with a C# N-Tier API backend and Angular frontend.

## Overview

DataBuilder (`db`) is a .NET global tool that generates complete full-stack solutions with CRUD operations based on JSON schema definitions.

## Features

- **C# N-Tier API Backend**
  - Couchbase database integration
  - Gateway.Core ORM for data access
  - Complete CRUD operations (GetAll, GetPage, GetById, Update, Delete, Create)
  - Service, Repository, and Controller layers

- **Angular Frontend**
  - Material Design 3 dark theme
  - Admin interface with list and detail views
  - Pre-configured API integration

- **Schema-Driven Generation**
  - Define data models with JSON
  - Automatic entity inference
  - Type-safe code generation using Scriban templates

- **Incremental Development**
  - Add new models to existing solutions
  - Automatic DI registration and route configuration

## Installation

```bash
# Install from NuGet
dotnet tool install --global QuinntyneBrown.DataBuilder.Cli
```

After installation, the `db` command will be available globally.

## Commands

### `solution-create`

Creates a new full-stack solution with API and Angular projects.

```bash
db solution-create --name MySolution --directory ./my-project
```

**Options:**
- `--name`: Solution name (default: `data-builder`)
- `--directory`: Output directory (default: current directory)
- `--json-file`: JSON schema file (optional, opens editor if not provided)

### `model-add`

Adds a new entity with full CRUD support to an existing solution.

```bash
db model-add --json-file product.json
```

**Options:**
- `--json-file`: JSON schema file (optional, opens editor if not provided)

**Requirements:**
- Must run from within a solution created by `solution-create`

## JSON Schema Example

```json
{
    "product": {
        "name": "",
        "price": 0.0,
        "isActive": true
    }
}
```

This generates a `Product` entity with full CRUD operations on both backend and frontend.

### ID Field Handling

The ID field maps directly to Couchbase's `Meta.id()` (the document key). DataBuilder handles ID fields as follows:

1. **`{entityName}Id` property** (e.g., `productId` for `Product`): Used as the ID field
2. **`id` property**: Used as the ID field
3. **No ID specified**: An `Id` property is automatically added

```json
// Option 1: Explicit entity ID
{
    "product": {
        "productId": "",
        "name": ""
    }
}

// Option 2: Generic ID
{
    "product": {
        "id": "",
        "name": ""
    }
}

// Option 3: Auto-generated (Id property added automatically)
{
    "product": {
        "name": ""
    }
}
```

The ID is always stored as a string and used directly as the Couchbase document key.

## Project Structure

```
DataBuilder/
├── src/
│   └── DataBuilder.Cli/           # CLI tool
│       ├── Commands/              # CLI command definitions
│       ├── Generators/
│       │   ├── Api/               # C# API code generator
│       │   └── Angular/           # Angular code generator
│       ├── Models/                # Entity and property definitions
│       ├── Services/              # Schema parsing, solution generation
│       ├── Templates/             # Scriban templates (.sbn)
│       │   ├── Api/               # API templates (Entity, Controller, etc.)
│       │   └── Angular/           # Angular templates (components, services)
│       └── Utilities/             # Naming conventions, type mapping
├── docs/                          # Documentation
├── playground/                    # Test projects and sample schemas
├── eng/
│   └── scripts/
│       └── install-tool.bat       # Build and install CLI tool locally
└── artifacts/                     # Generated output examples
```

## Requirements

- .NET 9.0 or later
- Node.js and npm (for Angular projects)

## Technology Stack

| Component | Technology |
|-----------|------------|
| CLI Framework | System.CommandLine |
| Templating | Scriban |
| String Utilities | Humanizer |
| Generated Backend | .NET 9.0, Couchbase, Gateway.Core |
| Generated Frontend | Angular, Material Design |

## Development

```bash
# Build, pack, and install in one step
eng\scripts\install-tool.bat

# Or manually:
dotnet build src/DataBuilder.Cli -c Release
dotnet pack src/DataBuilder.Cli -c Release
dotnet tool install -g QuinntyneBrown.DataBuilder.Cli --add-source src/DataBuilder.Cli/nupkg
```

## Documentation

- [Admin UI Implementation Guide](docs/ADMIN-UI-IMPLEMENTATION-GUIDE.md) - Material Design 3 dark theme patterns for generated UIs

## License

MIT
