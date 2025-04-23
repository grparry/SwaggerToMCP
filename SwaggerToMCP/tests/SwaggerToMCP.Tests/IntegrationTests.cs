using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Xunit;
using SwaggerToMCP.Models;

namespace SwaggerToMCP.Tests;

public class IntegrationTests
{
    [Fact]
    public async Task EndToEnd_ConvertPetStoreToMcp_Success()
    {
        // Arrange
        var inputPath = Path.Combine("TestData", "PetStore.json");
        var outputPath = Path.Combine(Path.GetTempPath(), "McpContext_Integration.json");
        
        // Clean up any existing output file
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }
        
        try
        {
            // Act
            var converter = new SwaggerToMcpConverter();
            var mcpContext = await converter.ConvertFromFileAsync(inputPath);
            converter.WriteToFile(mcpContext, outputPath);
            
            // Assert
            Assert.True(File.Exists(outputPath));
            var fileContent = File.ReadAllText(outputPath);
            Assert.NotEmpty(fileContent);
            
            // Verify the content is valid JSON and contains the expected tools
            var deserializedContext = JsonConvert.DeserializeObject<McpContext>(fileContent);
            Assert.NotNull(deserializedContext);
            Assert.NotNull(deserializedContext.Tools);
            Assert.Equal(2, deserializedContext.Tools.Count); // PetStore.json has 2 operations
            
            // Verify specific tools exist
            var tools = deserializedContext?.Tools;
            Assert.NotNull(tools);
            Assert.Contains(tools, t => t != null && t.Name == "getPetById");
            Assert.Contains(tools, t => t != null && t.Name == "addPet");
            
            // Verify metadata
            var metadata = deserializedContext.Metadata;
            Assert.NotNull(metadata);
            Assert.Equal("Swagger Petstore", metadata.Title);
            Assert.Equal("1.0.0", metadata.Version);
        }
        finally
        {
            // Clean up
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }
    
    [Fact]
    public void JsonFormat_CompliesWithMcpSchema()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        var mcpContext = new McpContext
        {
            Metadata = new McpMetadata
            {
                Title = "Test API",
                Description = "A test API",
                Version = "1.0.0"
            },
            Tools = new List<McpTool>
            {
                new McpTool
                {
                    Name = "testOperation",
                    Description = "A test operation",
                    InputSchema = new McpInputSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, McpSchemaProperty>
                        {
                            ["id"] = new McpSchemaProperty { Type = "string", Description = "The ID" }
                        },
                        Required = new List<string> { "id" }
                    },
                    Annotations = new McpAnnotations
                    {
                        Tags = new List<string> { "test" }
                    }
                }
            }
        };
        
        // Act
        var json = converter.SerializeToJson(mcpContext);
        
        // Assert
        // Parse the JSON to verify it has the expected structure
        var jObject = JObject.Parse(json);
        
        // Verify tools array exists and has the correct structure
        Assert.True(jObject.ContainsKey("tools"));
        var toolsArray = jObject["tools"];
        Assert.NotNull(toolsArray);
        Assert.True(toolsArray.Type == JTokenType.Array);
        
        // Verify the first tool has the expected properties with correct casing
        var toolsToken = jObject["tools"];
        Assert.NotNull(toolsToken);
        Assert.True(toolsToken.Type == JTokenType.Array);
        Assert.True(toolsToken.Count() > 0);
        
        var tool = toolsToken[0];
        Assert.NotNull(tool);
        Assert.True(tool["name"] != null);
        Assert.True(tool["description"] != null);
        Assert.True(tool["inputSchema"] != null);
        
        // Verify inputSchema has the expected properties
        var inputSchema = tool["inputSchema"];
        Assert.NotNull(inputSchema);
        Assert.True(inputSchema["type"] != null);
        Assert.True(inputSchema["properties"] != null);
        Assert.True(inputSchema["required"] != null);
        
        // Verify metadata has the expected properties
        Assert.True(jObject.ContainsKey("metadata"));
        var metadata = jObject["metadata"];
        Assert.NotNull(metadata);
        Assert.True(metadata["title"] != null);
        Assert.True(metadata["description"] != null);
        Assert.True(metadata["version"] != null);
    }
}
