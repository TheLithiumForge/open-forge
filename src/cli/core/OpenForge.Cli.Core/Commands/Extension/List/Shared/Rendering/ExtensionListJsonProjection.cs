using OpenForge.Cli.Core.Commands.Extension.List.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;

internal static class ExtensionListJsonProjection
{
    internal static ExtensionListJsonDocument Create(ExtensionListResult result)
    {
        return new ExtensionListJsonDocument
        {
            SchemaVersion = ExtensionListDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new ExtensionListJsonResult
            {
                Source = result.Source is null ? null : Source(result.Source),
                Selection = new ExtensionListJsonSelection
                {
                    Installed = result.Selection.Installed,
                    Available = result.Selection.Available,
                },
                Coverage = new ExtensionListJsonCoverage
                {
                    Installed = Coverage(result.InstalledCoverage),
                    Available = Coverage(result.AvailableCoverage),
                    LifecycleTrust = result.LifecycleTrust is null ? null : Trust(result.LifecycleTrust.Value),
                },
                Installed = result.Installed.Select(Installed).ToArray(),
                Available = result.Available.Select(Available).ToArray(),
                Findings = result.Findings.Select(Finding).ToArray(),
                Counts = new ExtensionListJsonCounts
                {
                    Installed = result.Installed.Count,
                    Available = result.Available.Count,
                },
            },
            Next = result.Next is null
                ? null
                : new ExtensionListJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static ExtensionListJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy),
        };

    private static ExtensionListJsonSource Source(ExtensionListSource source)
        => new()
        {
            Identity = source.Identity,
            Kind = source.Kind is null ? null : SourceKind(source.Kind.Value),
            State = SourceState(source.State),
        };

    private static ExtensionListJsonInstalledRow Installed(ExtensionListInstalledRow row)
        => new()
        {
            Id = row.Id,
            Version = row.Version,
            Trust = Trust(row.Trust),
            ManagedPathCount = row.ManagedPathCount,
            SourceAvailable = row.SourceAvailable,
        };

    private static ExtensionListJsonAvailableRow Available(ExtensionListAvailableRow row)
        => new()
        {
            Id = row.Id,
            Name = row.Name,
            Description = row.Description,
            Version = row.Version,
            PackageCount = row.PackageCount,
            DependencyCount = row.DependencyCount,
        };

    private static ExtensionListJsonFinding Finding(ExtensionListFinding finding)
        => new()
        {
            Code = ExtensionListDefinitions.ReadFindingCode(finding.Code),
            Status = CliStatusDefinitions.Read(finding.Status).MachineName,
            Subject = finding.Subject,
            Cause = finding.Cause,
        };

    internal static string Coverage(ExtensionListCoverage coverage)
        => coverage switch
        {
            ExtensionListCoverage.Complete => "complete",
            ExtensionListCoverage.Incomplete => "incomplete",
            ExtensionListCoverage.Blocked => "blocked",
            ExtensionListCoverage.NotRequested => "not-requested",
            _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The Extension List coverage is not defined."),
        };

    internal static string Trust(LifecycleExtensionTrust trust)
        => trust switch
        {
            LifecycleExtensionTrust.Trusted => "trusted",
            LifecycleExtensionTrust.Untrusted => "untrusted",
            LifecycleExtensionTrust.Incomplete => "incomplete",
            LifecycleExtensionTrust.Blocked => "blocked",
            LifecycleExtensionTrust.Absent => "absent",
            _ => throw new ArgumentOutOfRangeException(nameof(trust), trust, "The lifecycle Extension trust is not defined."),
        };

    internal static string SourceKind(ExtensionSourceKind kind)
        => kind switch
        {
            ExtensionSourceKind.EmbeddedCatalogue => "embedded-catalogue",
            ExtensionSourceKind.Package => "package",
            ExtensionSourceKind.Catalogue => "catalogue",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Extension source kind is not defined."),
        };

    internal static string SourceState(ExtensionSourceReadState state)
        => state switch
        {
            ExtensionSourceReadState.Complete => "available",
            ExtensionSourceReadState.Missing => "missing",
            ExtensionSourceReadState.Invalid => "invalid",
            ExtensionSourceReadState.Blocked => "blocked",
            ExtensionSourceReadState.Unavailable => "unavailable",
            ExtensionSourceReadState.Cancelled => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension source state is not defined."),
        };
}
