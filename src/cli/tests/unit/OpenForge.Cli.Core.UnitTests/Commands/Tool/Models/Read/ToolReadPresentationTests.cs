using OpenForge.Cli.Core.Commands.Tool.Inspect.Models;
using OpenForge.Cli.Core.Commands.Tool.List.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Commands.Tool.Status.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Presentation.Tool.Inspect;
using OpenForge.Cli.Core.Presentation.Tool.List;
using OpenForge.Cli.Core.Presentation.Tool.Status;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Models.Read;

public sealed class ToolReadPresentationTests
{
    [Fact(DisplayName = "Tool read leaves share one source-generated payload and reviewed human output"), Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void ReadReports()
    {
        var data = new ToolReadPayload([ToolReadProjection.Catalogue(Entry(), new(new(3, 2))) with
        {
            State = "missing",
            Changes = [new("synthetic-native", "no-change", ToolNativeConfigDefinitions.DisabledValue, ToolNativeConfigDefinitions.DisabledValue, null)
            { NativeEnvironmentOverrides = new Dictionary<string, string> { ["SYNTHETIC_NATIVE_OVERRIDE"] = ToolNativeConfigDefinitions.EnabledValue } }],
            Removals = [new("synthetic-retired", "remove", "synthetic-old", null, null)],
        }]);
        Assert.Equal(Entry().Channels[0].Requirements.Runtime?.VersionRange, data.Tools[0].Channels[0].RuntimeRange);
        Assert.Equal(Entry().Profiles[0].Uninstall.Arguments, data.Tools[0].Profiles[0].Uninstall.Arguments);
        Assert.Equal(Entry().Profiles[0].MinimumToolVersion, data.Tools[0].Profiles[0].MinimumToolVersion);
        Assert.Equal(Entry().Profiles[0].Setup.PossibleOutputs[0].PathTemplate, data.Tools[0].Profiles[0].Setup.PossibleOutputs[0].PathTemplate);
        Assert.Equal(Entry().Channels[0].Requirements.Targets[0].MinimumLinuxKernelVersion, data.Tools[0].Channels[0].Targets[0].MinimumLinuxKernelVersion);
        Assert.NotNull(Assert.Single(data.Tools[0].Channels[1].Assets).BundledRuntime);
        var list = new ToolListResult { Workspace = null, Status = CliSemanticStatus.Complete, Data = data with { Tools = [data.Tools[0] with { State = null }] } };
        var inspect = new ToolInspectResult { Workspace = null, Status = CliSemanticStatus.Attention, Data = data };
        var status = new ToolStatusResult { Workspace = null, Status = CliSemanticStatus.Attention, Data = data };
        var captures = new Dictionary<string, string>();
        foreach (var format in new[] { CliFormat.Text, CliFormat.Json })
        {
            var presentation = new CliPresentation(format, CliDetail.Standard, null);
            var listed = CliRenderingStage.Render(new CliPresentationRequest<ToolListResult>(list, presentation), ToolListPresentation.Rendering);
            var inspected = CliRenderingStage.Render(new CliPresentationRequest<ToolInspectResult>(inspect, presentation), ToolInspectPresentation.Rendering);
            var checkedStatus = CliRenderingStage.Render(new CliPresentationRequest<ToolStatusResult>(status, presentation), ToolStatusPresentation.Rendering);
            var suffix = format == CliFormat.Json ? $"{CommandOutputSnapshot.JsonContentNameSegment}standard" : ".standard";
            captures.Add($"list{suffix}", listed.PrimaryContent);
            captures.Add($"inspect{suffix}", inspected.PrimaryContent);
            captures.Add($"status{suffix}", checkedStatus.PrimaryContent);
            if (format == CliFormat.Json)
            {
                Assert.Equal(CliOutputTarget.StandardOutput, listed.PrimaryTarget);
                Assert.Equal(CliOutputTarget.StandardOutput, inspected.PrimaryTarget);
                Assert.Equal(CliOutputTarget.StandardOutput, checkedStatus.PrimaryTarget);
            }
        }
        CommandOutputSnapshot.MatchDetailSnapshot(captures);
    }

    [Fact(DisplayName = "Tool read presentation covers every named shared status and keeps JSON on stdout"), Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void SharedStatusesAreCovered()
    {
        CliSemanticStatus[] known = [CliSemanticStatus.Complete, CliSemanticStatus.Failed, CliSemanticStatus.Attention, CliSemanticStatus.Incomplete,
            CliSemanticStatus.Invalid, CliSemanticStatus.Blocked, CliSemanticStatus.Interrupted];
        Assert.Equal(known.Order(), Enum.GetValues<CliSemanticStatus>().Order());
        var presentation = new CliPresentation(CliFormat.Json, CliDetail.Standard, null);
        foreach (var state in known)
        {
            var list = new ToolListResult { Workspace = null, Status = state, Data = new([]) };
            var inspect = new ToolInspectResult { Workspace = null, Status = state, Data = new([]) };
            var status = new ToolStatusResult { Workspace = null, Status = state, Data = new([]) };
            var outputs = new[]
            {
                CliRenderingStage.Render(new CliPresentationRequest<ToolListResult>(list, presentation), ToolListPresentation.Rendering),
                CliRenderingStage.Render(new CliPresentationRequest<ToolInspectResult>(inspect, presentation), ToolInspectPresentation.Rendering),
                CliRenderingStage.Render(new CliPresentationRequest<ToolStatusResult>(status, presentation), ToolStatusPresentation.Rendering),
            };
            foreach (var output in outputs)
            {
                Assert.Equal(state, output.Status);
                Assert.Equal(CliOutputTarget.StandardOutput, output.PrimaryTarget);
                using var parsed = System.Text.Json.JsonDocument.Parse(output.PrimaryContent);
                Assert.Equal(System.Text.Json.JsonValueKind.Object, parsed.RootElement.ValueKind);
            }
        }
        var undefined = new ToolListResult { Workspace = null, Status = (CliSemanticStatus)(-1), Data = new([]) };
        Assert.Throws<ArgumentOutOfRangeException>(() => CliRenderingStage.Render(new CliPresentationRequest<ToolListResult>(undefined, presentation), ToolListPresentation.Rendering));
    }

    private static ToolCatalogueEntry Entry()
    {
        var setup = new ToolSetupSpec
        {
            Arguments = ["synthetic-setup"],
            DryRunArguments = ["synthetic-preview"],
            Environment = new Dictionary<string, string?>(),
            Timeout = TimeSpan.FromSeconds(1),
            MaximumOutputBytes = 128,
            PossibleOutputs = [new() { PathTemplate = "${nativeHome}/synthetic.json", Scope = ToolEffectScope.Account, Match = ToolNativeOutputMatch.Exact, Purpose = ToolNativeOutputPurpose.Backup, ClientId = "synthetic-client" }],
            NativePlan = new() { Arguments = ["synthetic-plan"], Parser = ToolNativePlanParser.CbmInstallPlanV1, Timeout = TimeSpan.FromSeconds(1), MaximumOutputBytes = 128 },
            Effects = [new() { Id = "synthetic-effect", Scope = ToolEffectScope.Account, Kind = ToolDeclaredEffectKind.Configuration, Description = "Synthetic account registration" }],
        };
        return new()
        {
            Identity = new() { Id = "synthetic", Name = "Synthetic tool", Origin = ToolOrigin.ThirdParty, Upstream = new("https://example.invalid/tool"), Licence = "synthetic" },
            Detection = new() { Command = "synthetic", Arguments = [], Environment = new Dictionary<string, string?>(), Timeout = TimeSpan.FromSeconds(1), MaximumOutputBytes = 128, Parser = ToolVersionParser.StableSemVer },
            Channels = [new ToolNpmChannel
            {
                Id = "synthetic-npm", Kind = ToolChannelKind.Npm, Package = "synthetic-package", EntryRelativePath = "synthetic-entry.js", RequiredClient = ToolNpmClient.Npm,
                MinimumClientVersions = new Dictionary<ToolNpmClient, string> { [ToolNpmClient.Npm] = "1.0.0" },
                Requirements = new() { Runtime = new("synthetic-runtime", ">=20"), Targets = [new()
                    { OperatingSystem = ToolOperatingSystem.Linux, Cpu = ToolCpu.X64, LinuxLibc = ToolLinuxLibc.Glibc, MinimumOperatingSystemVersion = "5.0", MinimumLibcVersion = "2.17", MinimumLinuxKernelVersion = "4.18", MinimumLibStdCppVersion = "6.0.25" }] },
            }, new ToolGitHubReleaseChannel
            {
                Id = "synthetic-release", Kind = ToolChannelKind.GitHubRelease, Repository = "synthetic/release", Requirements = new() { Targets = [] },
                Assets = [new()
                {
                    Version = "3.2.1", Target = new() { OperatingSystem = ToolOperatingSystem.Linux, Cpu = ToolCpu.X64 }, Url = new("https://example.invalid/asset"),
                    Archive = ToolArchiveKind.TarGzip, StripComponents = 1, EntryRelativePath = "synthetic", MaximumBytes = 128,
                    BundledRuntime = new() { Runtime = "synthetic-runtime", Version = "20.1.0", RuntimeRelativePath = "runtime", EntryRelativePath = "app.js", Arguments = ["synthetic-runtime-argument"] },
                }],
            }],
            Profiles = [new()
            {
                NativeHomePolicy = ToolNativeHomePolicy.Node, Id = "synthetic-profile", Range = new(3, 2), MinimumToolVersion = "3.2.1", MinimumOpenForgeVersion = "0.1.0", TestedVersions = ["3.2.1"], Setup = setup, Uninstall = setup with { Arguments = ["synthetic-remove"], DryRunArguments = null },
                Changes = [new ToolNativeConfigChange
                {
                    Id = "synthetic-native", Scope = ToolEffectScope.Account, InitialState = ToolInitialConfigState.ReadableNativeValue,
                    Description = "Synthetic native setting", AllowListId = "synthetic-setting", ApplyArguments = ["setting", "disable"], ReadArguments = ["setting", "status"],
                    StoredValue = new() { PathTemplate = "${nativeHome}/synthetic.json", Parser = ToolNativeStoredValueParser.CodeGraphTelemetryV1, MissingValue = "on", MaximumBytes = 128 },
                    ReadEnvironment = new Dictionary<string, string?> { ["SYNTHETIC_OVERRIDE"] = null }, ValueParserId = "synthetic-parser", DesiredValue = "disabled", ManualRestoreArgumentsTemplate = ["setting", "{previous}"],
                }],
            }],
        };
    }
}
