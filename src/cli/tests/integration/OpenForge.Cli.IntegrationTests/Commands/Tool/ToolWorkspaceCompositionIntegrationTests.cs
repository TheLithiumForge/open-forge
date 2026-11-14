using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition.Models;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph;
using OpenForge.Cli.Core.Presentation.Status.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

public sealed class ToolWorkspaceCompositionIntegrationTests
{
    [Theory(DisplayName = "Full Root Status and Doctor without declarations never resolve Tool homes or call hosts"),
     InlineData("status", "absent"), InlineData("doctor", "absent"), InlineData("status", "legacy"), InlineData("doctor", "legacy"),
     InlineData("status", "empty-tools"), InlineData("doctor", "empty-tools"), InlineData("status", "malformed-lock"), InlineData("doctor", "malformed-lock"),
     Trait("Feature", "tool-read-isolation"), Trait("Evidence", "Integration")]
    public async Task NoDeclaredTools(string command, string shape)
    {
        using var fixture = new ToolCompositionFixture { ThrowHosts = true, InvalidHome = true };
        if (shape != "absent")
        {
            var settings = new JsonObject { [WorkspaceSettingsDefinitions.SchemaVersionProperty] = shape == "legacy" ? 5 : WorkspaceSettingsDefinitions.SchemaVersion };
            if (shape == "empty-tools") settings[ToolRecordDefinitions.ToolsProperty] = new JsonObject();
            var path = Path.Combine(fixture.Workspace, WorkspaceSettingsDefinitions.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? fixture.Workspace);
            File.WriteAllText(path, settings.ToJsonString());
        }
        if (shape == "malformed-lock") File.WriteAllText(Path.Combine(fixture.Workspace, WorkspaceOwnershipDefinitions.RelativePath), "null");
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync([command, "--format", "json", "--detail", "full"], fullRoot: true);
        using var json = run.Json();
        var findings = ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Findings));
        var toolCodes = command == "status"
            ? new[] { StatusFindingCode.ToolDeclaredMissing, StatusFindingCode.ToolDrift, StatusFindingCode.ToolUnsupported, StatusFindingCode.ToolObservationUnavailable }.Select(StatusWording.MachineCode)
            : new[] { DoctorFindingKind.ToolDeclaredMissing, DoctorFindingKind.ToolDrift, DoctorFindingKind.ToolUnsupported, DoctorFindingKind.ToolObservationUnavailable }.Select(DoctorWording.MachineCode);
        Assert.DoesNotContain(findings.EnumerateArray(), finding => toolCodes.Contains(ToolCompositionFixture.Field(finding, nameof(ToolReadFinding.Code)).GetString(), StringComparer.Ordinal));
        Assert.Equal(before, fixture.Snapshot());
        Assert.False(Directory.Exists(fixture.Home));
        Assert.Equal(0, fixture.ResolveCalls);
        Assert.Equal(0, fixture.PathObservations);
        Assert.Equal(0, fixture.PathEffects);
        Assert.Equal(0, fixture.DownloadCalls);
        Assert.Empty(fixture.Events);
    }

    [Theory(DisplayName = "Declared projects still enter Tool home observation and report unavailable homes once"), InlineData("status"), InlineData("doctor"),
     Trait("Feature", "tool-read-isolation"), Trait("Evidence", "Integration")]
    public async Task DeclaredInvalidHome(string command)
    {
        using var fixture = new ToolCompositionFixture { ThrowHosts = true, InvalidHome = true };
        fixture.Declare();
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync([command, "--format", "json", "--detail", "full"], fullRoot: true);
        using var json = run.Json();
        var findings = ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Findings));
        var expected = command == "status" ? StatusWording.MachineCode(StatusFindingCode.ToolObservationUnavailable) : DoctorWording.MachineCode(DoctorFindingKind.ToolObservationUnavailable);
        Assert.Single(findings.EnumerateArray(), finding => ToolCompositionFixture.Field(finding, nameof(ToolReadFinding.Code)).GetString() == expected);
        Assert.Equal(before, fixture.Snapshot());
        Assert.False(Directory.Exists(fixture.Home));
        Assert.Equal(0, fixture.ResolveCalls);
        Assert.Equal(0, fixture.PathObservations);
        Assert.Equal(0, fixture.DownloadCalls);
    }

    [Theory(DisplayName = "Full Root Status and Doctor add each Tool finding exactly once without writes"),
     InlineData("status", "missing"), InlineData("doctor", "missing"), InlineData("status", "drift"), InlineData("doctor", "drift"), InlineData("status", "invalid"), InlineData("doctor", "invalid"), InlineData("status", "unsupported"), InlineData("doctor", "unsupported"),
     Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task OnceOnlyWorkspaceFindings(string command, string scenario)
    {
        using var fixture = new ToolCompositionFixture();
        var catalogue = ToolCatalogueFactory.Create(ToolHostFactory.CreateNodeRuntimes());
        fixture.Catalogue = catalogue with { Entries = new Dictionary<string, Core.Framework.Tools.Models.Catalogue.ToolCatalogueEntry> { [CodeGraphDefinitions.Id] = catalogue.Entries[CodeGraphDefinitions.Id] } };
        fixture.EnsureCommands();
        fixture.Declare();
        fixture.Missing = scenario == "missing";
        fixture.DetectedVersion = scenario == "unsupported" ? "99.0.0" : CodeGraphDefinitions.Version;
        if (scenario == "invalid")
        {
            Directory.CreateDirectory(fixture.RecordPaths.ToolsDirectory);
            File.WriteAllText(fixture.RecordPaths.Tool(CodeGraphDefinitions.Id), "null");
        }
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync([command, "--format", "json", "--detail", "full"], fullRoot: true);
        using var json = run.Json();
        var statusKind = scenario switch { "missing" => StatusFindingCode.ToolDeclaredMissing, "drift" => StatusFindingCode.ToolDrift, "unsupported" => StatusFindingCode.ToolUnsupported, _ => StatusFindingCode.ToolObservationUnavailable };
        var doctorKind = scenario switch { "missing" => DoctorFindingKind.ToolDeclaredMissing, "drift" => DoctorFindingKind.ToolDrift, "unsupported" => DoctorFindingKind.ToolUnsupported, _ => DoctorFindingKind.ToolObservationUnavailable };
        var expected = command == "status" ? StatusWording.MachineCode(statusKind) : DoctorWording.MachineCode(doctorKind);
        var findings = ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Findings));
        Assert.Single(findings.EnumerateArray(), finding => ToolCompositionFixture.Field(finding, nameof(ToolReadFinding.Code)).GetString() == expected);
        Assert.Equal(before, fixture.Snapshot());
        Assert.False(Directory.Exists(Path.Combine(fixture.Workspace, TemporaryWorkspace.RuntimeDirectoryName)));
        Assert.Equal(0, fixture.DownloadCalls);
        Assert.Equal(0, fixture.PathEffects);
        Assert.DoesNotContain(ToolHostEvent.Setup, fixture.Events);
    }
}
