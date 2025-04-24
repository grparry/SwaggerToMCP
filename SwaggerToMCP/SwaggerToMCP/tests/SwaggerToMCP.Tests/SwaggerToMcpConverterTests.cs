using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using SwaggerToMCP;
using SwaggerToMCP.Models;
using System.IO;
using System.Threading.Tasks;

namespace SwaggerToMCP.Tests;

public class SwaggerToMcpConverterTests
{
    private readonly string _testFilePath = Path.Combine("TestData", "PetStore.json");

    [Fact]
    public async Task ConvertFromFile_ValidOpenApiJson_ReturnsMcpContext()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        
        // Act
        var mcpContext = await converter.ConvertFromFileAsync(_testFilePath);
        
        // Assert
        Assert.NotNull(mcpContext);
        Assert.NotNull(mcpContext.Tools);
        Assert.NotEmpty(mcpContext.Tools);
        Assert.NotNull(mcpContext.Metadata);
        Assert.Equal("Swagger Petstore", mcpContext.Metadata.Title);
    }
    
    [Fact]
    public async Task ConvertFromFile_ValidOpenApiJson_PreservesMetadata()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        
        // Act
        var mcpContext = await converter.ConvertFromFileAsync(_testFilePath);
        
        // Assert
        Assert.NotNull(mcpContext.Metadata);
        Assert.Equal("Swagger Petstore", mcpContext.Metadata.Title);
        Assert.Equal("This is a sample server Petstore server.", mcpContext.Metadata.Description);
        Assert.Equal("1.0.0", mcpContext.Metadata.Version);
        
        // Check contact information
        Assert.NotNull(mcpContext.Metadata.Contact);
        Assert.True(mcpContext.Metadata.Contact.ContainsKey("email"));
        Assert.Equal("apiteam@swagger.io", mcpContext.Metadata.Contact["email"]);
        
        // Check license information
        Assert.NotNull(mcpContext.Metadata.License);
        Assert.True(mcpContext.Metadata.License.ContainsKey("name"));
        Assert.Equal("Apache 2.0", mcpContext.Metadata.License["name"]);
        Assert.True(mcpContext.Metadata.License.ContainsKey("url"));
        Assert.Equal("http://www.apache.org/licenses/LICENSE-2.0.html", mcpContext.Metadata.License["url"]);
        
        // Check terms of service
        Assert.Equal("http://swagger.io/terms/", mcpContext.Metadata.TermsOfService);
    }
    
    [Fact]
    public async Task ConvertFromFile_ValidOpenApiJson_CorrectlyMapsPetByIdOperation()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        
        // Act
        var mcpContext = await converter.ConvertFromFileAsync(_testFilePath);
        
        // Assert
        var getPetByIdTool = mcpContext.Tools.FirstOrDefault(t => t.Name == "getPetById");
        Assert.NotNull(getPetByIdTool);
        
        // Check basic properties
        Assert.Equal("Find pet by ID", getPetByIdTool.Description);
        
        // Check input schema
        Assert.NotNull(getPetByIdTool.InputSchema);
        Assert.Equal("object", getPetByIdTool.InputSchema.Type);
        Assert.True(getPetByIdTool.InputSchema.Properties.ContainsKey("petId"));
        Assert.Equal("integer", getPetByIdTool.InputSchema.Properties["petId"].Type);
        Assert.Equal("int64", getPetByIdTool.InputSchema.Properties["petId"].Format);
        Assert.Equal("ID of pet to return", getPetByIdTool.InputSchema.Properties["petId"].Description);
        Assert.Contains("petId", getPetByIdTool.InputSchema.Required);
        
        // Check annotations
        Assert.NotNull(getPetByIdTool.Annotations);
        Assert.NotNull(getPetByIdTool.Annotations.Tags);
        Assert.Contains("pet", getPetByIdTool.Annotations.Tags);
        
        // Check security
        Assert.NotNull(getPetByIdTool.Annotations.Security);
        Assert.Single(getPetByIdTool.Annotations.Security);
        Assert.True(getPetByIdTool.Annotations.Security[0].ContainsKey("api_key"));
        
        // Check responses
        Assert.NotNull(getPetByIdTool.Annotations.Responses);
        Assert.True(getPetByIdTool.Annotations.Responses.ContainsKey("200"));
        Assert.Equal("successful operation", getPetByIdTool.Annotations.Responses["200"].Description);
        Assert.True(getPetByIdTool.Annotations.Responses.ContainsKey("404"));
        Assert.Equal("Pet not found", getPetByIdTool.Annotations.Responses["404"].Description);
        
        // Check original OpenAPI info
        Assert.NotNull(getPetByIdTool.Annotations.OpenApiOriginal);
        Assert.True(getPetByIdTool.Annotations.OpenApiOriginal.ContainsKey("path"));
        Assert.True(getPetByIdTool.Annotations.OpenApiOriginal.ContainsKey("method"));
    }
    
    [Fact]
    public async Task ConvertFromFile_ValidOpenApiJson_CorrectlyMapsAddPetOperation()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        
        // Act
        var mcpContext = await converter.ConvertFromFileAsync(_testFilePath);
        
        // Assert
        var addPetTool = mcpContext.Tools.FirstOrDefault(t => t.Name == "addPet");
        Assert.NotNull(addPetTool);
        
        // Check basic properties
        Assert.Equal("Add a new pet to the store", addPetTool.Description);
        
        // Check input schema - should have properties from the Pet schema
        Assert.NotNull(addPetTool.InputSchema);
        Assert.Equal("object", addPetTool.InputSchema.Type);
        
        // The request body should be mapped to input schema properties
        Assert.True(addPetTool.InputSchema.Properties.ContainsKey("name"));
        Assert.True(addPetTool.InputSchema.Properties.ContainsKey("photoUrls"));
        Assert.True(addPetTool.InputSchema.Properties.ContainsKey("status"));
        
        // Check required fields
        Assert.Contains("name", addPetTool.InputSchema.Required);
        Assert.Contains("photoUrls", addPetTool.InputSchema.Required);
        
        // Check annotations
        Assert.NotNull(addPetTool.Annotations);
        Assert.NotNull(addPetTool.Annotations.Tags);
        Assert.Contains("pet", addPetTool.Annotations.Tags);
        
        // Check security
        Assert.NotNull(addPetTool.Annotations.Security);
        Assert.Single(addPetTool.Annotations.Security);
        Assert.True(addPetTool.Annotations.Security[0].ContainsKey("petstore_auth"));
        Assert.Contains("write:pets", addPetTool.Annotations.Security[0]["petstore_auth"]);
        Assert.Contains("read:pets", addPetTool.Annotations.Security[0]["petstore_auth"]);
    }
    
    [Fact]
    public async Task ConvertFromFile_ValidOpenApiJson_IncludesServerInformation()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        
        // Act
        var mcpContext = await converter.ConvertFromFileAsync(_testFilePath);
        var tool = mcpContext.Tools.First();
        
        // Assert
        Assert.NotNull(tool.Annotations.Servers);
        Assert.Single(tool.Annotations.Servers);
        Assert.Equal("https://petstore.swagger.io/v1", tool.Annotations.Servers[0].Url);
    }
    
    [Fact]
    public void SerializeToJson_ValidMcpContext_ReturnsValidJson()
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
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        
        // Verify the JSON can be deserialized back to a valid object
        var deserializedContext = JsonConvert.DeserializeObject<McpContext>(json);
        
        Assert.NotNull(deserializedContext);
        Assert.NotNull(deserializedContext.Metadata);
        Assert.NotNull(deserializedContext.Tools);
        Assert.Equal(mcpContext.Metadata.Title, deserializedContext.Metadata.Title);
        Assert.Equal(mcpContext.Tools.Count, deserializedContext.Tools.Count);
    }
    
    [Fact]
    public async Task ConvertFromStream_ValidOpenApiJson_ReturnsMcpContext()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        using var fileStream = File.OpenRead(_testFilePath);
        
        // Act
        var mcpContext = await converter.ConvertFromStreamAsync(fileStream);
        
        // Assert
        Assert.NotNull(mcpContext);
        Assert.NotNull(mcpContext.Tools);
        Assert.NotEmpty(mcpContext.Tools);
    }
    
    [Fact]
    public void WriteToFile_ValidMcpContext_WritesJsonToFile()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        var mcpContext = new McpContext
        {
            Metadata = new McpMetadata { Title = "Test API", Version = "1.0.0" },
            Tools = new List<McpTool> { new McpTool { Name = "test", Description = "Test operation" } }
        };
        var tempFile = Path.GetTempFileName();
        
        try
        {
            // Act
            converter.WriteToFile(mcpContext, tempFile);
            
            // Assert
            Assert.True(File.Exists(tempFile));
            var fileContent = File.ReadAllText(tempFile);
            Assert.NotEmpty(fileContent);
            
            // Verify the content is valid JSON and can be deserialized
            var deserializedContext = JsonConvert.DeserializeObject<McpContext>(fileContent);
            Assert.NotNull(deserializedContext);
            Assert.NotNull(deserializedContext.Metadata);
            Assert.NotNull(deserializedContext.Tools);
            Assert.Equal(mcpContext.Metadata.Title, deserializedContext.Metadata.Title);
            Assert.Equal(mcpContext.Tools.Count, deserializedContext.Tools.Count);
        }
        finally
        {
            // Clean up
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
