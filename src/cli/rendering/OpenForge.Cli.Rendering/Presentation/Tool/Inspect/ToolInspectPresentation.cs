using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Inspect.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Tool.Inspect;

namespace OpenForge.Cli.Core.Presentation.Tool.Inspect;

internal static class ToolInspectPresentation
{
    internal static CliReportRendering<ToolInspectResult, ToolReadPayload> Rendering { get; } = new()
    {
        Selector = Select,
        DataTextRenderer = Render,
        DataJsonTypeInfo = ToolReadJsonContext.Default.ToolReadPayload,
        Shape = CliCommandShape.Data,
    };

    internal static CliHelpContent CreateHelp() => global::OpenForge.Cli.Core.Presentation.Tool.Shared.Help.ToolHelpSections.CreateInspect();

    private static CliReport<ToolReadPayload> Select(ToolInspectResult result, CliSelection selection) => new()
    {
        ShowMinimalTextWarnings = true,
        Command = result.Command,
        Status = result.Status,
        Headline = new(ToolInspectText.Headline(), HeadlineKind(result.Status)),
        Workspace = result.WorkspacePath is { } path ? new(path, result.WorkspaceExplicit) : null,
        Data = result.Data,
        Next = result.Next,
        Findings = result.Findings.Select(finding => new CliFinding
        {
            Code = finding.Code,
            Title = ToolInspectText.Finding(),
            Message = finding.Cause,
            Severity = result.Status == CliSemanticStatus.Invalid ? CliSeverity.Error : CliSeverity.Warning,
            Subject = new(CliSubjectKind.Identifier, Id: finding.Id ?? result.Command),
            Actions = finding.NextCommand is { } command ? [new(command, finding.Cause)] : [],
        }).ToArray(),
    };

