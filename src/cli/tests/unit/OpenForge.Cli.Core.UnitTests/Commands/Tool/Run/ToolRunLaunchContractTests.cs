using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Run;

[Trait("Feature", "tool-run"), Trait("Evidence", "Unit")]
public sealed class ToolRunLaunchContractTests
{
    [Theory(DisplayName = "Launch records distinguish verified MCP argument vectors from ordinary CLI input and keep old JSON valid")]
    [InlineData(false)]
    [InlineData(true)]
    public void RoundTripsMcpLifetime(bool mcp)
    {
        var launch = new ToolLaunchBinding
        {
            ToolId = ToolFixtureSource.Id,
            Version = ToolFixtureSource.Version,
            ManifestSha256 = "",
            StoreId = "",
            Executable = ToolFixtureSource.ArtifactName,
            PrefixArguments = [],
            Environment = [],
            Files = [],
            McpArguments = mcp ? [["serve", "space value"], []] : null,
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(launch, ToolStoreJsonContext.Default.ToolLaunchBinding);
        var decoded = Assert.IsType<ToolLaunchBinding>(JsonSerializer.Deserialize(bytes, ToolStoreJsonContext.Default.ToolLaunchBinding));
        if (mcp)
        {
            Assert.NotNull(decoded.McpArguments);
            Assert.Equal(launch.McpArguments!.Length, decoded.McpArguments.Length);
            for (var index = 0; index < decoded.McpArguments.Length; index++) Assert.Equal(launch.McpArguments[index], decoded.McpArguments[index]);
        }
        else
        {
            Assert.Null(decoded.McpArguments);
            using var document = JsonDocument.Parse(bytes);
            Assert.False(document.RootElement.TryGetProperty(JsonNamingPolicy.CamelCase.ConvertName(nameof(ToolLaunchBinding.McpArguments)), out _));
        }
    }
}
