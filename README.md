# DataBuilder

[![NuGet](https://img.shields.io/nuget/v/QuinntyneBrown.DataBuilder.Cli.svg)](https://www.nuget.org/packages/QuinntyneBrown.DataBuilder.Cli/)
[![Build](https://github.com/QuinntyneBrown/DataBuilder/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/QuinntyneBrown/DataBuilder/actions/workflows/publish-nuget.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A CLI tool for scaffolding full-stack applications with a C# N-Tier API backend and Angular frontend.

## Overview

DataBuilder (`db`) is a .NET global tool that generates complete full-stack solutions with CRUD operations based on JSON schema definitions.

## Features

- **C# N-Tier API Backend**
  - Couchbase database integration with System.Text.Json serialization
  - Gateway.Core ORM for data access
  - Complete CRUD operations (GetAll, GetPage, GetById, Update, Delete, Create)
  - Service, Repository, and Controller layers
  - Support for complex nested objects and arrays

- **Angular Frontend**
  - Angular Material components throughout
  - Material Design 3 dark theme
  - Admin interface with list and detail views
  - Responsive sidenav layout with toolbar
  - Pre-configured API integration with HttpClient
  - JSON editor for complex object and array properties (with dark theme)
  - Clone button for duplicating entities

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

# Update to latest version
dotnet tool update --global QuinntyneBrown.DataBuilder.Cli
```

After installation, the `db` command will be available globally.

## Quick Start

```bash
# 1. Create a new solution with initial entities
db solution-create --name MyApp --directory ./my-app --json-file entities.json

# 2. Navigate to generated solution
cd my-app/src/MyApp.Api

# 3. Run the API
dotnet run

# 4. In another terminal, run the Angular UI
cd my-app/src/MyApp.Ui
npm install
ng serve
```

## Commands

### `solution-create`

Creates a new full-stack solution with API and Angular projects.

```bash
db solution-create --name MySolution --directory ./my-project
```

**Options:**
| Option | Alias | Description | Default |
|--------|-------|-------------|---------|
| `--name` | `-n` | Solution name (required) | - |
| `--directory` | `-d` | Output directory | Current directory |
| `--json-file` | `-j` | JSON schema file (opens editor if not provided) | - |
| `--bucket` | `-b` | Couchbase bucket name | `general` |
| `--scope` | `-s` | Couchbase scope name | `general` |
| `--collection` | `-c` | Couchbase collection name | Entity name |
| `--use-type-discriminator` | - | Use shared collection with type field | `false` |

### `model-add`

Adds a new entity with full CRUD support to an existing solution.

```bash
db model-add --json-file product.json
```

**Options:**
| Option | Alias | Description | Default |
|--------|-------|-------------|---------|
| `--json-file` | `-j` | JSON schema file (opens editor if not provided) | - |
| `--bucket` | `-b` | Couchbase bucket name | `general` |
| `--scope` | `-s` | Couchbase scope name | `general` |
| `--collection` | `-c` | Couchbase collection name | Entity name |
| `--use-type-discriminator` | - | Use shared collection with type field | `false` |

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

### Complex Property Support

DataBuilder supports nested objects and arrays in your schema. These are rendered as JSON editors in the Angular UI with full dark theme support:

```json
{
    "idea": {
        "title": "",
        "description": "",
        "tags": [""],
        "customMetadata": {
            "key": "value"
        }
    }
}
```

- **Arrays** (e.g., `tags`): Rendered as a JSON editor initialized with `[]`
- **Objects** (e.g., `customMetadata`): Rendered as a JSON editor initialized with `{}`

The JSON editors support validation, syntax highlighting, and a dark theme that matches the Material Design 3 aesthetic.

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

## Couchbase Storage Options

DataBuilder supports two storage strategies for Couchbase:

### 1. Separate Collections (Default)

Each entity is stored in its own collection within the specified bucket and scope. This is the default behavior and recommended for most use cases.

```bash
# Each entity gets its own collection (e.g., 'product', 'category')
db solution-create -n MyApp -j entities.json -b myBucket -s myScope
```

**Generated structure:**
- Bucket: `myBucket`
- Scope: `myScope`
- Collections: `product`, `category` (one per entity)

### 2. Shared Collection with Type Discriminator

All entities share a single collection and use a `type` field to distinguish between entity types. Useful for simpler setups or when you want all data in one collection.

```bash
# All entities in 'general' collection with type discriminator
db solution-create -n MyApp -j entities.json --use-type-discriminator
```

**Generated structure:**
- Bucket: `general`
- Scope: `general`
- Collection: `general`
- Documents include `"type": "product"` or `"type": "category"` field

### Auto-Detection via `type` Property

If your JSON schema includes a `type` property on an entity, DataBuilder automatically enables type discrimination for that entity:

```json
{
    "product": {
        "type": "product",
        "name": "",
        "price": 0.0
    }
}
```

The `type` property is removed from the generated entity model (it's handled by the repository layer). This allows per-entity control over storage strategy without using CLI flags.

### Custom Configuration Examples

```bash
# Custom bucket and scope, separate collections per entity
db solution-create -n MyApp -j entities.json -b enterprise -s sales

# Custom bucket with type discriminator (all in one collection)
db solution-create -n MyApp -j entities.json -b enterprise -s sales --use-type-discriminator

# Force specific collection name for all entities
db solution-create -n MyApp -j entities.json -b enterprise -s sales -c documents --use-type-discriminator
```

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
| Generated Backend | .NET 9.0, Couchbase, System.Text.Json |
| Generated Frontend | Angular 19, Angular Material, RxJS, Monaco Editor |
| CI/CD | GitHub Actions |

## Development

```bash
# Build, pack, and install in one step
eng\scripts\install-tool.bat

# Or manually:
dotnet build src/DataBuilder.Cli -c Release
dotnet pack src/DataBuilder.Cli -c Release
dotnet tool install -g QuinntyneBrown.DataBuilder.Cli --add-source src/DataBuilder.Cli/nupkg
```

### CI/CD

The project uses GitHub Actions for continuous integration and deployment:

- **Automatic publishing**: Every push to `main` that modifies `src/DataBuilder.Cli/**` triggers a new NuGet release
- **Versioning**: Uses `{major}.{minor}.{run_number}` format (e.g., 1.0.42)
- **GitHub Releases**: Each publish creates a tagged release with installation instructions

## Documentation

- [Admin UI Implementation Guide](docs/ADMIN-UI-IMPLEMENTATION-GUIDE.md) - Material Design 3 dark theme patterns for generated UIs
- [Contributing Guide](docs/CONTRIBUTING.md) - How to contribute to DataBuilder

## Changelog

### v1.3.0
- Added JSON editor dark theme support (jse-theme-dark)
- Improved JSON editor initialization timing for better reliability
- Switched to System.Text.Json serializer for Couchbase with Newtonsoft compatibility mode
- Fixed object property initialization for JsonElement types
- Improved null handling with `DefaultIgnoreCondition.WhenWritingNull`

### v1.2.0
- Added Clone button to edit UI for duplicating entities
- Added JSON editor support for array properties in edit UI
- Fixed case sensitivity for template files

### v1.1.0
- Auto-detect type discriminator from JSON schema
- Added comprehensive unit test project with 290 tests
- GitHub Actions CI/CD pipeline

## License

MIT
