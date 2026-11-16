using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.TestSupport;

using Tokens = OpenForge.Cli.TestSupport.Contracts.PublishedTokens;

namespace OpenForge.Cli.EndToEndTests.Journeys;

/// <summary>
/// Published Tool Status against an owned home and an owned command directory. No package
/// manager, download or PATH edit is involved. The published binary resolves the Open Forge
/// home through the OS user home, which only HOME relocates on Linux and macOS.
/// </summary>
public sealed class ToolStatusJourneyTests
{
    private const string Feature = "tool-journey";
    // Operating-system variable names, not Open Forge contract values.
    private const string HomeVariable = "HOME";
    private const string PathVariable = "PATH";
    // A synthetic self-update below the catalogue's supported CodeGraph range.
    private const string OutOfRangeVersion = "1.5.0";

    [Fact(DisplayName = "A declared CodeGraph that is missing, then outside support, is reported with the bin readiness and no writes"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "T01")]
    public async Task StatusReportsMissingThenUnsupportedTool()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            Assert.Skip("Windows resolves the published Tool home through the known profile folder, which a child environment can't relocate.");
            return;
        }

        using var workspace = PublishedJourneyWorkspace.Create("tool-status-journey");
        using var machine = TemporaryWorkspace.Create("tool-status-machine");
        var home = machine.CreateDirectory("home");
        var commands = machine.CreateDirectory("commands");
        workspace.WriteText(Tokens.Settings.RelativePath, new JsonObject
        {
            [Tokens.Settings.SchemaVersionProperty] = Tokens.Settings.SchemaVersion,
            [Tokens.Tool.Tools] = new JsonObject { [Tokens.Tool.CodeGraphId] = new JsonObject { [Tokens.Tool.Version] = Tokens.Tool.CodeGraphRange } },
        }.ToJsonString());
        var environment = new Dictionary<string, string>(workspace.ProcessEnvironment, StringComparer.Ordinal)
        {
            [HomeVariable] = home,
            // Only the owned command directory, so no real package manager, Node or CodeGraph is visible.
            [PathVariable] = commands,
        };
        string[] status = [Tokens.Tool.Command, Tokens.Tool.Status, Tokens.Syntax.FormatOption, Tokens.Syntax.JsonFormat];

        var machineBefore = machine.SnapshotHashes();
        var missing = await PublishedJourneyProcess.RunWithoutWritesAsync(workspace.Target, workspace.Path, workspace.SnapshotState, status, environment);

        Assert.Equal(Tokens.Statuses.AttentionExitCode, missing.ExitCode);
        using (var json = JsonDocument.Parse(missing.StandardOutput))
        {
            var tool = Tool(json);
            Assert.Equal(Tokens.Tool.Missing, tool.GetProperty(Tokens.Tool.State).GetString());
            Assert.NotEqual(Tokens.Tool.Ready, tool.GetProperty(Tokens.Tool.BinReadiness).GetString());
        }
        Assert.Equal(machineBefore, machine.SnapshotHashes());

        var fake = Path.Combine(commands, Tokens.Tool.CodeGraphId);
        File.WriteAllText(fake, $"#!/bin/sh\necho {OutOfRangeVersion}\n");
        File.SetUnixFileMode(fake, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        machineBefore = machine.SnapshotHashes();

        var unsupported = await PublishedJourneyProcess.RunWithoutWritesAsync(workspace.Target, workspace.Path, workspace.SnapshotState, status, environment);

        Assert.Equal(Tokens.Statuses.AttentionExitCode, unsupported.ExitCode);
        using (var json = JsonDocument.Parse(unsupported.StandardOutput))
        {
            var tool = Tool(json);
            Assert.Equal(Tokens.Tool.Unsupported, tool.GetProperty(Tokens.Tool.State).GetString());
            Assert.Equal(OutOfRangeVersion, tool.GetProperty(Tokens.Tool.DetectedVersion).GetString());
        }
        Assert.Equal(machineBefore, machine.SnapshotHashes());
    }

    private static JsonElement Tool(JsonDocument json)
    {
        var tool = Assert.Single(json.RootElement.GetProperty(Tokens.Json.Data).GetProperty(Tokens.Tool.Tools).EnumerateArray());
        Assert.Equal(Tokens.Tool.CodeGraphId, tool.GetProperty(Tokens.Json.Id).GetString());
        return tool;
    }
}
