using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Choose.Models;
using OpenForge.Cli.Core.Commands.Tool.Configure.Models;
using OpenForge.Cli.Core.Commands.Tool.Install.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing;
using OpenForge.Cli.Core.Commands.Tool.Remove.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;
using OpenForge.Cli.Core.Commands.Tool.Update.Models;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.Core.Presentation.Tool.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Reporting;

[Trait("Feature", "tool-contracts"), Trait("Evidence", "Unit")]
public sealed class ToolWriteReportProjectionTests
{
    [Theory(DisplayName = "Each writer projects its complete data and machine findings through report models")]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3), InlineData(4)]
    public void ProjectsWriter(int leaf)
    {
        var data = Data();
        ToolWriteIssue[] issues = [new(ToolWriteIssueCode.PathExcluded, CliSemanticStatus.Blocked, ToolFixtureSource.ArtifactName, string.Empty)];
        var report = leaf switch
        {
            0 => ToolWriteReport.From(new ToolInstallResult { Status = CliSemanticStatus.Blocked, Workspace = null, Data = data, Issues = issues }),
            1 => ToolWriteReport.From(new ToolConfigureResult { Status = CliSemanticStatus.Blocked, Workspace = null, Data = data, Issues = issues }),
            2 => ToolWriteReport.From(new ToolChooseResult { Status = CliSemanticStatus.Blocked, Workspace = null, Data = data, Issues = issues }),
            3 => ToolWriteReport.From(new ToolUpdateResult { Status = CliSemanticStatus.Blocked, Workspace = null, Data = data, Issues = issues }),
            4 => ToolWriteReport.From(new ToolRemoveResult { Status = CliSemanticStatus.Blocked, Workspace = null, Data = data, Issues = issues }),
            _ => throw new ArgumentOutOfRangeException(nameof(leaf)),
        };
        Assert.Equal(data.Id, report.Data.Id);
        Assert.Equal(data.Role, report.Data.Role);
        Assert.Equal(data.Pin, report.Data.Pin);
        Assert.Equal(ToolWriteReportProjection.Mode(data.Mode), report.Data.Mode);
        Assert.Equal(ToolWriteReportProjection.Backup(ToolWriteBackupMode.Sibling), report.Data.BackupMode);
        Assert.Equal(issues[0].Status, Assert.Single(report.Issues).SemanticStatus);
        Assert.Equal(issues[0].Subject, report.Issues[0].Subject);
        Assert.Equal(data.Plan[0].Subject, Assert.Single(report.Data.Plan).Subject);
        Assert.Equal(data.Permissions[0].Path, Assert.Single(report.Data.Permissions).Path);
        Assert.Equal(data.Consents[0].Artifact?.Integrity, Assert.Single(report.Data.Consents).Artifact?.Integrity);
        Assert.Equal(ToolWriteReportProjection.Verification(data.Verification[0].State), Assert.Single(report.Data.Verification).State);
        Assert.Equal(data.Remaining[0].Subject, Assert.Single(report.Data.Remaining).Subject);
        Assert.Equal(data.Effects[0].Subject, Assert.Single(report.Data.Effects).Subject);
        Assert.Same(data.Instructions, report.Data.Instructions);
        Assert.Equal(leaf == 0, report.Data.Tools is not null);
    }

    [Fact(DisplayName = "Team install report data keeps every per-pin result and its established order")]
    public void KeepsTeamItems()
    {
        var items = new[]
        {
            new ToolInstallItemResult(ToolFixtureSource.Id, CliSemanticStatus.Complete, Data(), []),
            new ToolInstallItemResult(ToolFixtureSource.Id + "-second", CliSemanticStatus.Blocked, Data(), []),
        };
        var result = new ToolInstallResult { Status = CliSemanticStatus.Blocked, Workspace = null, Data = Data() with { Id = null, Pin = null }, Tools = items };
        var report = ToolWriteReport.From(result);
        Assert.Null(report.Data.Id);
        Assert.Null(report.Data.Pin);
        Assert.NotNull(report.Data.Tools);
        Assert.Equal(items.Select(item => item.Id), report.Data.Tools.Select(item => item.Id));
        Assert.Equal(items.Select(item => CliStatusDefinitions.Read(item.Status).MachineName), report.Data.Tools.Select(item => item.Status));
    }

    [Fact(DisplayName = "Writer report enum mappings cover each declared value and reject unnamed values")]
    public void KeepsClosedVocabularies()
    {
        ToolWriteMode[] modes = [ToolWriteMode.Preview, ToolWriteMode.Apply];
        ToolConsentDecision[] consents = [ToolConsentDecision.NotRequired, ToolConsentDecision.Missing, ToolConsentDecision.Supplied, ToolConsentDecision.RecordedLocal];
        ToolVerificationState[] checks = [ToolVerificationState.NotRun, ToolVerificationState.Verified, ToolVerificationState.Failed, ToolVerificationState.Unavailable];
        ToolWriteBackupMode[] backups = [ToolWriteBackupMode.Git, ToolWriteBackupMode.Sibling];
        Assert.Equal(modes, Enum.GetValues<ToolWriteMode>());
        Assert.Equal(consents, Enum.GetValues<ToolConsentDecision>());
        Assert.Equal(checks, Enum.GetValues<ToolVerificationState>());
        Assert.Equal(backups, Enum.GetValues<ToolWriteBackupMode>());
        Assert.Equal(modes.Length, modes.Select(ToolWriteReportProjection.Mode).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(consents.Length, consents.Select(ToolWriteReportProjection.Consent).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(checks.Length, checks.Select(ToolWriteReportProjection.Verification).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(backups.Length, backups.Select(ToolWriteReportProjection.Backup).Distinct(StringComparer.Ordinal).Count());
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolWriteReportProjection.Mode((ToolWriteMode)(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolWriteReportProjection.Consent((ToolConsentDecision)(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolWriteReportProjection.Verification((ToolVerificationState)(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolWriteReportProjection.Backup((ToolWriteBackupMode)(-1)));
    }

    [Fact(DisplayName = "The complete writer report graph serializes through its static context with string statuses")]
    public void SerializesStaticReportGraph()
    {
        ToolWriteIssue[] issues = [new(ToolWriteIssueCode.PathExcluded, CliSemanticStatus.Blocked, ToolFixtureSource.ArtifactName, string.Empty)];
        var result = new ToolInstallResult
        {
            Status = CliSemanticStatus.Blocked,
            Workspace = null,
            Data = Data(),
            Tools = [new(ToolFixtureSource.Id, CliSemanticStatus.Blocked, Data(), issues)],
        };
        var report = ToolWriteReport.From(result);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(report.Data, ToolWriteReportJsonContext.Default.ToolWriteReportData);
        using var document = JsonDocument.Parse(bytes);
        var mode = document.RootElement.GetProperty(Field(nameof(ToolWriteReportData.Mode)));
        Assert.Equal(ToolWriteReportProjection.Mode(ToolWriteMode.Preview), mode.GetString());
        var item = document.RootElement.GetProperty(Field(nameof(ToolWriteReportData.Tools)))[0];
        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Blocked).MachineName, item.GetProperty(Field(nameof(ToolReportInstallItem.Status))).GetString());
        var issue = item.GetProperty(Field(nameof(ToolReportInstallItem.Issues)))[0];
        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Blocked).MachineName, issue.GetProperty(Field(nameof(ToolWriteReportIssue.Status))).GetString());
    }

    private static string Field(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);

    private static ToolWriteData Data() => new()
    {
        Mode = ToolWriteMode.Preview,
        Id = ToolFixtureSource.Id,
        Role = null,
        Pin = ToolFixtureSource.Version,
        BackupMode = ToolWriteBackupMode.Sibling,
        Plan = [new(nameof(ToolPlannedChange), ToolFixtureSource.ArtifactName, nameof(ProjectsWriter), null, null, [], [])],
        Permissions = [new(ToolFixtureSource.ArtifactName, true, false, null)],
        Consents = [new(nameof(ToolConsent), ToolFixtureSource.ArtifactName, ToolConsentDecision.Supplied, ToolFixtureSource.Id)
        { Artifact = new(new Uri("https://fixture.invalid/archive"), ToolFixtureSource.ArtifactBytes().Length,
            ToolArtifactIntegrity.Sha256Prefix + ToolCatalogueReader.Hash(ToolFixtureSource.ArtifactBytes())) }],
        Verification = [new(nameof(ToolVerification), ToolFixtureSource.ArtifactName, ToolVerificationState.Verified, null)],
        Remaining = [new(ToolFixtureSource.ArtifactName, nameof(ProjectsWriter), string.Empty)],
        Effects = [new(nameof(ToolAppliedChange), ToolFixtureSource.ArtifactName, nameof(ProjectsWriter), null, null)],
        Instructions = [new(Path.GetFullPath(ToolFixtureSource.ArtifactName), [], new(StringComparer.Ordinal), Path.GetFullPath("."), null)],
    };
}
