using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using SwaggerToMCP.Models;

namespace SwaggerToMCP;

/// <summary>
/// Converts OpenAPI/Swagger documents to Anthropic's Model Context Protocol (MCP) format.
/// </summary>
public class SwaggerToMcpConverter
{
    /// <summary>
    /// Converts an OpenAPI document from a file path to an MCP context.
    /// </summary>
    /// <param name="openApiFilePath">Path to the OpenAPI JSON file.</param>
    /// <returns>An MCP context containing tools derived from the OpenAPI operations.</returns>
    public async Task<McpContext> ConvertFromFileAsync(string openApiFilePath)
    {
        using var fileStream = File.OpenRead(openApiFilePath);
        return await ConvertFromStreamAsync(fileStream);
    }

    /// <summary>
    /// Converts an OpenAPI document from a stream to an MCP context.
    /// </summary>
    /// <param name="openApiStream">Stream containing the OpenAPI JSON.</param>
    /// <returns>An MCP context containing tools derived from the OpenAPI operations.</returns>
    public async Task<McpContext> ConvertFromStreamAsync(Stream openApiStream)
    {
        var openApiDocument = await ReadOpenApiDocumentAsync(openApiStream);
        return ConvertOpenApiToMcp(openApiDocument);
    }

    /// <summary>
    /// Reads an OpenAPI document from a stream with support for OpenAPI 3.1.0.
    /// </summary>
    /// <param name="stream">Stream containing the OpenAPI JSON.</param>
    /// <returns>The parsed OpenAPI document.</returns>
    private async Task<OpenApiDocument> ReadOpenApiDocumentAsync(Stream stream)
    {
        // Configure reader with explicit 3.1.0 support
        var readerSettings = new OpenApiReaderSettings
        {
            // This setting will allow reading OpenAPI 3.1.0 documents
            // The reader is backwards compatible and will still read 3.0.x documents
            ReferenceResolution = ReferenceResolutionSetting.ResolveAllReferences,
            // Set maximum nesting depth for complex schemas
            MaximumAllowedRecursionDepth = 100 
        };

        var openApiReader = new OpenApiStreamReader(readerSettings);
        var result = await openApiReader.ReadAsync(stream);

        if (result.OpenApiDiagnostic.Errors.Any())
        {
            throw new InvalidOperationException($"Failed to parse OpenAPI document: {string.Join(", ", result.OpenApiDiagnostic.Errors)}");
        }

        // Log information about the document version
        Console.WriteLine($"Processed OpenAPI document version: {result.OpenApiDocument.Info?.Version}");
        Console.WriteLine($"OpenAPI spec version: {result.OpenApiDocument.SpecVersion}");

        return result.OpenApiDocument;
    }

    /// <summary>
    /// Converts an OpenAPI document to an MCP context.
    /// </summary>
    /// <param name="openApiDocument">The OpenAPI document to convert.</param>
    /// <returns>An MCP context containing tools derived from the OpenAPI operations.</returns>
    private McpContext ConvertOpenApiToMcp(OpenApiDocument openApiDocument)
    {
        var mcpContext = new McpContext
        {
            Metadata = CreateMetadata(openApiDocument),
            Tools = CreateTools(openApiDocument)
        };

        return mcpContext;
    }

    /// <summary>
    /// Creates metadata for the MCP context from the OpenAPI document.
    /// </summary>
    /// <param name="openApiDocument">The OpenAPI document.</param>
    /// <returns>Metadata for the MCP context.</returns>
    private McpMetadata CreateMetadata(OpenApiDocument openApiDocument)
    {
        var metadata = new McpMetadata
        {
            Title = openApiDocument.Info.Title,
            Description = openApiDocument.Info.Description,
            Version = openApiDocument.Info.Version
        };

        // Add contact information
        if (openApiDocument.Info.Contact != null)
        {
            metadata.Contact = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(openApiDocument.Info.Contact.Name))
                metadata.Contact["name"] = openApiDocument.Info.Contact.Name;
            if (!string.IsNullOrEmpty(openApiDocument.Info.Contact.Email))
                metadata.Contact["email"] = openApiDocument.Info.Contact.Email;
            if (openApiDocument.Info.Contact.Url != null)
                metadata.Contact["url"] = openApiDocument.Info.Contact.Url.ToString();
        }

