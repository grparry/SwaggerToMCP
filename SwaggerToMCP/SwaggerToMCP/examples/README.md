# SwaggerToMCP Examples

This directory contains example Swagger/OpenAPI files and their corresponding MCP output files generated by the SwaggerToMCP tool.

## Weather API Example

The Weather API example demonstrates conversion of a simple weather service API with two endpoints:
- `getCurrentWeather`: Get current weather conditions for a city
- `getWeatherForecast`: Get a multi-day forecast for a city

### Files:
- `weather-api.swagger.json`: The original Swagger/OpenAPI specification
- `weather-api.mcp.json`: The generated MCP output

### Usage:

To regenerate the MCP output from the Swagger file:

```bash
dotnet run --project ../SwaggerToMCP.Cli -- -i weather-api.swagger.json -o weather-api.mcp.json
```

## Adding Your Own Examples

Feel free to add your own examples to this directory. The process is simple:

1. Add your Swagger/OpenAPI JSON file (e.g., `my-api.swagger.json`)
2. Generate the MCP output:
   ```bash
   dotnet run --project ../SwaggerToMCP.Cli -- -i my-api.swagger.json -o my-api.mcp.json
   ```
3. Update this README to include information about your example

## Using Examples with Claude

The MCP files generated can be used directly with Anthropic's Claude API. Here's a simple example of how to use the generated MCP file with Claude:

```python
import anthropic
import json

# Load the MCP file
with open('weather-api.mcp.json', 'r') as f:
    mcp_tools = json.load(f)

# Initialize Claude client
client = anthropic.Anthropic(api_key="your_api_key")

# Create a message with the tools
message = client.messages.create(
    model="claude-3-opus-20240229",
    max_tokens=1000,
    temperature=0,
    system="You have access to weather tools to provide forecasts.",
    messages=[
        {
            "role": "user",
            "content": "What's the current weather in New York?"
        }
    ],
    tools=mcp_tools["tools"]
)

print(message.content)
```

This example shows how the converted MCP tools can be integrated with Claude to enable API interactions.
