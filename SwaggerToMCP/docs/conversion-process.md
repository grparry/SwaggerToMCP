# SwaggerToMCP Conversion Process

The following diagram illustrates the process of converting a Swagger/OpenAPI specification to Anthropic's Model Context Protocol (MCP) format:

```
┌─────────────────┐     ┌───────────────────┐     ┌────────────────┐
│                 │     │                   │     │                │
│  Swagger/OpenAPI│     │  SwaggerToMCP     │     │  MCP Context   │
│  Specification  │────▶│  Converter        │────▶│  JSON          │
│  (.json)        │     │                   │     │                │
│                 │     │                   │     │                │
└─────────────────┘     └───────────────────┘     └────────────────┘
                               │
                               │
                               ▼
                        ┌─────────────────┐
                        │                 │
                        │  Mapping Logic  │
                        │                 │
                        │ ┌─────────────┐ │
                        │ │OpenAPI Field│ │
                        │ │    ↓        │ │
                        │ │ MCP Field   │ │
                        │ └─────────────┘ │
                        │                 │
                        └─────────────────┘
```

## Conversion Steps

1. **Parse Swagger/OpenAPI JSON**
   - Load and validate the input Swagger/OpenAPI specification
   - Extract metadata, paths, operations, and schemas

2. **Map OpenAPI Elements to MCP**
   - Convert each API operation to an MCP Tool
   - Transform parameters and request bodies to MCP inputSchema
   - Preserve metadata in annotations

3. **Generate MCP JSON**
   - Create a valid MCP Context JSON structure
   - Include all converted tools
   - Add metadata from the original specification

4. **Validate Output**
   - Ensure the generated MCP JSON is valid
   - Verify all required fields are present
   - Check for any conversion issues

## Field Mapping Details

| OpenAPI Element | MCP Element | Notes |
|-----------------|-------------|-------|
| Operation | Tool | Each API operation becomes a separate MCP Tool |
| operationId | name | Unique identifier for the tool |
| summary/description | description | Human-readable description |
| parameters | inputSchema.properties | API parameters become properties in the inputSchema |
| required parameters | inputSchema.required | Required parameters are listed in the required array |
| requestBody | inputSchema | Request body schema is incorporated into inputSchema |
| responses | annotations.responses | Response information is preserved in annotations |
| security | annotations.security | Security requirements are preserved in annotations |
| tags | annotations.tags | Operation tags are preserved in annotations |
| x-* extensions | annotations | Custom extensions are preserved in annotations |

This mapping ensures that all relevant information from the OpenAPI specification is preserved in the MCP format, allowing Claude to understand and use the API correctly.
