using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Reading;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Filesystem.Shared.Reading;

public sealed class ManagedTargetReaderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Managed target reads retain contained content and physical boundary outcomes"),
        InlineData("present", (int)ManagedTargetReadState.Available),
        InlineData("missing", (int)ManagedTargetReadState.Missing),
        InlineData("directory", (int)ManagedTargetReadState.Unavailable),
        InlineData("outside", (int)ManagedTargetReadState.Blocked),
        InlineData("cancelled", (int)ManagedTargetReadState.Cancelled)]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ReadsReportPhysicalBoundaries(string scenario, int expected)
    {
        using var temporary = TemporaryWorkspace.Create("managed-target-read");
        if (scenario == "present") temporary.CreateFile(".agents/loader.md", "current content\n"u8.ToArray());
        if (scenario == "directory") temporary.CreateDirectory(".agents/loader.md");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        using var cancellation = new CancellationTokenSource();
        if (scenario == "cancelled") cancellation.Cancel();
        var result = await new ManagedTargetReader(new PhysicalPathResolver()).ReadAsync(workspace,
            scenario == "outside" ? "../outside.md" : ".agents/loader.md", cancellation.Token);
        Assert.Equal((ManagedTargetReadState)expected, result.State);
        if (scenario == "present") Assert.Equal("current content\n"u8.ToArray(), result.Bytes.ToArray());
        else Assert.True(result.Bytes.IsEmpty);
    }
}