        // Add license information
        if (openApiDocument.Info.License != null)
        {
            metadata.License = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(openApiDocument.Info.License.Name))
                metadata.License["name"] = openApiDocument.Info.License.Name;
            if (openApiDocument.Info.License.Url != null)
                metadata.License["url"] = openApiDocument.Info.License.Url.ToString();
        }

        // Add terms of service
        if (openApiDocument.Info.TermsOfService != null)
        {
            metadata.TermsOfService = openApiDocument.Info.TermsOfService.ToString();
        }

        // Add server information
        if (openApiDocument.Servers != null && openApiDocument.Servers.Any())
        {
            metadata.Servers = openApiDocument.Servers.Select(s => new McpServerInfo
            {
                Url = s.Url,
                Description = s.Description
            }).ToList();
        }

        // Add security schemes
        if (openApiDocument.Components?.SecuritySchemes != null && openApiDocument.Components.SecuritySchemes.Any())
        {
            metadata.SecuritySchemes = new Dictionary<string, object>();
            
            foreach (var scheme in openApiDocument.Components.SecuritySchemes)
            {
                var securityScheme = new Dictionary<string, object>
                {
                    { "type", scheme.Value.Type.ToString().ToLowerInvariant() },
                };
                
                if (!string.IsNullOrEmpty(scheme.Value.Description))
                    securityScheme["description"] = scheme.Value.Description;
                    
                if (!string.IsNullOrEmpty(scheme.Value.Name))
                    securityScheme["name"] = scheme.Value.Name;
                    
                if (scheme.Value.In != Microsoft.OpenApi.Models.ParameterLocation.Query) // Default is Query
                    securityScheme["in"] = scheme.Value.In.ToString().ToLowerInvariant();
                    
                if (scheme.Value.Scheme != null)
                    securityScheme["scheme"] = scheme.Value.Scheme;
                    
                if (scheme.Value.BearerFormat != null)
                    securityScheme["bearerFormat"] = scheme.Value.BearerFormat;
                
                // Add flows for OAuth2
                if (scheme.Value.Flows != null && scheme.Value.Type == Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2)
                {
                    var flows = new Dictionary<string, object>();
                    
                    if (scheme.Value.Flows.Implicit != null)
                        flows["implicit"] = CreateOAuthFlowObject(scheme.Value.Flows.Implicit);
                        
                    if (scheme.Value.Flows.Password != null)
                        flows["password"] = CreateOAuthFlowObject(scheme.Value.Flows.Password);
                        
                    if (scheme.Value.Flows.ClientCredentials != null)
                        flows["clientCredentials"] = CreateOAuthFlowObject(scheme.Value.Flows.ClientCredentials);
                        
                    if (scheme.Value.Flows.AuthorizationCode != null)
                        flows["authorizationCode"] = CreateOAuthFlowObject(scheme.Value.Flows.AuthorizationCode);
                        
                    if (flows.Any())
                        securityScheme["flows"] = flows;
                }
                
                metadata.SecuritySchemes[scheme.Key] = securityScheme;
            }
        }

        // Add any additional OpenAPI extensions as x-openapi-additional
        if (openApiDocument.Extensions != null && openApiDocument.Extensions.Any())
        {
            metadata.AdditionalInfo = new Dictionary<string, object>();
            
            foreach (var extension in openApiDocument.Extensions)
            {
                if (extension.Value != null)
                {
                    metadata.AdditionalInfo[extension.Key] = extension.Value;
                }
            }
        }

        return metadata;
    }

    /// <summary>
    /// Creates MCP tools from the operations in the OpenAPI document.
    /// </summary>
    /// <param name="openApiDocument">The OpenAPI document.</param>
    /// <returns>A list of MCP tools.</returns>
    private List<McpTool> CreateTools(OpenApiDocument openApiDocument)
    {
        var tools = new List<McpTool>();

        foreach (var pathItem in openApiDocument.Paths)
        {
            var path = pathItem.Key;
            var operations = pathItem.Value.Operations;

            foreach (var operation in operations)
            {
                var httpMethod = operation.Key.ToString().ToLowerInvariant();
                var operationValue = operation.Value;

                // Generate an operationId if one is not provided
                string operationId = operationValue.OperationId;
                if (string.IsNullOrEmpty(operationId))
                {
                    // Generate a camelCase operationId from the path and method
                    // Example: /api/pets/{id} with GET becomes getPetsById
                    string pathWithoutLeadingSlash = path.TrimStart('/');
                    string[] pathSegments = pathWithoutLeadingSlash.Split('/');
                    
                    // Convert path parameters from {param} to ByParam
                    for (int i = 0; i < pathSegments.Length; i++)
                    {
                        if (pathSegments[i].StartsWith("{") && pathSegments[i].EndsWith("}"))
                        {
                            // Extract parameter name and capitalize first letter
                            string paramName = pathSegments[i].Trim('{', '}');
                            pathSegments[i] = "By" + char.ToUpperInvariant(paramName[0]) + paramName.Substring(1);
                        }
                        else
                        {
                            // Capitalize first letter of each segment
                            pathSegments[i] = char.ToUpperInvariant(pathSegments[i][0]) + pathSegments[i].Substring(1);
                        }
                    }
                    
                    // Combine method and path segments
                    string methodPrefix = httpMethod.ToLowerInvariant();
                    string pathPart = string.Join("", pathSegments);
                    operationId = methodPrefix + pathPart;
                }

                var tool = new McpTool
                {
                    Name = operationId,
                    Description = !string.IsNullOrEmpty(operationValue.Summary) ? operationValue.Summary : operationValue.Description ?? string.Empty,
                    InputSchema = CreateInputSchema(operationValue, path, httpMethod),
                    Annotations = CreateAnnotations(operationValue, path, httpMethod, openApiDocument.Servers)
                };

                tools.Add(tool);
            }
        }

        return tools;
    }

    /// <summary>
    /// Creates an input schema for an MCP tool from an OpenAPI operation.
    /// </summary>
    /// <param name="operation">The OpenAPI operation.</param>
    /// <param name="path">The path of the operation.</param>
    /// <param name="httpMethod">The HTTP method of the operation.</param>
    /// <returns>An input schema for the MCP tool.</returns>
    private McpInputSchema CreateInputSchema(OpenApiOperation operation, string path, string httpMethod)
    {
        var inputSchema = new McpInputSchema
        {
            Type = "object",
            Properties = new Dictionary<string, McpSchemaProperty>(),
            Required = new List<string>()
        };

        // Add path parameters
        foreach (var parameter in operation.Parameters.Where(p => p.In == ParameterLocation.Path))
        {
            AddParameterToSchema(inputSchema, parameter);
        }

        // Add query parameters
        foreach (var parameter in operation.Parameters.Where(p => p.In == ParameterLocation.Query))
        {
            AddParameterToSchema(inputSchema, parameter);
        }

        // Add header parameters
        foreach (var parameter in operation.Parameters.Where(p => p.In == ParameterLocation.Header))
        {
            AddParameterToSchema(inputSchema, parameter);
        }

        // Add request body if present
        if (operation.RequestBody != null)
        {
            AddRequestBodyToSchema(inputSchema, operation.RequestBody);
        }

        return inputSchema;
    }

    /// <summary>
    /// Adds a parameter to an input schema.
    /// </summary>
    /// <param name="inputSchema">The input schema to add the parameter to.</param>
    /// <param name="parameter">The parameter to add.</param>
    private void AddParameterToSchema(McpInputSchema inputSchema, OpenApiParameter parameter)
    {
        var schemaProperty = ConvertOpenApiSchemaToMcpSchemaProperty(parameter.Schema);
        schemaProperty.Description = parameter.Description;

        inputSchema.Properties[parameter.Name] = schemaProperty;

        if (parameter.Required)
        {
            inputSchema.Required.Add(parameter.Name);
        }
    }

    /// <summary>
    /// Adds a request body to an input schema.
    /// </summary>
    /// <param name="inputSchema">The input schema to add the request body to.</param>
    /// <param name="requestBody">The request body to add.</param>
    private void AddRequestBodyToSchema(McpInputSchema inputSchema, OpenApiRequestBody requestBody)
    {
        // For simplicity, we'll just use the first content type (usually application/json)
        var content = requestBody.Content.FirstOrDefault();
        if (content.Value == null)
            return;

        var bodySchema = content.Value.Schema;
        if (bodySchema == null)
            return;

        // If the body is an object with properties, add each property to the input schema
        if (bodySchema.Type == "object" && bodySchema.Properties != null)
        {
            foreach (var property in bodySchema.Properties)
            {
                var schemaProperty = ConvertOpenApiSchemaToMcpSchemaProperty(property.Value);
                inputSchema.Properties[property.Key] = schemaProperty;

                if (bodySchema.Required.Contains(property.Key))
                {
                    inputSchema.Required.Add(property.Key);
                }
            }
        }
        else
        {
            // If the body is not an object with properties, add it as a single "body" parameter
            var schemaProperty = ConvertOpenApiSchemaToMcpSchemaProperty(bodySchema);
            schemaProperty.Description = requestBody.Description;

            inputSchema.Properties["body"] = schemaProperty;

            if (requestBody.Required)
            {
                inputSchema.Required.Add("body");
            }
        }
    }

    /// <summary>
    /// Converts an OpenAPI schema to an MCP schema property with support for OpenAPI 3.1.0 features.
    /// </summary>
    /// <param name="schema">The OpenAPI schema to convert.</param>
    /// <returns>An MCP schema property.</returns>
    private McpSchemaProperty ConvertOpenApiSchemaToMcpSchemaProperty(OpenApiSchema schema)
    {
        var schemaProperty = new McpSchemaProperty
        {
            Type = schema.Type ?? "string",
            Description = schema.Description
        };

        // Handle reference - OpenAPI 3.1 allows references alongside other properties
        if (schema.Reference != null && !string.IsNullOrEmpty(schema.Reference.Id))
        {
            string refPath = "#/components/schemas/" + schema.Reference.Id;
            if (!string.IsNullOrEmpty(schema.Reference.ExternalResource))
            {
                refPath = schema.Reference.ExternalResource + "#/components/schemas/" + schema.Reference.Id;
            }
            schemaProperty.Ref = refPath;
        }

        // Handle nullable property (OpenAPI 3.1)
        if (schema.Nullable)
        {
            schemaProperty.Nullable = true;
        }

        // Handle format
        if (!string.IsNullOrEmpty(schema.Format))
        {
            schemaProperty.Format = schema.Format;
        }

        // Handle enum values
        if (schema.Enum != null && schema.Enum.Any())
        {
            schemaProperty.Enum = new List<string>();
            
            foreach (var enumValue in schema.Enum)
            {
                // Handle different OpenApi enum value types
                if (enumValue is Microsoft.OpenApi.Any.OpenApiString stringValue)
                {
                    schemaProperty.Enum.Add(stringValue.Value);
                }
                else if (enumValue is Microsoft.OpenApi.Any.OpenApiInteger intValue)
                {
                    schemaProperty.Enum.Add(intValue.Value.ToString());
                }
                else if (enumValue is Microsoft.OpenApi.Any.OpenApiLong longValue)
                {
                    schemaProperty.Enum.Add(longValue.Value.ToString());
                }
                else if (enumValue is Microsoft.OpenApi.Any.OpenApiFloat floatValue)
                {
                    schemaProperty.Enum.Add(floatValue.Value.ToString());
                }
                else if (enumValue is Microsoft.OpenApi.Any.OpenApiDouble doubleValue)
                {
                    schemaProperty.Enum.Add(doubleValue.Value.ToString());
                }
                else if (enumValue is Microsoft.OpenApi.Any.OpenApiBoolean boolValue)
                {
                    schemaProperty.Enum.Add(boolValue.Value.ToString().ToLower());
                }
                else if (enumValue != null)
                {
                    // Fallback for other types
                    var valueStr = enumValue.ToString();
                    if (!string.IsNullOrEmpty(valueStr) && !valueStr.StartsWith("Microsoft.OpenApi"))
                    {
                        schemaProperty.Enum.Add(valueStr);
                    }
                }
            }
            
            if (!schemaProperty.Enum.Any())
            {
                schemaProperty.Enum = null;
            }
        }

        // Handle array types
        if (schema.Type == "array" && schema.Items != null)
        {
            schemaProperty.Items = ConvertOpenApiSchemaToMcpSchemaProperty(schema.Items);
        }

        // Handle object types
        if (schema.Type == "object" && schema.Properties != null && schema.Properties.Any())
        {
            schemaProperty.Properties = new Dictionary<string, McpSchemaProperty>();
            schemaProperty.Required = new List<string>();

            foreach (var property in schema.Properties)
            {
                schemaProperty.Properties[property.Key] = ConvertOpenApiSchemaToMcpSchemaProperty(property.Value);

                if (schema.Required.Contains(property.Key))
                {
                    schemaProperty.Required.Add(property.Key);
                }
            }

            if (!schemaProperty.Required.Any())
            {
                schemaProperty.Required = null;
            }
        }
        
        // Handle OpenAPI 3.1 allOf, oneOf, anyOf composition
        if (schema.AllOf != null && schema.AllOf.Any())
        {
            schemaProperty.AllOf = new List<McpSchemaProperty>();
            foreach (var subSchema in schema.AllOf)
            {
                schemaProperty.AllOf.Add(ConvertOpenApiSchemaToMcpSchemaProperty(subSchema));
            }
        }
        
        if (schema.OneOf != null && schema.OneOf.Any())
        {
            schemaProperty.OneOf = new List<McpSchemaProperty>();
            foreach (var subSchema in schema.OneOf)
            {
                schemaProperty.OneOf.Add(ConvertOpenApiSchemaToMcpSchemaProperty(subSchema));
            }
        }
        
        if (schema.AnyOf != null && schema.AnyOf.Any())
        {
            schemaProperty.AnyOf = new List<McpSchemaProperty>();
            foreach (var subSchema in schema.AnyOf)
            {
                schemaProperty.AnyOf.Add(ConvertOpenApiSchemaToMcpSchemaProperty(subSchema));
            }
        }
        
        // Handle OpenAPI 3.1 discriminator
        if (schema.Discriminator != null)
        {
            schemaProperty.Discriminator = new McpDiscriminator
            {
                PropertyName = schema.Discriminator.PropertyName
            };
            
            if (schema.Discriminator.Mapping != null && schema.Discriminator.Mapping.Any())
            {
                schemaProperty.Discriminator.Mapping = new Dictionary<string, string>();
                foreach (var mapping in schema.Discriminator.Mapping)
                {
                    schemaProperty.Discriminator.Mapping[mapping.Key] = mapping.Value;
                }
            }
        }

        return schemaProperty;
    }

    /// <summary>
    /// Creates annotations for an MCP tool from an OpenAPI operation.
    /// </summary>
    /// <param name="operation">The OpenAPI operation.</param>
    /// <param name="path">The path of the operation.</param>
    /// <param name="httpMethod">The HTTP method of the operation.</param>
    /// <param name="servers">The servers from the OpenAPI document.</param>
    /// <returns>Annotations for the MCP tool.</returns>
    private McpAnnotations CreateAnnotations(OpenApiOperation operation, string path, string httpMethod, IList<OpenApiServer> servers)
    {
        var annotations = new McpAnnotations();

        // Add security requirements
        if (operation.Security != null && operation.Security.Any())
        {
            annotations.Security = new List<Dictionary<string, List<string>>>();

            foreach (var securityRequirement in operation.Security)
            {
                var requirement = new Dictionary<string, List<string>>();

                foreach (var item in securityRequirement)
                {
                    requirement[item.Key.Reference.Id] = item.Value.ToList();
                }

                annotations.Security.Add(requirement);
            }
        }

        // Add tags
        if (operation.Tags != null && operation.Tags.Any())
        {
            annotations.Tags = operation.Tags.Select(t => t.Name).ToList();
        }

        // Add responses with enhanced information
        if (operation.Responses != null && operation.Responses.Any())
        {
            annotations.Responses = new Dictionary<string, McpResponseInfo>();

            foreach (var response in operation.Responses)
            {
                var responseInfo = new McpResponseInfo
                {
                    Description = response.Value.Description ?? string.Empty
                };

                // Add content types and schemas if available
                if (response.Value.Content != null && response.Value.Content.Any())
                {
                    responseInfo.Content = new Dictionary<string, object>();
                    
                    foreach (var content in response.Value.Content)
                    {
                        var contentInfo = new Dictionary<string, object>();
                        
                        if (content.Value.Schema != null)
                        {
                            // Add a simplified schema representation
                            contentInfo["schema"] = new Dictionary<string, object>
                            {
                                { "type", content.Value.Schema.Type ?? "object" }
                            };
                            
                            if (!string.IsNullOrEmpty(content.Value.Schema.Format))
                                ((Dictionary<string, object>)contentInfo["schema"])["format"] = content.Value.Schema.Format;
                                
                            if (!string.IsNullOrEmpty(content.Value.Schema.Description))
                                ((Dictionary<string, object>)contentInfo["schema"])["description"] = content.Value.Schema.Description;
                        }
                        
                        responseInfo.Content[content.Key] = contentInfo;
                    }
                }

                // Add headers if available
                if (response.Value.Headers != null && response.Value.Headers.Any())
                {
                    responseInfo.Headers = new Dictionary<string, object>();
                    
                    foreach (var header in response.Value.Headers)
                    {
                        responseInfo.Headers[header.Key] = new Dictionary<string, object>
                        {
                            { "description", header.Value.Description ?? string.Empty }
                        };
                    }
                }

                annotations.Responses[response.Key] = responseInfo;
            }
        }

        // Add servers
        if (servers != null && servers.Any())
        {
            annotations.Servers = servers.Select(s => new McpServerInfo
            {
                Url = s.Url,
                Description = s.Description
            }).ToList();
        }

        // Add path and method information with enhanced details
        var openApiOriginal = new Dictionary<string, JToken>
        {
            ["path"] = JToken.FromObject(path),
            ["method"] = JToken.FromObject(httpMethod)
        };
        
        // Add operation ID if available
        if (!string.IsNullOrEmpty(operation.OperationId))
            openApiOriginal["operationId"] = JToken.FromObject(operation.OperationId);
            
        // Add deprecated flag if true
        if (operation.Deprecated)
            openApiOriginal["deprecated"] = JToken.FromObject(true);
            
        // Add external docs if available
        if (operation.ExternalDocs != null && operation.ExternalDocs.Url != null)
        {
            openApiOriginal["externalDocs"] = JToken.FromObject(new Dictionary<string, string>
            {
                { "url", operation.ExternalDocs.Url.ToString() },
                { "description", operation.ExternalDocs.Description ?? string.Empty }
            });
        }
        
        // Add extensions if available
        if (operation.Extensions != null && operation.Extensions.Any())
        {
            foreach (var extension in operation.Extensions)
            {
                openApiOriginal[extension.Key] = JToken.FromObject(extension.Value);
            }
        }

        annotations.OpenApiOriginal = openApiOriginal;

        return annotations;
    }

    /// <summary>
    /// Creates an OAuth flow object from an OpenApiOAuthFlow.
    /// </summary>
    /// <param name="flow">The OpenApiOAuthFlow to convert.</param>
    /// <returns>A dictionary representing the OAuth flow.</returns>
    private Dictionary<string, object> CreateOAuthFlowObject(Microsoft.OpenApi.Models.OpenApiOAuthFlow flow)
    {
        var result = new Dictionary<string, object>();
        
        if (flow.AuthorizationUrl != null)
            result["authorizationUrl"] = flow.AuthorizationUrl.ToString();
            
        if (flow.TokenUrl != null)
            result["tokenUrl"] = flow.TokenUrl.ToString();
            
        if (flow.RefreshUrl != null)
            result["refreshUrl"] = flow.RefreshUrl.ToString();
            
        if (flow.Scopes != null && flow.Scopes.Any())
        {
            var scopes = new Dictionary<string, string>();
            foreach (var scope in flow.Scopes)
            {
                scopes[scope.Key] = scope.Value;
            }
            result["scopes"] = scopes;
        }
        
        return result;
    }

    /// <summary>
    /// Serializes an MCP context to JSON.
    /// </summary>
    /// <param name="mcpContext">The MCP context to serialize.</param>
    /// <returns>The serialized JSON.</returns>
    public string SerializeToJson(McpContext mcpContext)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        return JsonConvert.SerializeObject(mcpContext, settings);
    }

    /// <summary>
    /// Writes an MCP context to a file.
    /// </summary>
    /// <param name="mcpContext">The MCP context to write.</param>
    /// <param name="outputFilePath">The path to write the MCP context to.</param>
    public void WriteToFile(McpContext mcpContext, string outputFilePath)
    {
        var json = SerializeToJson(mcpContext);
        File.WriteAllText(outputFilePath, json);
    }
}
