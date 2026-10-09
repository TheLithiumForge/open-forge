using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Create.Shared.Prompts;

public sealed class ExtensionCreatePromptsTests
{
    [Theory(DisplayName = "Extension ID text input explains the validator on line and key-capable hosts")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task IdScreens(bool keys)
    {
        var script = new ScriptedCliTerminal(new(true, keys, keys), lines: ["", "BAD ID", "my-tools"]);
        var question = new CliTextQuestion<string>(ExtensionCreateWording.ExtensionId(), ExtensionCreateWording.ExtensionIdRule(), Required: true,
            value => ExtensionIdentity.IsValidStableId(value) ? new(true, value, null) : new(false, null, ExtensionCreateWording.InvalidExtensionId(value)));
        var reply = await new CliPrompts(script.Terminal).TextAsync(question, new(true), CancellationToken.None);
        Assert.Equal("my-tools", reply.Value);
        Assert.Equal(0, script.KeyReadCalls);
        CommandOutputSnapshot.MatchSnapshot(script.Output.ToString(), keys ? "id.key-host.80x24" : "id.line");
    }

    [Theory(DisplayName = "Extension parent-folder text input explains child creation and how to retry")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task FolderScreens(bool keys)
    {
        var script = new ScriptedCliTerminal(new(true, keys, keys), lines: ["", "missing", "catalogue"]);
        var question = new CliTextQuestion<string>(ExtensionCreateWording.PackageFolder(), ExtensionCreateWording.PackageFolderRule(), Required: true,
            value => value == "catalogue" ? new(true, value, null) : new(false, null, ExtensionCreateWording.InvalidPackageFolder()));
        var reply = await new CliPrompts(script.Terminal).TextAsync(question, new(true), CancellationToken.None);
        Assert.Equal("catalogue", reply.Value);
        Assert.Equal(0, script.KeyReadCalls);
        CommandOutputSnapshot.MatchSnapshot(script.Output.ToString(), keys ? "folder.key-host.80x24" : "folder.line");
    }
}
