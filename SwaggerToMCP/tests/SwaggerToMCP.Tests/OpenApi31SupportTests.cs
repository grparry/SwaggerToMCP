using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwaggerToMCP.Models;
using Xunit;
using Xunit.Abstractions;

namespace SwaggerToMCP.Tests;

/// <summary>
/// Tests to verify support for OpenAPI 3.1.0 features in the SwaggerToMCP converter.
/// </summary>
public class OpenApi31SupportTests
{
    private readonly ITestOutputHelper _output;

    public OpenApi31SupportTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ConvertFromFile_WithOpenApi31Features_CreatesCorrectMcpContext()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        var testFilePath = Path.Combine("TestData", "openapi31_test.json");
        
        // Ensure the test file exists
        Assert.True(File.Exists(testFilePath), $"Test file not found: {testFilePath}");
        
        // Act
        var mcpContext = await converter.ConvertFromFileAsync(testFilePath);
        
        // Write the output to help with debugging
        var json = JsonConvert.SerializeObject(mcpContext, Formatting.Indented);
        _output.WriteLine("Generated MCP context:");
        _output.WriteLine(json);
        
        // Assert
        Assert.NotNull(mcpContext);
        Assert.NotNull(mcpContext.Tools);
        Assert.Single(mcpContext.Tools);
        
        // Get the test31 tool
        var tool = mcpContext.Tools.Find(t => t.Name == "testOpenApi31Features");
        Assert.NotNull(tool);
        
        // Verify input schema properties
        Assert.NotNull(tool.InputSchema);
        Assert.NotNull(tool.InputSchema.Properties);
        
        // 1. Check for nullable type array (OpenAPI 3.1 feature)
        Assert.True(tool.InputSchema.Properties.ContainsKey("nullableField"));
        var nullableField = tool.InputSchema.Properties["nullableField"];
        Assert.Equal("string", nullableField.Type);
        Assert.True(nullableField.Nullable == true);
        
        // 2. Check anyOf property (OpenAPI 3.1 feature)
        Assert.True(tool.InputSchema.Properties.ContainsKey("mixedTypeField"));
        var mixedTypeField = tool.InputSchema.Properties["mixedTypeField"];
        Assert.NotNull(mixedTypeField.AnyOf);
        Assert.Equal(2, mixedTypeField.AnyOf.Count);
        
        // 3. Check oneOf with discriminator (OpenAPI 3.1 feature)
        Assert.True(tool.InputSchema.Properties.ContainsKey("discriminatedType"));
        var discriminatedType = tool.InputSchema.Properties["discriminatedType"];
        Assert.NotNull(discriminatedType.OneOf);
        Assert.Equal(2, discriminatedType.OneOf.Count);
        Assert.NotNull(discriminatedType.Discriminator);
        Assert.Equal("petType", discriminatedType.Discriminator.PropertyName);
        Assert.Equal(2, discriminatedType.Discriminator.Mapping.Count);
        
        // 4. Check for reference alongside properties (OpenAPI 3.1 feature)
        Assert.True(tool.InputSchema.Properties.ContainsKey("refWithProperties"));
        var refWithProps = tool.InputSchema.Properties["refWithProperties"];
        Assert.NotNull(refWithProps.Ref);
        Assert.NotNull(refWithProps.Description);
        
        _output.WriteLine("OpenAPI 3.1.0 support test passed!");
    }
}
