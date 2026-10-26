using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Prompts;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
public sealed class WorkspacePermissionPromptTests
{
    private const string ContextFolder = "docs/forge";
    private const string Owner = "toolkit";
    private const string LongOwner = "catalogue/extensions/development-toolkit";
    private const string Destination = "external";
    private const string OnceAnswer = "once";
    private const string SnapshotName = "permission.custom-context";

    [Fact]
    public async Task LinePromptUsesProjectedContextFolder()
    {
        var terminal = ScriptedCliTerminal.Lines([OnceAnswer]);
        var reply = await new CliPrompts(terminal.Terminal).PermissionAsync(Question(Owner), new(true), CancellationToken.None);
        Assert.Equal(CliPermissionChoice.Once, reply.Value);
        CommandOutputSnapshot.MatchSnapshot(terminal.Output.ToString(), SnapshotName);
    }

    [Theory]
    [InlineData(Owner), InlineData(LongOwner)]
    public async Task KeyPromptUsesProjectedContextFolder(string owner)
    {
        var terminal = ScriptedCliTerminal.Keys([new(CliKey.Enter)]);
        var reply = await new CliPrompts(terminal.Terminal).PermissionAsync(Question(owner), new(true), CancellationToken.None);
        Assert.Equal(CliPermissionChoice.Once, reply.Value);
        CommandOutputSnapshot.MatchSnapshot(Assert.Single(terminal.Frames), owner);
    }

    [Fact]
    public void FindingUsesProjectedContextFolder()
        => CommandOutputSnapshot.MatchSnapshot(CliFindingWording.PermissionRequired(Owner, ContextFolder), SnapshotName);

    private static CliPermissionQuestion Question(string owner)
        => new(owner, [new(Destination, true)]) { ContextFolder = ContextFolder };
}
