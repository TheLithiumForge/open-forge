using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Remove;

public sealed class RemoveRootChangeOutputTests
{
    [Theory(DisplayName = "Root Remove confirms the whole plan with the actual deletion count in both input modes")]
    [InlineData(true, -1), InlineData(false, -1)]
    [InlineData(true, 1), InlineData(false, 1)]
    [InlineData(true, 0), InlineData(false, 0)]
    [InlineData(true, 3), InlineData(false, 3)]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task ConfirmationScreens(bool keys, int files)
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-confirmation-output");
        var target = files is -1 or 1 ? "old.txt" : "old";
        if (files == 1)
        {
            workspace.WriteText(target, "Remove this file.\n");
        }
        else if (files >= 0)
        {
            workspace.CreateDirectory(target);
            for (var index = 0; index < files; index++)
            {
                workspace.WriteText($"old/{index}.txt", "Remove this file.\n");
            }
        }
        var before = workspace.SnapshotHashes();
        var terminal = keys
            ? ScriptedCliTerminal.Keys([new(CliKey.Character, 'n')])
            : ScriptedCliTerminal.Lines(["no"]);
        var output = new StringWriter();
        var error = new StringWriter();
        var result = await workspace.RunAsync(["remove", target], output, error, terminal: terminal.Terminal);
        Assert.Equal(130, result.ExitCode);
        Assert.Equal(before, workspace.SnapshotHashes());
        var question = files <= 0
            ? "Apply these changes? [y/N]"
            : $"Apply these changes, including deleting {files} {(files == 1 ? "file" : "files")}? [y/N]";
        var screen = terminal.Output.ToString();
        Assert.Contains(question, screen, StringComparison.Ordinal);
        Assert.Contains(".agents/open-forge.json  would be created", screen, StringComparison.Ordinal);
        Assert.DoesNotContain("replaced", screen, StringComparison.Ordinal);
        Assert.True(screen.IndexOf("would be created", StringComparison.Ordinal) < screen.IndexOf(question, StringComparison.Ordinal),
            "The plan review must precede the confirmation question.");
    }

    [Theory(DisplayName = "Root Remove labels existing settings updates once in dry-run and applied text")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task ExistingSettings(bool preview)
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-settings-output");
        workspace.WriteText(".agents/open-forge.json", "{\"schemaVersion\":1,\"removedFiles\":[]}");
        workspace.WriteText("old.txt", "Remove this file.\n");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();
        var arguments = new List<string> { "remove", "old.txt", "--automatic", "--detail", "standard" };
        if (preview) arguments.Add("--dry-run");
        var result = await workspace.RunAsync(arguments, output, error);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, error.ToString());
        Assert.Equal(preview, File.Exists(workspace.Combine("old.txt")));
        if (preview) Assert.Equal(before, workspace.SnapshotHashes());
        var text = output.ToString().Replace(workspace.Path, "<workspace>", StringComparison.Ordinal);
        Assert.Equal(1, text.Split(".agents/open-forge.json", StringSplitOptions.None).Length - 1);
        Assert.Contains(preview ? ".agents/open-forge.json  settings would be updated" : ".agents/open-forge.json  settings updated", text, StringComparison.Ordinal);
        Assert.DoesNotContain("replaced", text, StringComparison.Ordinal);
    }
}
