using Newtonsoft.Json;
using System.Collections.Generic;

namespace SwaggerToMCP.Models;

/// <summary>
/// Represents the root MCP context containing a collection of tools.
/// </summary>
public class McpContext
{
    /// <summary>
    /// Collection of MCP tools derived from OpenAPI operations.
    /// </summary>
    [JsonProperty("tools")]
    public List<McpTool> Tools { get; set; } = new();

    /// <summary>
    /// Optional metadata about the API as a whole.
    /// </summary>
    [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
    public McpMetadata? Metadata { get; set; }
}

/// <summary>
/// Represents metadata about the API as a whole.
/// </summary>
public class McpMetadata
{
    /// <summary>
    /// Title of the API.
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description of the API.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }

    /// <summary>
    /// Version of the API.
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Contact information for the API.
    /// </summary>
    [JsonProperty("contact", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, string>? Contact { get; set; }

    /// <summary>
    /// License information for the API.
    /// </summary>
    [JsonProperty("license", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, string>? License { get; set; }

    /// <summary>
    /// Terms of service for the API.
    /// </summary>
    [JsonProperty("termsOfService", NullValueHandling = NullValueHandling.Ignore)]
    public string? TermsOfService { get; set; }

    /// <summary>
    /// Server information for the API.
    /// </summary>
    [JsonProperty("servers", NullValueHandling = NullValueHandling.Ignore)]
    public List<McpServerInfo>? Servers { get; set; }

    /// <summary>
    /// Security schemes defined in the API.
    /// </summary>
    [JsonProperty("securitySchemes", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object>? SecuritySchemes { get; set; }

    /// <summary>
    /// Additional OpenAPI information that doesn't map directly to standard MCP metadata fields.
    /// </summary>
    [JsonProperty("x-openapi-additional", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object>? AdditionalInfo { get; set; }
}