    private static CliHeadlineKind HeadlineKind(CliSemanticStatus status) => status switch
    {
        CliSemanticStatus.Complete => CliHeadlineKind.Done,
        CliSemanticStatus.Attention => CliHeadlineKind.Warnings,
        CliSemanticStatus.Incomplete => CliHeadlineKind.Incomplete,
        CliSemanticStatus.Invalid => CliHeadlineKind.CannotStart,
        CliSemanticStatus.Blocked => CliHeadlineKind.Blocked,
        CliSemanticStatus.Failed => CliHeadlineKind.Failed,
        CliSemanticStatus.Interrupted => CliHeadlineKind.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    private static CliTextDocument Render(ToolReadPayload data, CliSelection selection, CliTextStyle style)
    {
        var builder = new StringBuilder();
        foreach (var tool in data.Tools)
        {
            builder.AppendLine(CliText.Escape(ToolInspectText.ToolRow(tool.Id, tool.Name, tool.DeclaredRange, tool.State)));
            builder.AppendLine(CliText.Escape(ToolInspectText.Version(tool.LockedVersion, tool.DetectedVersion)));
            builder.AppendLine(CliText.Escape(ToolInspectText.Machine(tool.MachineState, tool.NativeRecordState)));
            builder.AppendLine(CliText.Escape(ToolInspectText.Bin(tool.BinPath, tool.BinReadiness)));
            builder.AppendLine(CliText.Escape(ToolInspectText.ManagerBin(tool.ManagerGlobalBinPath, tool.ManagerGlobalBinReadiness)));
            builder.AppendLine(CliText.Escape(ToolInspectText.ToolPath(tool.ToolPath)));
            builder.AppendLine(CliText.Escape(ToolInspectText.RuntimePath(tool.RuntimePath)));
            foreach (var native in tool.NativeSettings)
            {
                Line(builder, ToolInspectText.RecordedNativeHome(native.NativeHomePath));
                builder.AppendLine(CliText.Escape(ToolInspectText.NativeSetting(native.ChangeId, native.Previous, native.Applied, native.ManualChangeArguments)));
            }
            foreach (var change in tool.Changes)
            {
                builder.AppendLine(CliText.Escape(ToolInspectText.StoredChange(change.Id, change.Disposition, change.Current, change.Desired, change.Conflict)));
                if (change.NativeEnvironmentOverrides is null) Line(builder, ToolInspectText.NativeOverrideUnavailable());
                else if (change.NativeEnvironmentOverrides.Count == 0) Line(builder, ToolInspectText.NativeOverrideNone());
                else
                {
                    foreach (var environment in change.NativeEnvironmentOverrides)
                        Line(builder, ToolInspectText.NativeOverride(environment.Key, environment.Value));
                }
            }
            foreach (var removal in tool.Removals)
                builder.AppendLine(CliText.Escape(ToolInspectText.RetiredAddition(removal.Id, removal.Disposition, removal.Conflict)));
            builder.AppendLine(CliText.Escape(ToolInspectText.Origin(tool.Origin)));
            builder.AppendLine(CliText.Escape(ToolInspectText.Upstream(tool.Upstream)));
            builder.AppendLine(CliText.Escape(ToolInspectText.Licence(tool.Licence)));
            foreach (var channel in tool.Channels)
            {
                builder.AppendLine(CliText.Escape(ToolInspectText.Channel(channel.Id, channel.Kind, channel.Runtime, channel.RuntimeRange, channel.RequiredClient)));
                foreach (var client in channel.MinimumClientVersions)
                    builder.AppendLine(CliText.Escape(ToolInspectText.Detail(client.Key, client.Value)));
                foreach (var target in channel.Targets) Target(builder, target);
                foreach (var asset in channel.Assets)
                {
                    Line(builder, ToolInspectText.ReleaseAssetVersion(asset.Version));
                    Target(builder, asset.Target);
                    if (asset.BundledRuntime is not { } runtime) continue;
                    Line(builder, ToolInspectText.BundledRuntime(runtime.Runtime, runtime.Version));
                    Line(builder, ToolInspectText.BundledRuntimePath(runtime.RuntimeRelativePath));
                    Line(builder, ToolInspectText.BundledApplicationEntry(runtime.EntryRelativePath));
                    builder.AppendLine(CliText.Escape(ToolInspectText.BundledRuntimeArguments(runtime.Arguments)));
                }
            }
            foreach (var profile in tool.Profiles)
            {
                builder.AppendLine(CliText.Escape(ToolInspectText.Profile(profile.Id, profile.Range, profile.MinimumToolVersion, profile.MinimumOpenForgeVersion, string.Join(", ", profile.TestedVersions))));
                Line(builder, ToolInspectText.NativeHomePolicy(profile.NativeHomePolicy));
                Native(builder, ToolInspectText.NativeSetup(profile.Setup.Arguments), profile.Setup);
                Native(builder, ToolInspectText.NativeUninstall(profile.Uninstall.Arguments), profile.Uninstall);
                foreach (var addition in profile.Additions)
                {
                    builder.AppendLine(CliText.Escape(ToolInspectText.Addition(addition.Id, addition.Kind, addition.Scope, addition.Description)));
                    Line(builder, ToolInspectText.RequiredInitialState(addition.InitialState));
                    if (addition.WhenNotIgnored is not null) Line(builder, ToolInspectText.WhenNotIgnored(addition.WhenNotIgnored));
                    if (addition.PathTemplate is not null) Line(builder, ToolInspectText.ConfigPath(addition.PathTemplate));
                    if (addition.Pointer is not null) Line(builder, ToolInspectText.JsonPointer(addition.Pointer));
                    if (addition.Value is { } value) Line(builder, ToolInspectText.JsonValue(value.GetRawText()));
                    if (addition.StartMarker is not null) Line(builder, ToolInspectText.StartMarker(addition.StartMarker));
                    if (addition.EndMarker is not null) Line(builder, ToolInspectText.EndMarker(addition.EndMarker));
                    if (addition.ContentTemplate is not null) Line(builder, ToolInspectText.Content(addition.ContentTemplate));
                    if (addition.TemplateResource is not null) Line(builder, ToolInspectText.FileTemplate(addition.TemplateResource));
                    if (addition.DesiredValue is not null) Line(builder, ToolInspectText.DesiredNativeSetting(addition.DesiredValue));
                    if (addition.StoredValue is { } stored)
                    {
                        Line(builder, ToolInspectText.StoredNativePath(stored.PathTemplate));
                        Line(builder, ToolInspectText.StoredNativeParser(stored.Parser));
                        Line(builder, ToolInspectText.MissingStoredNativeValue(stored.MissingValue));
                        Line(builder, ToolInspectText.StoredNativeLimit(stored.MaximumBytes.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    }
                    if (addition.ValueParserId is not null) Line(builder, ToolInspectText.NativeStatusParser(addition.ValueParserId));
                    if (addition.ReadTimeoutSeconds is { } readTimeout) Line(builder, ToolInspectText.NativeReadTimeout(readTimeout.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    if (addition.ReadMaximumOutputBytes is { } readLimit) Line(builder, ToolInspectText.NativeReadLimit(readLimit.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    if (addition.ApplyArguments is { } apply) builder.AppendLine(CliText.Escape(ToolInspectText.Apply(apply)));
                    if (addition.ReadArguments is { } read) builder.AppendLine(CliText.Escape(ToolInspectText.Read(read)));
                    if (addition.ManualRestoreArguments is { } manual) builder.AppendLine(CliText.Escape(ToolInspectText.ManualSettingChange(manual)));
                    foreach (var environment in addition.ReadEnvironment ?? new Dictionary<string, string?>()) Line(builder, ToolInspectText.Environment(environment.Key, environment.Value));
                }
            }
        }
        return new([new CliTextSpan(builder.ToString().ReplaceLineEndings("\n"))]);
    }

    private static void Target(StringBuilder builder, ToolReadTarget target)
    {
        builder.AppendLine(CliText.Escape(ToolInspectText.Target(target.OperatingSystem, target.Cpu, target.LinuxLibc, target.MinimumOperatingSystemVersion, target.MinimumLibcVersion)));
        Line(builder, ToolInspectText.MinimumLinuxKernel(target.MinimumLinuxKernelVersion));
        Line(builder, ToolInspectText.MinimumLibStdCpp(target.MinimumLibStdCppVersion));
    }

    private static void Native(StringBuilder builder, string heading, ToolReadCommand command)
    {
        builder.AppendLine(CliText.Escape(heading));
        Line(builder, ToolInspectText.NativeTimeout(command.TimeoutSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        Line(builder, ToolInspectText.NativeOutputLimit(command.MaximumOutputBytes.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        if (command.NativePlan is { } plan)
        {
            builder.AppendLine(CliText.Escape(ToolInspectText.NativeInventory(plan.Arguments)));
            Line(builder, ToolInspectText.NativeInventoryParser(plan.Parser));
            Line(builder, ToolInspectText.NativeInventoryTimeout(plan.TimeoutSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            Line(builder, ToolInspectText.NativeInventoryLimit(plan.MaximumOutputBytes.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }
        foreach (var output in command.PossibleOutputs)
            builder.AppendLine(CliText.Escape(ToolInspectText.PossibleOutput(output.PathTemplate, output.Scope, output.Match, output.Purpose,
                string.Join(", ", new[] { output.OperatingSystem, output.ClientId }.Where(value => value is not null)))));
        if (command.DryRunArguments is { } preview) builder.AppendLine(CliText.Escape(ToolInspectText.NativePreview(preview)));
        foreach (var effect in command.Effects) builder.AppendLine(CliText.Escape(ToolInspectText.Effect(effect.Scope, effect.Description, effect.PathTemplate, effect.Selector)));
        foreach (var environment in command.Environment) Line(builder, ToolInspectText.Environment(environment.Key, environment.Value));
    }
    private static void Line(StringBuilder builder, string value) => builder.AppendLine(CliText.Escape(value));
}
