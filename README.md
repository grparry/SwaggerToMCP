# SwaggerToMCP

A CLI tool that converts Swagger/OpenAPI JSON files into Anthropic's Model Context Protocol (MCP) format.

## Quick Overview

SwaggerToMCP is a command-line tool that takes a Swagger/OpenAPI JSON file as input and converts it to a valid MCPContext JSON file. The tool preserves all metadata from the OpenAPI input and outputs in compliance with Anthropic's Model Context Protocol (MCP) schema.

## Key Features

- Converts OpenAPI operations to MCP Tool objects
- Preserves all metadata from the OpenAPI specification
- Maps OpenAPI fields to their corresponding MCP fields
- Outputs a JSON file compliant with MCP's Tool schema

## Getting Started

For detailed documentation, installation instructions, and usage examples, please see the [detailed README](SwaggerToMCP/README.md) in the SwaggerToMCP directory.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.