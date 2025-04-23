# SwaggerToMCP

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download)
[![OpenAPI](https://img.shields.io/badge/OpenAPI-3.0-6BA539)](https://www.openapis.org/)

A CLI tool that converts Swagger/OpenAPI JSON files into Anthropic's Model Context Protocol (MCP) format.

> **Project Summary**: SwaggerToMCP bridges the gap between OpenAPI specifications and Anthropic's Claude AI by transforming API definitions into the MCP format. This enables seamless integration of external APIs with Claude, allowing the AI to interact with these services through well-defined tool interfaces.

## Quick Overview

SwaggerToMCP is a command-line tool that takes a Swagger/OpenAPI JSON file as input and converts it to a valid MCPContext JSON file. The tool preserves all metadata from the OpenAPI input and outputs in compliance with Anthropic's Model Context Protocol (MCP) schema.

## Key Features

- Converts OpenAPI operations to MCP Tool objects
- Preserves all metadata from the OpenAPI specification
- Maps OpenAPI fields to their corresponding MCP fields
- Outputs a JSON file compliant with MCP's Tool schema
- Handles complex parameter structures and nested schemas
- Supports both OpenAPI 2.0 (Swagger) and OpenAPI 3.0+ specifications
- Maintains security definitions and requirements
- Provides detailed error reporting for invalid specifications

## Installation

```bash
# Clone the repository
git clone https://github.com/grparry/SwaggerToMCP.git
cd SwaggerToMCP

# Build the project
dotnet build
```

## Usage Example

```bash
# Convert a Swagger/OpenAPI file to MCP format
dotnet run --project SwaggerToMCP.Cli -- -i ./path/to/swagger.json -o ./output/mcp-context.json

# Or with full argument names
dotnet run --project SwaggerToMCP.Cli -- --input ./path/to/swagger.json --output ./output/mcp-context.json
```

## Contributing

Contributions are welcome! Please see our [CONTRIBUTING.md](CONTRIBUTING.md) file for detailed guidelines on how to contribute to this project.

If you'd like to improve SwaggerToMCP, please feel free to:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure your code follows the existing style and includes appropriate tests.

## Documentation

For detailed documentation, advanced usage examples, and technical details, please see the [detailed README](SwaggerToMCP/README.md) in the SwaggerToMCP directory.

## Examples

Check out the [examples directory](SwaggerToMCP/examples) for sample Swagger/OpenAPI files and their corresponding MCP outputs. These examples demonstrate how to use the tool with different API specifications and how to integrate the generated MCP files with Claude.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Citation

If you use this software in your research or projects, please cite it using the information in the [CITATION.cff](CITATION.cff) file.