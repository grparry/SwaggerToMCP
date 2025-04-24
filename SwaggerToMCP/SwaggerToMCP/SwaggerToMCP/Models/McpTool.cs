using Newtonsoft.Json;

namespace SwaggerToMCP.Models;

/// <summary>
/// Represents a tool in the Model Context Protocol (MCP) format.
/// </summary>
public class McpTool
{
    /// <summary>
    /// Unique identifier for the tool.
    /// Maps from OpenAPI's operationId.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the tool.
    /// Maps from OpenAPI's summary or description.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// JSON Schema for the tool's input parameters.
    /// Maps from OpenAPI's parameters and requestBody.
    /// </summary>
    [JsonProperty("inputSchema")]
    public McpInputSchema InputSchema { get; set; } = new();

    /// <summary>
    /// Additional metadata and annotations for the tool.
    /// Used to preserve OpenAPI metadata that doesn't map directly to MCP fields.
    /// </summary>
    [JsonProperty("annotations")]
    public McpAnnotations Annotations { get; set; } = new();
}
