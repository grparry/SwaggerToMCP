using System.CommandLine;
using SwaggerToMCP;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Create command-line options
        var inputOption = new Option<string>(
            aliases: new[] { "--input", "-i" },
            description: "Path to the input Swagger/OpenAPI JSON file")
        {
            IsRequired = true
        };

        var outputOption = new Option<string>(
            aliases: new[] { "--output", "-o" },
            description: "Path to the output MCP JSON file")
        {
            IsRequired = true
        };

        // Create root command
        var rootCommand = new RootCommand("Converts Swagger/OpenAPI JSON to Anthropic's Model Context Protocol (MCP) format")
        {
            inputOption,
            outputOption
        };

        // Set the handler for the root command
        rootCommand.SetHandler(async (string input, string output) =>
        {
            try
            {
                Console.WriteLine($"Converting {input} to {output}...");

                // Validate input file exists
                if (!File.Exists(input))
                {
                    Console.Error.WriteLine($"Error: Input file '{input}' does not exist.");
                    Environment.Exit(1);
                }

                // Create output directory if it doesn't exist
                var outputDir = Path.GetDirectoryName(output);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Convert the OpenAPI document to MCP
                var converter = new SwaggerToMcpConverter();
                var mcpContext = await converter.ConvertFromFileAsync(input);

                // Write the MCP context to the output file
                converter.WriteToFile(mcpContext, output);

                Console.WriteLine($"Successfully converted {input} to {output}.");
                Console.WriteLine($"Generated {mcpContext.Tools.Count} MCP tools.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine($"Inner Error: {ex.InnerException.Message}");
                }
                Environment.Exit(1);
            }
        }, inputOption, outputOption);

        // Parse the command line arguments and invoke the handler
        return await rootCommand.InvokeAsync(args);
    }
}
