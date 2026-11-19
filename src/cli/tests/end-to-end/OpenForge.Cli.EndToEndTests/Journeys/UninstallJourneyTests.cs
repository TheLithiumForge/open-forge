using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;

using Tokens = OpenForge.Cli.TestSupport.Contracts.PublishedTokens;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class UninstallJourneyTests
{
    private const string Feature = "uninstall";
    private const string NotePath = Tokens.Sources.AgentsRoot + "/notes/project.md";
    private const string NoteBody = "# Project\n\nKeep every word.\n";
    private const string NoteMetadata = "---\nopen-forge:\n  description: Project note\n  tags: [Project]\n---\n";

    [Fact(DisplayName = "Published uninstall removes the installed Framework, strips the note that stays and repeats as a no-op")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task UninstallLeavesPlainNotes()
    {
        using var workspace = PublishedJourneyWorkspace.Create("uninstall-journey");
        Assert.Equal(Tokens.Statuses.CompleteExitCode, (await workspace.RunAsync(Tokens.Install.Command, Tokens.Install.Preset, Tokens.Install.MinimalPreset, Tokens.Install.Automatic)).ExitCode);
        workspace.WriteText(NotePath, NoteMetadata + NoteBody);

        var applied = await workspace.RunAsync(Tokens.Uninstall.Command, Tokens.Uninstall.Strip, Tokens.Uninstall.MetadataPart, Tokens.Uninstall.Automatic);

        Assert.Equal(Tokens.Statuses.CompleteExitCode, applied.ExitCode);
        Assert.Equal(NoteBody, File.ReadAllText(workspace.Combine(NotePath)));
        Assert.False(File.Exists(workspace.Combine(Tokens.Sources.LoaderPath)));
        var repeated = await workspace.RunAsync(Tokens.Uninstall.Command, Tokens.Uninstall.Strip, Tokens.Uninstall.MetadataPart, Tokens.Uninstall.Automatic, Tokens.Syntax.FormatOption, Tokens.Syntax.JsonFormat);
        Assert.Equal(Tokens.Statuses.CompleteExitCode, repeated.ExitCode);
        using var json = JsonDocument.Parse(repeated.StandardOutput);
        Assert.Empty(json.RootElement.GetProperty(Tokens.Json.Effects).EnumerateArray());
    }
}
