using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedExtensionInspectProcessTests
{
    [Fact(DisplayName = "Published Extension Inspect help and version bypass workspace selection"), Trait("Feature", "extension-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedHelpAndVersionAreTerminalAndReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInspectWorkspace.Create(
            fingerprintKind: "semantic",
            intendedContent: "alpha\n");
        var missingWorkspace = working.Combine("missing-workspace");
        var beforeSource = working.SnapshotSource();

        var help = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "extension", "inspect", "--help",
                "--workspace", missingWorkspace,
                "--json", "--view=expanded", "--verbose",
            ]);
        var version = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "extension", "inspect", "--version",
                "--workspace", missingWorkspace,
                "--json", "--view=expanded", "--verbose",
            ]);

        Assert.Equal(0, help.ExitCode);
        Assert.Equal(string.Empty, help.StandardError);
        Assert.Contains("open-forge extension inspect <stable-id>", help.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--source <package-or-catalogue-path>", help.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Results and streams", help.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(0, version.ExitCode);
        Assert.Equal(target.ExpectedVersion + Environment.NewLine, version.StandardOutput);
        Assert.Equal(string.Empty, version.StandardError);
        Assert.False(Directory.Exists(missingWorkspace));
        Assert.Equal(beforeSource, working.SnapshotSource());
    }

    [Theory(DisplayName = "Published Extension Inspect applies only semantic baselines to three-way comparison"), Trait("Feature", "extension-inspect"), Trait("Evidence", "EndToEnd")]
    [InlineData("semantic", "attention", 2, "changed", "open-forge extension update toolkit")]
    [InlineData("exact-bytes", "complete", 0, "unknown", "")]
    public async Task PublishedComparisonHonoursLifecycleFingerprintKind(
        string fingerprintKind,
        string expectedStatus,
        int expectedExitCode,
        string expectedRelation,
        string expectedNext)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInspectWorkspace.Create(
            fingerprintKind,
            intendedContent: "beta\n");
        var beforeWorkspace = working.SnapshotState();
        var beforeSource = working.SnapshotSource();

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "extension", "inspect", "toolkit",
                "--workspace", working.Path,
                "--source", working.SourcePath,
                "--json",
            ]);

        Assert.Equal(expectedExitCode, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("extension inspect", root.GetProperty("command").GetString());
        Assert.Equal(expectedStatus, root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("result");
        Assert.Equal("toolkit", commandResult.GetProperty("subject").GetProperty("id").GetString());
        Assert.Equal("package", commandResult.GetProperty("source").GetProperty("kind").GetString());
        Assert.Equal(working.SourcePath, commandResult.GetProperty("source").GetProperty("identity").GetString());
        Assert.Equal("trusted", commandResult.GetProperty("lifecycle").GetProperty("trust").GetString());
        Assert.Equal("three-way", commandResult.GetProperty("comparison").GetProperty("mode").GetString());
        var comparisonPath = Assert.Single(commandResult.GetProperty("comparison").GetProperty("paths").EnumerateArray());
        Assert.Equal(expectedRelation, comparisonPath.GetProperty("relation").GetString());
        Assert.Equal(fingerprintKind, comparisonPath.GetProperty("baseline").GetProperty("kind").GetString());
        Assert.Equal("semantic", comparisonPath.GetProperty("current").GetProperty("kind").GetString());
        Assert.Equal("semantic", comparisonPath.GetProperty("intended").GetProperty("kind").GetString());
        if (expectedNext.Length == 0)
        {
            Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        }
        else
        {
            Assert.Equal(expectedNext, root.GetProperty("next").GetProperty("command").GetString());
        }

        Assert.Equal(beforeWorkspace, working.SnapshotState());
        Assert.Equal(beforeSource, working.SnapshotSource());
    }

    [Fact(DisplayName = "Published Extension Inspect preserves status streams and bounded verbose diagnostics"), Trait("Feature", "extension-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedStatusStreamsRemainSeparate()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInspectWorkspace.Create(
            fingerprintKind: "semantic",
            intendedContent: "beta\n");

        var human = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "extension", "inspect", "toolkit",
                "--workspace", working.Path,
                "--source", working.SourcePath,
            ]);
        var invalid = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "extension", "inspect", "Toolkit",
                "--workspace", working.Path,
            ]);
        var plainJson = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "extension", "inspect", "toolkit",
                "--workspace", working.Path,
                "--source", working.SourcePath,
                "--json",
            ]);
        var verboseJson = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "extension", "inspect", "toolkit",
                "--workspace", working.Path,
                "--source", working.SourcePath,
                "--json", "--verbose",
            ]);

        Assert.Equal(2, human.ExitCode);
        Assert.Equal(string.Empty, human.StandardError);
        Assert.Contains("Status: requires attention", human.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge extension update toolkit", human.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(4, invalid.ExitCode);
        Assert.Equal(string.Empty, invalid.StandardOutput);
        Assert.Contains("Status: invalid", invalid.StandardError, StringComparison.Ordinal);
        Assert.Equal(plainJson.ExitCode, verboseJson.ExitCode);
        Assert.Equal(plainJson.StandardOutput, verboseJson.StandardOutput);
        Assert.Equal(string.Empty, plainJson.StandardError);
        Assert.InRange(verboseJson.StandardError.Length, 1, 4096);
        Assert.DoesNotContain('\n', verboseJson.StandardError.TrimEnd('\r', '\n'));
    }

    [Fact(DisplayName = "Published Extension Inspect rejects repeated source syntax before producing a result"), Trait("Feature", "extension-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRepeatedSourceIsParserFailure()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInspectWorkspace.Create(
            fingerprintKind: "semantic",
            intendedContent: "alpha\n");

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "extension", "inspect", "toolkit",
                "--workspace", working.Path,
                "--source", working.SourcePath,
                "--source", working.SourcePath,
                "--json",
            ]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("--source", result.StandardError, StringComparison.Ordinal);
    }
}

