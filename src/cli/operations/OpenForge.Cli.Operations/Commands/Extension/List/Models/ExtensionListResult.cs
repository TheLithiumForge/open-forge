using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Extension.List.Models;

internal enum ExtensionListOwnershipTrust
{
    Trusted,
    Incomplete,
    Absent,
}

internal enum ExtensionListCoverage
{
    Complete,
    Incomplete,
    Blocked,
    NotRequested,
}

internal enum ExtensionListSourceKind
{
    EmbeddedCatalogue,
    Package,
    Catalogue,
}

internal enum ExtensionListSourceState
{
    Complete,
    Missing,
    Invalid,
    Blocked,
    Unavailable,
    Interrupted,
}

internal enum ExtensionListInstalledPackageState
{
    Current,
    Missing,
    VersionMismatch,
    DependencyMismatch,
    Ambiguous,
    SourceUnavailable,
}

internal enum ExtensionListInstalledFileState
{
    Current,
    Changed,
    Missing,
    Unavailable,
    Blocked,
}

internal enum ExtensionListInstalledUnavailableReason
{
    IntendedComparisonUnavailable,
    TargetReadUnavailable,
}

internal enum ExtensionListInstalledCoverage
{
    Complete,
    Changed,
    Missing,
    Incomplete,
    Blocked,
}

internal enum ExtensionListFindingCode
{
    InvalidInput,
    WorkspaceUnavailable,
    SourceUnavailable,
    SourceInvalid,
    SourceBlocked,
    InstalledSourceMissing,
    InstalledSourceUnavailable,
    InstalledSourceInvalid,
    InstalledSourceBlocked,
    InstalledFilesChanged,
    InstalledFilesMissing,
    InstalledTargetUnavailable,
    InstalledTargetBlocked,
    InstalledFilesUnavailable,
    OwnershipObservation,
    OperationFailed,
    Interrupted,
}

internal sealed record ExtensionListFinding
{
    internal ExtensionListFinding(
        ExtensionListFindingCode code,
        CliSemanticStatus status,
        string? subject,
        string cause)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension List finding code is not defined.");
        }

        _ = CliStatusDefinitions.Read(status);
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        Code = code;
        Status = status;
        Subject = subject;
        Cause = cause;
    }

    internal ExtensionListFindingCode Code { get; }

    internal string MachineCode => ExtensionListDefinitions.ReadFindingCode(Code);

    internal CliSemanticStatus Status { get; }

    internal string? Subject { get; }

    internal string Cause { get; }

    internal int? Count { get; init; }

    internal string? Path { get; init; }

    internal string? Owner { get; init; }
}

internal sealed record ExtensionListInstalledFile
{
    public required string Path { get; init; }

    public required ExtensionListInstalledFileState State { get; init; }

    public string? Cause { get; init; }

    internal ExtensionListInstalledUnavailableReason? UnavailableReason { get; init; }

    public IReadOnlyList<string> Owners { get; init; } = [];
}

internal sealed record ExtensionListInstalledRow
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required ExtensionListOwnershipTrust Trust { get; init; }

    public required int ManagedPathCount { get; init; }

    public required bool SourceAvailable { get; init; }

    public ExtensionListSourceState? SourceState { get; init; }

    public IReadOnlyList<string> Dependencies { get; init; } = [];

    public string? RecordedSource { get; init; }

    public ExtensionListInstalledPackageState PackageState { get; init; } = ExtensionListInstalledPackageState.Current;

    public ExtensionListInstalledCoverage Coverage { get; init; } = ExtensionListInstalledCoverage.Complete;

    public IReadOnlyList<ExtensionListInstalledFile> Files { get; init; } = [];
}

internal sealed record ExtensionListAvailableRow
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required int PackageCount { get; init; }

    public required int DependencyCount { get; init; }

    public IReadOnlyList<string> Dependencies { get; init; } = [];

    public string? InstalledVersion { get; init; }
}

internal sealed record ExtensionListSource
{
    public required string Identity { get; init; }

    public required ExtensionListSourceKind? Kind { get; init; }

    public required ExtensionListSourceState State { get; init; }
}

internal sealed record ExtensionListResult : ICliCommandResult
{
    internal ExtensionListResult(
        CliSemanticStatus status,
        CliWorkspace? workspace,
        ExtensionListSelection selection,
        ExtensionListSource? source,
        ExtensionListOwnershipTrust? lifecycleTrust,
        ExtensionListCoverage installedCoverage,
        ExtensionListCoverage availableCoverage,
        IEnumerable<ExtensionListInstalledRow> installed,
        IEnumerable<ExtensionListAvailableRow> available,
        IEnumerable<ExtensionListFinding> findings,
        CliNextAction? next)
    {
        _ = CliStatusDefinitions.Read(status);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(installed);
        ArgumentNullException.ThrowIfNull(available);
        ArgumentNullException.ThrowIfNull(findings);
        if (!Enum.IsDefined(installedCoverage) || !Enum.IsDefined(availableCoverage))
        {
            throw new ArgumentOutOfRangeException(nameof(installedCoverage), "An Extension List coverage state is not defined.");
        }

        var installedValues = installed.OrderBy(row => row.Id, StringComparer.Ordinal).ToArray();
        var availableValues = available.OrderBy(row => row.Id, StringComparer.Ordinal).ToArray();
        var findingValues = findings.ToArray();
        if (installedValues.Any(value => value is null)
            || availableValues.Any(value => value is null)
            || findingValues.Any(value => value is null))
        {
            throw new ArgumentException("Extension List result collections cannot contain null members.");
        }

        if (!selection.Installed && installedValues.Length != 0
            || !selection.Available && availableValues.Length != 0)
        {
            throw new ArgumentException("Extension List rows must match the requested sections.");
        }

        Status = status;
        Workspace = workspace;
        Selection = selection;
        Source = source;
        LifecycleTrust = lifecycleTrust;
        InstalledCoverage = installedCoverage;
        AvailableCoverage = availableCoverage;
        Installed = new ReadOnlyCollection<ExtensionListInstalledRow>(installedValues);
        Available = new ReadOnlyCollection<ExtensionListAvailableRow>(availableValues);
        Findings = new ReadOnlyCollection<ExtensionListFinding>(findingValues);
        Next = next;
    }

    public string Command => ExtensionListDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    internal ExtensionListSelection Selection { get; }

    internal ExtensionListSource? Source { get; }

    internal ExtensionListOwnershipTrust? LifecycleTrust { get; }

    internal ExtensionListCoverage InstalledCoverage { get; }

    internal ExtensionListCoverage AvailableCoverage { get; }

    internal IReadOnlyList<ExtensionListInstalledRow> Installed { get; }

    internal IReadOnlyList<ExtensionListAvailableRow> Available { get; }

    internal IReadOnlyList<ExtensionListFinding> Findings { get; }

    public CliNextAction? Next { get; }
}
