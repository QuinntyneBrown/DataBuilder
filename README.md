# DataBuilder

A powerful CLI tool for scaffolding full-stack applications with a C# N-Tier API backend and Angular frontend.

## Overview

DataBuilder (`db`) is a .NET global tool that streamlines the process of creating enterprise-grade full-stack applications. It generates a complete solution with CRUD operations based on your custom JSON schema, saving hours of boilerplate coding.

## Features

- **C# N-Tier API Backend**
  - Couchbase database integration
  - SystemTextJsonSerializer for serialization
  - Gateway.Core ORM for data access
  - Complete CRUD operations (GetAll, GetPage, GetById, Update, Delete, Create)

- **Angular Frontend**
  - Material Design UI components
  - Admin interface with CRUD operations
  - Pre-configured to connect to generated API
  - Modern, responsive design

- **Schema-Driven Generation**
  - Define your data model with JSON
  - Automatic entity inference from schema
  - Type-safe code generation

## Installation

Install as a global .NET tool:

```bash
dotnet tool install --global DataBuilder.Cli
```

Or install locally:

```bash
dotnet pack
dotnet tool install --global --add-source ./nupkg DataBuilder.Cli
```

## Usage

### Create a Solution

```bash
db solution-create --name MySolution --directory ./my-project
```

#### Options

- `--name` (optional): Name of the solution (default: `data-builder`)
- `--directory` (optional): Output directory (default: current directory)

The command will:
1. Open a JSON editor for defining your data model
2. Generate a complete C# N-Tier API with the specified entity
3. Create an Angular workspace with admin UI
4. Configure the frontend to connect to the backend API
5. Set up Material Design icons

#### Example JSON Schema

```json
{
    "toDo": {
        "id": 0,
        "name": ""
    }
}
```

This generates a `ToDo` entity with endpoints like `GetAllToDos`, `CreateToDo`, etc.

## Project Structure

```
DataBuilder/
├── src/
│   └── DataBuilder.Cli/       # CLI tool source code
│       ├── Commands/          # Command implementations
│       ├── Generators/        # Code generators
│       ├── Models/           # Data models
│       ├── Services/         # Business logic
│       ├── Templates/        # Scriban templates
│       └── Utilities/        # Helper utilities
├── docs/                     # Documentation
├── playground/               # Testing and examples
├── eng/                      # Engineering scripts
└── artifacts/                # Build outputs

```

## Requirements

- .NET 9.0 or later
- Node.js and npm (for Angular projects)

## Technology Stack

- **Backend**: .NET 9.0, Couchbase, Gateway.Core
- **Frontend**: Angular, Material Design
- **Templating**: Scriban
- **CLI Framework**: System.CommandLine

## Development

Build the solution:

```bash
dotnet build
```

Run tests:

```bash
dotnet test
```

Pack as a tool:

```bash
dotnet pack
```

## Documentation

For more detailed implementation guides, see:
- [Admin UI Implementation Guide](docs/ADMIN-UI-IMPLEMENTATION-GUIDE.md)
- [Project Ideas](docs/idea.md)

## License

See LICENSE file for details.
