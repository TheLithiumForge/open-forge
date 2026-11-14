using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Presentation.Tool.Shared.Writing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing;

[Trait("Feature", "tool"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class ToolWriteSafetyTests
{
    [Theory(DisplayName = "Project uninstall with incidental account telemetry executes under yes")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IncidentalTelemetryDoesNotRequireAccountConsent(bool terminal)
    {
        var fixture = new ToolWriteOperationFixture { IncidentalTelemetry = true, Terminal = terminal };
        fixture.DetectedVersions[ToolWriteOperationFixture.Id] = ToolWriteOperationFixture.OldVersion;
        fixture.SeedChanges(new ToolManagedFileAddition { Id = ToolWriteOperationFixture.FirstChange, Path = ToolWriteOperationFixture.FirstChange, Sha256 = ToolWriteOperationFixture.Digest });
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Single(fixture.Processes);
        Assert.Empty(fixture.OptionalQuestions);
        Assert.Single(fixture.Calls, call => call.Call == ToolWriteCall.NativePreparation);
        Assert.True(fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.Native) < fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.ConfigRemove));
    }

    [Theory(DisplayName = "Too-old running Open Forge prerelease blocks every tool effect")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    [InlineData((int)ToolWriteKind.Remove)]
    public async Task MinimumOpenForgeBlocksBeforeEffects(int kind)
    {
        var fixture = new ToolWriteOperationFixture(secondTool: true, minimumOpenForgeVersion: "0.9.0-beta.19");
        fixture.Facts = fixture.Facts with { OpenForgeVersion = "0.9.0-beta.18" };
        fixture.DetectedVersions[ToolWriteOperationFixture.Id] = ToolWriteOperationFixture.OldVersion;
        var result = await fixture.Create().ExecuteAsync(fixture.Request((ToolWriteKind)kind), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ToolWriteFindingCode.OpenForgeVersionUnsupported);
        Assert.Empty(fixture.ExecutedPlans);
        Assert.Empty(fixture.Processes);
        Assert.Empty(fixture.ConfigCalls);
        Assert.Empty(fixture.Writes);
        Assert.Empty(fixture.CompletedBackups);
    }

    [Theory(DisplayName = "Root native environment reaches recipe contexts without appearing in results")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NativeEnvironmentIsPrivateInput(bool unavailable)
    {
        const string raw = "synthetic raw native value";
        var fixture = new ToolWriteOperationFixture();
        fixture.Facts = fixture.Facts with
        {
            NativeEnvironment = unavailable ? null : new Dictionary<string, string> { [ToolNativeConfigDefinitions.CodeGraphTelemetryVariable] = raw },
        };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(preview: true), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.All(fixture.SetupRequests, request => Assert.Same(fixture.Facts.NativeEnvironment, request.Context.NativeEnvironment));
        Assert.All(fixture.ConfigRequests, request => Assert.Same(fixture.Facts.NativeEnvironment, request.Context.NativeEnvironment));
        Assert.DoesNotContain(raw, JsonSerializer.Serialize(result.Data, ToolWriteJsonContext.Default.ToolWriteData), StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Native stored values stay separate from checked effective environment overrides")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EffectiveNativeValuesAreSeparate(bool overridden)
    {
        var fixture = new ToolWriteOperationFixture();
        fixture.Observation = new()
        {
            Change = new ToolNativeConfigChange
            {
                Id = ToolWriteOperationFixture.NativeChange,
                Description = "Synthetic native setting.",
                Scope = ToolEffectScope.Account,
                InitialState = ToolInitialConfigState.ReadableNativeValue,
                AllowListId = ToolWriteOperationFixture.NativeChange,
                ApplyArguments = [],
                DesiredValue = ToolNativeConfigDefinitions.DisabledValue,
                ManualRestoreArgumentsTemplate = [],
            },
            Current = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = ToolNativeConfigDefinitions.DisabledValue },
            Desired = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = ToolNativeConfigDefinitions.DisabledValue },
            Disposition = ToolChangeDisposition.NoChange,
            NativeEnvironmentOverrides = overridden
                ? new Dictionary<string, string> { [ToolNativeConfigDefinitions.CodeGraphTelemetryVariable] = ToolNativeConfigDefinitions.EnabledValue }
                : new Dictionary<string, string>(),
        };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(preview: true), CancellationToken.None);

        var change = Assert.Single(Assert.Single(result.Data.Tools).Changes);
        Assert.Equal(ToolNativeConfigDefinitions.DisabledValue, change.StoredNativeValue);
        Assert.Same(fixture.Observation.NativeEnvironmentOverrides, change.NativeEnvironmentOverrides);
    }

    [Theory(DisplayName = "PATH add backups survive failure and complete only after final publication")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task PathBackupsFollowPublication(bool failPath, bool failPublication)
    {
        var fixture = new ToolWriteOperationFixture { Terminal = true, AddPathChoice = true, FailPath = failPath, FailPathPublication = failPublication };
        fixture.Facts = fixture.Facts with { Path = fixture.Facts.Path with { Readiness = ToolPathReadiness.Missing } };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(yes: false), CancellationToken.None);

        var pending = Assert.Single(fixture.PathBackups);
        if (failPath || failPublication)
        {
            Assert.Contains(result.Data.Plan.Backups, backup => backup.Target == pending.Backup.TargetPath);
            Assert.Empty(fixture.CompletedBackups);
            Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Contains(fixture.CompletedBackups, backup => backup.Backup == pending.Backup);
            Assert.True(fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.BackupComplete) > fixture.Calls.FindLastIndex(call => call.Call == ToolWriteCall.Write));
        }
        if (!failPublication) Assert.NotNull(fixture.Records.Path);
    }

    [Theory(DisplayName = "PATH removal backups stay pending through record publication and failures")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task PathRemovalBackupsFollowPublication(bool failPath, bool failPublication)
    {
        var fixture = new ToolWriteOperationFixture { Terminal = true, AddPathChoice = true, AcceptOptional = true, PlacedByOpenForge = true };
        fixture.Facts = fixture.Facts with { Path = fixture.Facts.Path with { Readiness = ToolPathReadiness.Missing } };
        var writer = fixture.Create();
        await writer.ExecuteAsync(fixture.Request(yes: false), CancellationToken.None);
        fixture.CompletedBackups.Clear();
        fixture.PathBackups.Clear();
        fixture.Calls.Clear();
        fixture.FailPath = failPath;
        fixture.FailPathPublication = failPublication;
        var result = await writer.ExecuteAsync(fixture.Request(ToolWriteKind.Remove, yes: false), CancellationToken.None);

        var pending = Assert.Single(fixture.PathBackups);
        if (failPath || failPublication)
        {
            Assert.Contains(result.Data.Plan.Backups, backup => backup.Target == pending.Backup.TargetPath);
            Assert.Empty(fixture.CompletedBackups);
            Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Contains(fixture.CompletedBackups, backup => backup.Backup == pending.Backup);
            Assert.True(fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.BackupComplete) > fixture.Calls.FindLastIndex(call => call.Call == ToolWriteCall.Write));
        }
    }
}
