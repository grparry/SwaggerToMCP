# SwaggerToMCP

A CLI tool that converts Swagger/OpenAPI JSON files into Anthropic's Model Context Protocol (MCP) format.

## Overview

SwaggerToMCP is a command-line tool that takes a Swagger/OpenAPI JSON file as input and converts it to a valid MCPContext JSON file. The tool preserves all metadata from the OpenAPI input and outputs in compliance with Anthropic's Model Context Protocol (MCP) schema.

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

## Example Output

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
