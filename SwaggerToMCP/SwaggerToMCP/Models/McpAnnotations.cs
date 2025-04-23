using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SwaggerToMCP.Models;

/// <summary>
/// Represents annotations for an MCP tool, used to preserve metadata from the OpenAPI specification.
/// </summary>
public class McpAnnotations
{
    /// <summary>
    /// Security requirements from the OpenAPI specification.
    /// </summary>
    [JsonProperty("security", NullValueHandling = NullValueHandling.Ignore)]
    public List<Dictionary<string, List<string>>>? Security { get; set; }

    /// <summary>
    /// Tags from the OpenAPI specification.
    /// </summary>
    [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Response information from the OpenAPI specification.
    /// </summary>
    [JsonProperty("responses", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, McpResponseInfo>? Responses { get; set; }

    /// <summary>
    /// Original OpenAPI fields that don't map directly to MCP.
    /// </summary>
    [JsonProperty("x-openapi-original", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, JToken>? OpenApiOriginal { get; set; }

    /// <summary>
    /// Server information from the OpenAPI specification.
    /// </summary>
    [JsonProperty("servers", NullValueHandling = NullValueHandling.Ignore)]
    public List<McpServerInfo>? Servers { get; set; }
}

/// <summary>
/// Represents response information from the OpenAPI specification.
/// </summary>
public class McpResponseInfo
{
    /// <summary>
    /// Description of the response.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Content of the response, if specified.
    /// </summary>
    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object>? Content { get; set; }

    /// <summary>
    /// Headers of the response, if specified.
    /// </summary>
    [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object>? Headers { get; set; }
    
    /// <summary>
    /// Examples for this response, if specified.
    /// </summary>
    [JsonProperty("examples", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object>? Examples { get; set; }
}

/// <summary>
/// Represents server information from the OpenAPI specification.
/// </summary>
public class McpServerInfo
{
    /// <summary>
    /// URL of the server.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Description of the server.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }

    /// <summary>
    /// Variables for the server URL, if any.
    /// </summary>
    [JsonProperty("variables", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, JToken>? Variables { get; set; }
}