internal sealed class PublishedExtensionInspectWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace;
    private readonly TemporaryWorkspace _source;

    private PublishedExtensionInspectWorkspace(
        TemporaryWorkspace workspace,
        TemporaryWorkspace source)
    {
        _workspace = workspace;
        _source = source;
    }

    internal string Path => _workspace.Path;

    internal string SourcePath => _source.Path;

    internal string Combine(params string[] relativeSegments) => _workspace.Combine(relativeSegments);

    internal IReadOnlyDictionary<string, string> SnapshotState() => _workspace.SnapshotHashes();

    internal IReadOnlyDictionary<string, string> SnapshotSource() => _source.SnapshotHashes();

    internal static PublishedExtensionInspectWorkspace Create(
        string fingerprintKind,
        string intendedContent)
    {
        var workspace = TemporaryWorkspace.Create("e2e-extension-inspect-workspace");
        var source = TemporaryWorkspace.Create("e2e-extension-inspect-source");
        try
        {
            workspace.WriteText(".agents/toolkit.md", "alpha\n");
            workspace.WriteText(
                ".agents/open-forge.lifecycle.json",
                Lifecycle(workspace.Path, fingerprintKind));
            source.WriteText(
                "extension.json",
                """
                {
                  "id": "toolkit",
                  "name": "Toolkit",
                  "description": "A published test package.",
                  "version": "1.0.0",
                  "dependencies": []
                }
                """);
            source.WriteText("payload/.agents/toolkit.md", intendedContent);
            return new PublishedExtensionInspectWorkspace(workspace, source);
        }
        catch
        {
            source.Dispose();
            workspace.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        _source.Dispose();
        _workspace.Dispose();
    }

    private static string Lifecycle(string workspacePath, string fingerprintKind)
        => $$"""
            {
              "schemaVersion": 1,
              "fingerprintPolicy": "open-forge-markdown-v1",
              "workspacePath": "{{JsonEncodedText.Encode(System.IO.Path.GetFullPath(workspacePath))}}",
              "framework": null,
              "extensions": {
                "coverage": "complete",
                "packages": [{
                  "id": "toolkit",
                  "version": "1.0.0",
                  "source": "embedded catalogue",
                  "dependencies": [],
                  "paths": [".agents/toolkit.md"]
                }],
                "paths": [{
                  "path": ".agents/toolkit.md",
                  "owners": ["toolkit"],
                  "baselineFingerprint": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060",
                  "fingerprintKind": "{{fingerprintKind}}"
                }]
              }
            }
            """;
}
