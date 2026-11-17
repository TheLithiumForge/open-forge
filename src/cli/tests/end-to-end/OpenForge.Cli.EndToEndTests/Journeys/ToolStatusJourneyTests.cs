using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.TestSupport;

using Tokens = OpenForge.Cli.TestSupport.Contracts.PublishedTokens;

namespace OpenForge.Cli.EndToEndTests.Journeys;

/// <summary>
/// Published Tool Status against an owned Tool home, user home and command directory. No package
/// manager, download or PATH edit is involved. Published writer journeys wait for the release whose
/// version meets the real catalogue's minimum Open Forge version, so this journey stays read-only.
/// </summary>
public sealed class ToolStatusJourneyTests
{
    private const string Feature = "tool-journey";
    // Operating-system variable names and the Windows command-script extension, not Open Forge contract values.
    private const string HomeVariable = "HOME";
    private const string UserProfileVariable = "USERPROFILE";
    private const string PathVariable = "PATH";
    private const string WindowsScriptExtension = ".cmd";
    // npm package manifest members that point a command at its entry script.
    private const string NpmNameMember = "name";
    private const string NpmBinMember = "bin";
    // A synthetic self-update below the catalogue's supported CodeGraph range.
    private const string OutOfRangeVersion = "1.5.0";
    private const string FakeEntry = "cli.js";
    private const string CommandsDirectory = "commands";

    [Fact(DisplayName = "A declared CodeGraph that is missing, then outside support, is reported with the bin readiness and no writes"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "T01")]
    public async Task StatusReportsMissingThenUnsupportedTool()
    {
        using var workspace = PublishedJourneyWorkspace.Create("tool-status-journey");
        using var machine = TemporaryWorkspace.Create("tool-status-machine");
        var home = machine.CreateDirectory("home");
        var commands = machine.CreateDirectory(CommandsDirectory);
        workspace.WriteText(Tokens.Settings.RelativePath, new JsonObject
        {
            [Tokens.Settings.SchemaVersionProperty] = Tokens.Settings.SchemaVersion,
            [Tokens.Tool.Tools] = new JsonObject { [Tokens.Tool.CodeGraphId] = new JsonObject { [Tokens.Tool.Version] = Tokens.Tool.CodeGraphRange } },
        }.ToJsonString());
        var environment = new Dictionary<string, string>(workspace.ProcessEnvironment, StringComparer.Ordinal)
        {
            [Tokens.Tool.HomeVariable] = machine.Combine("tool-home"),
            [HomeVariable] = home,
            [UserProfileVariable] = home,
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

        WriteOutOfRangeCodeGraph(machine);
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

    // Unix runs an executable script directly. Windows resolves an npm command script through its
    // package manifest and the Node command beside it, so the fake Node answers the version probe.
    // Every fake file goes through the owned workspace so its cleanup knows about it.
    private static void WriteOutOfRangeCodeGraph(TemporaryWorkspace machine)
    {
        if (!OperatingSystem.IsWindows())
        {
            var script = machine.CreateFile(Path.Combine(CommandsDirectory, Tokens.Tool.CodeGraphId), $"#!/bin/sh\necho {OutOfRangeVersion}\n");
            File.SetUnixFileMode(script, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            return;
        }

        machine.CreateFile(Path.Combine(CommandsDirectory, Tokens.Tool.CodeGraphId + WindowsScriptExtension), "@exit /b 1\r\n");
        machine.CreateFile(Path.Combine(CommandsDirectory, Tokens.Tool.NodeCommand + WindowsScriptExtension), $"@echo {OutOfRangeVersion}\r\n");
        var package = Path.Combine(CommandsDirectory, Tokens.Tool.NpmModulesDirectory, Tokens.Tool.CodeGraphId);
        machine.CreateFile(Path.Combine(package, Tokens.Tool.NpmManifest), new JsonObject
        {
            [NpmNameMember] = Tokens.Tool.CodeGraphId,
            [Tokens.Tool.Version] = OutOfRangeVersion,
            [NpmBinMember] = FakeEntry,
        }.ToJsonString());
        machine.CreateFile(Path.Combine(package, FakeEntry));
    }
}
