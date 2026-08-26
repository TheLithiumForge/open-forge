using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Request;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Extension.List.Models.Result;

internal enum ExtensionListCoverage
{
    Complete,
    Incomplete,
    Blocked,
    NotRequested,
}

internal enum ExtensionListFindingCode
{
    InvalidInput,
    WorkspaceUnavailable,
    SourceUnavailable,
    SourceInvalid,
    SourceBlocked,
    LifecycleUnavailable,
    LifecycleBlocked,
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

    internal CliSemanticStatus Status { get; }

    internal string? Subject { get; }

    internal string Cause { get; }
}

internal sealed record ExtensionListInstalledRow
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required LifecycleExtensionTrust Trust { get; init; }

    public required int ManagedPathCount { get; init; }

    public required bool SourceAvailable { get; init; }
}

internal sealed record ExtensionListAvailableRow
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required int PackageCount { get; init; }

    public required int DependencyCount { get; init; }
}

internal sealed record ExtensionListSource
{
    public required string Identity { get; init; }

    public required ExtensionSourceKind? Kind { get; init; }

    public required ExtensionSourceReadState State { get; init; }
}

internal sealed record ExtensionListResult : ICliCommandResult
{
    internal ExtensionListResult(
        CliSemanticStatus status,
        CliWorkspace? workspace,
        ExtensionListSelection selection,
        ExtensionListSource? source,
        LifecycleExtensionTrust? lifecycleTrust,
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

    internal ExtensionListSelection Selection { get; }

    internal ExtensionListSource? Source { get; }

    internal LifecycleExtensionTrust? LifecycleTrust { get; }

    internal ExtensionListCoverage InstalledCoverage { get; }

    internal ExtensionListCoverage AvailableCoverage { get; }

    internal IReadOnlyList<ExtensionListInstalledRow> Installed { get; }

    internal IReadOnlyList<ExtensionListAvailableRow> Available { get; }

    internal IReadOnlyList<ExtensionListFinding> Findings { get; }

    public CliNextAction? Next { get; }
}
