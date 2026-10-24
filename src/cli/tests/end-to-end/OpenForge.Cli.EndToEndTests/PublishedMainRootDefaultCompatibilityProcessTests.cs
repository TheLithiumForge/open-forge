using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.MainRoot;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

[Trait("Feature", "install-main-root"), Trait("Evidence", "EndToEnd")]
public sealed class PublishedMainRootDefaultCompatibilityProcessTests
{
    [Theory(DisplayName = "Omitted and explicit default roots preserve pre-activation paths bytes ownership regions and output")]
    [InlineData(false), InlineData(true)]
    public async Task ExactDefaultCompatibility(bool explicitRoot)
    {
        using var workspace = new PublishedMainRootWorkspace(PublishedTokens.Sources.AgentsRoot);
        using var baseline = new DefaultLayoutBaseline();
        string[] arguments = [PublishedTokens.Install.Command, PublishedTokens.Install.Automatic];
        if (explicitRoot) arguments = [.. arguments, PublishedTokens.Install.MainRoot, PublishedTokens.Sources.AgentsRoot];
        var installed = await workspace.RunRaw(arguments);
        Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, installed.ExitCode);
        Assert.Empty(installed.StandardError);
        workspace.AssertBytes(baseline.Files);
        Assert.Equal(baseline.Text, Encoding.UTF8.GetBytes(workspace.Normalize(installed.StandardOutput)));
        var noOp = await workspace.RunRaw([.. arguments, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);
        Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, noOp.ExitCode);
        Assert.Empty(noOp.StandardError);
        using var json = JsonDocument.Parse(workspace.Normalize(noOp.StandardOutput));
        Assert.True(JsonElement.DeepEquals(baseline.Json, json.RootElement));
        workspace.AssertBytes(baseline.Files);
        using var settings = JsonDocument.Parse(workspace.Read(PublishedTokens.Settings.RelativePath));
        Assert.False(settings.RootElement.TryGetProperty(PublishedTokens.Settings.MainRootProperty, out _));
    }
}
