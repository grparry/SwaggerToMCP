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
    
    /// <summary>
    /// Reference to another schema definition.
    /// </summary>
    [JsonProperty("$ref", NullValueHandling = NullValueHandling.Ignore)]
    public string? Ref { get; set; }
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
    
    /// <summary>
    /// Whether the property is nullable (OpenAPI 3.1 feature).
    /// </summary>
    [JsonProperty("nullable", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Nullable { get; set; }
    
    /// <summary>
    /// Reference to another schema definition (OpenAPI 3.1 allows this alongside other properties).
    /// </summary>
    [JsonProperty("$ref", NullValueHandling = NullValueHandling.Ignore)]
    public string? Ref { get; set; }
    
    /// <summary>
    /// OneOf composition (OpenAPI 3.1 feature).
    /// </summary>
    [JsonProperty("oneOf", NullValueHandling = NullValueHandling.Ignore)]
    public List<McpSchemaProperty>? OneOf { get; set; }
    
    /// <summary>
    /// AnyOf composition (OpenAPI 3.1 feature).
    /// </summary>
    [JsonProperty("anyOf", NullValueHandling = NullValueHandling.Ignore)]
    public List<McpSchemaProperty>? AnyOf { get; set; }
    
    /// <summary>
    /// AllOf composition (OpenAPI 3.1 feature).
    /// </summary>
    [JsonProperty("allOf", NullValueHandling = NullValueHandling.Ignore)]
    public List<McpSchemaProperty>? AllOf { get; set; }
    
    /// <summary>
    /// Discriminator for polymorphic schemas (OpenAPI 3.1 feature).
    /// </summary>
    [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
    public McpDiscriminator? Discriminator { get; set; }
}

/// <summary>
/// Represents a discriminator for polymorphic schemas in OpenAPI 3.1.
/// </summary>
public class McpDiscriminator
{
    /// <summary>
    /// The property name that is used to discriminate between the schemas.
    /// </summary>
    [JsonProperty("propertyName")]
    public string PropertyName { get; set; } = string.Empty;
    
    /// <summary>
    /// An object to hold mappings between payload values and schema names or references.
    /// </summary>
    [JsonProperty("mapping", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, string>? Mapping { get; set; }
}
