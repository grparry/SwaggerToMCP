using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using SwaggerToMCP;
using SwaggerToMCP.Models;

namespace SwaggerToMCP.Tests;

public class EdgeCaseTests
{
    [Fact]
    public async Task ConvertFromFile_EmptyPaths_ReturnsEmptyToolsList()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        var testFilePath = Path.Combine("TestData", "EmptyPaths.json");
        
        // Act
        var mcpContext = await converter.ConvertFromFileAsync(testFilePath);
        
        // Assert
        Assert.NotNull(mcpContext);
        Assert.NotNull(mcpContext.Tools);
        Assert.Empty(mcpContext.Tools);
        Assert.NotNull(mcpContext.Metadata);
        Assert.Equal("Empty API", mcpContext.Metadata.Title);
    }
    
    [Fact]
    public async Task ConvertFromFile_MissingOperationId_SkipsOperationsWithoutId()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        var testFilePath = Path.Combine("TestData", "MissingOperationId.json");
        
        // Act
        var mcpContext = await converter.ConvertFromFileAsync(testFilePath);
        
        // Assert
        Assert.NotNull(mcpContext);
        Assert.NotNull(mcpContext.Tools);
        Assert.Single(mcpContext.Tools);
        
        // Only the operation with an operationId should be included
        var tool = mcpContext.Tools[0];
        Assert.Equal("createTest", tool.Name);
        Assert.Equal("Create test", tool.Description);
    }
    
    [Fact]
    public async Task ConvertFromFile_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        var nonExistentFilePath = Path.Combine("TestData", "NonExistent.json");
        
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => 
            converter.ConvertFromFileAsync(nonExistentFilePath));
    }
    
    [Fact]
    public void WriteToFile_InvalidPath_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var converter = new SwaggerToMcpConverter();
        var mcpContext = new McpContext
        {
            Metadata = new McpMetadata { Title = "Test API", Version = "1.0.0" },
            Tools = new List<McpTool> { new McpTool { Name = "test", Description = "Test operation" } }
        };
        var invalidPath = Path.Combine("NonExistentDirectory", "output.json");
        
        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => 
            converter.WriteToFile(mcpContext, invalidPath));
    }
}
