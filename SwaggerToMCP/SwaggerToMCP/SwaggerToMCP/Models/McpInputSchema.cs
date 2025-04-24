using Newtonsoft.Json;
using System.Collections.Generic;

namespace SwaggerToMCP.Models;

/// <summary>
/// Represents the input schema for an MCP tool, following JSON Schema format.
/// </summary>
public class McpInputSchema
{
    /// <summary>
    /// The JSON Schema type (typically "object" for API parameters).
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = "object";

    /// <summary>
    /// Properties of the input schema, mapping parameter names to their JSON Schema definitions.
    /// </summary>
    [JsonProperty("properties")]
    public Dictionary<string, McpSchemaProperty> Properties { get; set; } = new();

    /// <summary>
    /// List of required property names.
    /// </summary>
    [JsonProperty("required")]
    public List<string> Required { get; set; } = new();
}

/// <summary>
/// Represents a property in a JSON Schema.
/// </summary>
public class McpSchemaProperty
{
    /// <summary>
    /// The JSON Schema type of the property.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the property.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }

    /// <summary>
    /// For array types, the items schema.
    /// </summary>
    [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
    public McpSchemaProperty? Items { get; set; }

    /// <summary>
    /// For enum types, the list of possible values.
    /// </summary>
    [JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Enum { get; set; }

    /// <summary>
    /// For object types, the properties of the object.
    /// </summary>
    [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, McpSchemaProperty>? Properties { get; set; }

    /// <summary>
    /// For object types, the list of required property names.
    /// </summary>
    [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Required { get; set; }

    /// <summary>
    /// Additional format information for the property.
    /// </summary>
    [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
    public string? Format { get; set; }
}
