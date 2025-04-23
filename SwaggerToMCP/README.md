# SwaggerToMCP

A CLI tool that converts Swagger/OpenAPI JSON files into Anthropic's Model Context Protocol (MCP) format.

## Overview

SwaggerToMCP is a command-line tool that takes a Swagger/OpenAPI JSON file as input and converts it to a valid MCPContext JSON file. The tool preserves all metadata from the OpenAPI input and outputs in compliance with Anthropic's Model Context Protocol (MCP) schema.

### What is MCP?

Model Context Protocol (MCP) is Anthropic's structured format for defining tools that Claude can use. MCP enables Claude to interact with external systems through well-defined interfaces, allowing the AI to call functions, APIs, and services with proper parameter validation. This protocol standardizes how tools are described to Claude, ensuring consistent and reliable tool usage across different applications.

## Features

- Converts OpenAPI operations to MCP Tool objects
- Preserves all metadata from the OpenAPI specification
- Maps OpenAPI fields to their corresponding MCP fields
- Outputs a JSON file compliant with MCP's Tool schema

## Requirements

- .NET 9.0 SDK or later

## Installation

Clone the repository and build the project:

```bash
git clone <repository-url>
cd SwaggerToMCP
dotnet build
```

## Usage

### Command Line

The CLI supports both positional and named arguments:

```bash
# Using named arguments
dotnet run --project SwaggerToMCP.Cli -- --input ./Swagger.json --output ./McpContext.json

# Using short form arguments
dotnet run --project SwaggerToMCP.Cli -- -i ./Swagger.json -o ./McpContext.json
```

### CLI Flags

| Flag | Long Form | Description |
|------|-----------|-------------|
| `-i` | `--input` | Path to the input Swagger/OpenAPI JSON file |
| `-o` | `--output` | Path where the MCP JSON file will be written |
| `-v` | `--verbose` | Enable verbose logging for debugging |
| `-h` | `--help` | Display help information |

### Field Mapping

The tool maps OpenAPI fields to MCP fields according to the following mapping:

| OpenAPI Field         | MCP Tool Field / Notes                                 |
|----------------------|-------------------------------------------------------|
| `operationId`        | `name` (unique identifier for the tool)                |
| `summary`/`description` | `description` (human-readable description)           |
| `parameters`, `requestBody` | `inputSchema` (JSON Schema for parameters/body)  |
| `security`           | `annotations` (security info, auth requirements)       |
| `tags`               | `annotations` (or custom field in annotations)         |
| `responses`          | `annotations` (preserve as metadata)                   |
| Custom Extensions    | `annotations` (preserve all unmapped metadata)         |

## Example Input and Output

### Sample Swagger Input

```json
{
  "swagger": "2.0",
  "info": {
    "title": "Swagger Petstore",
    "description": "This is a sample server Petstore server.",
    "version": "1.0.0"
  },
  "host": "petstore.swagger.io",
  "basePath": "/v2",
  "paths": {
    "/pet/{petId}": {
      "get": {
        "tags": ["pet"],
        "summary": "Find pet by ID",
        "description": "Returns a single pet",
        "operationId": "getPetById",
        "parameters": [
          {
            "name": "petId",
            "in": "path",
            "description": "ID of pet to return",
            "required": true,
            "type": "integer",
            "format": "int64"
          }
        ],
        "responses": {
          "200": {
            "description": "successful operation"
          },
          "404": {
            "description": "Pet not found"
          }
        },
        "security": [
          {
            "api_key": []
          }
        ]
      }
    }
  }
}
```

### Resulting MCP Output

```json
{
  "tools": [
    {
      "name": "getPetById",
      "description": "Find pet by ID",
      "inputSchema": {
        "type": "object",
        "properties": {
          "petId": {
            "type": "integer",
            "format": "int64",
            "description": "ID of pet to return"
          }
        },
        "required": ["petId"]
      },
      "annotations": {
        "security": [{ "api_key": [] }],
        "tags": ["pet"],
        "responses": {
          "200": { "description": "successful operation" },
          "404": { "description": "Pet not found" }
        },
        "x-openapi-original": {
          "path": "/pet/{petId}",
          "method": "get"
        }
      }
    }
  ],
  "metadata": {
    "title": "Swagger Petstore",
    "description": "This is a sample server Petstore server.",
    "version": "1.0.0"
  }
}
```

## Troubleshooting

### Common Issues

#### Missing operationId

**Problem**: OpenAPI operations without an `operationId` cannot be converted to MCP tools.

**Solution**: Add unique `operationId` values to each operation in your Swagger/OpenAPI file. Example:
```json
"get": {
  "operationId": "getUserById",
  // other operation properties
}
```

#### Invalid Parameter Types

**Problem**: MCP requires all parameters to have valid JSON Schema types.

**Solution**: Ensure all parameters in your Swagger/OpenAPI file have valid `type` properties (string, number, integer, boolean, array, object).

#### Circular References

**Problem**: OpenAPI schemas with circular references can cause conversion errors.

**Solution**: Restructure your schema to eliminate circular references, or use the `allOf` construct to reference shared components without creating circular dependencies.

#### Unsupported Authentication Methods

**Problem**: Some authentication methods in your Swagger file may not translate directly to MCP.

**Solution**: Authentication information is preserved in the `annotations` field of the MCP output. Your implementation will need to handle authentication appropriately when executing the tools.

## Development

### Project Structure

- `SwaggerToMCP` - Core library for converting OpenAPI to MCP
- `SwaggerToMCP.Cli` - Command-line interface for the converter
- `tests/SwaggerToMCP.Tests` - Unit tests for the converter

### Running Tests

```bash
dotnet test
```

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.
