using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;

internal sealed record ExtensionUpdateResult : ICliCommandResult
{
    internal ExtensionUpdateResult(ExtensionUpdateResultFormation formation)
    {
        ArgumentNullException.ThrowIfNull(formation);
        ArgumentNullException.ThrowIfNull(formation.Facts);
        ArgumentNullException.ThrowIfNull(formation.Facts.Packages);
        ArgumentNullException.ThrowIfNull(formation.Facts.Comparisons);
        ArgumentNullException.ThrowIfNull(formation.Facts.Effects);
        ArgumentNullException.ThrowIfNull(formation.Facts.Lifecycle);
        ArgumentNullException.ThrowIfNull(formation.Facts.Recovery);
        ArgumentNullException.ThrowIfNull(formation.Facts.Verification);
        ArgumentNullException.ThrowIfNull(formation.Findings);
        if (!Enum.IsDefined(formation.Mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(formation),
                formation.Mode,
                "The Extension Update mode is not defined.");
        }

        Workspace = formation.Workspace;
        Mode = formation.Mode;
        Force = formation.Force;
        Prune = formation.Prune;
        Automatic = formation.Automatic;
        Selection = formation.Facts.Selection;
        Source = formation.Facts.Source;
        Packages = Snapshot(formation.Facts.Packages, nameof(formation.Facts.Packages));
        Comparisons = Snapshot(formation.Facts.Comparisons, nameof(formation.Facts.Comparisons));
        GeneratedNavigation = formation.Facts.GeneratedNavigation;
        Effects = Snapshot(formation.Facts.Effects, nameof(formation.Facts.Effects));
        Permissions = formation.Facts.Permissions;
        Lifecycle = formation.Facts.Lifecycle;
        Recovery = formation.Facts.Recovery;
        Verification = formation.Facts.Verification;
        Findings = new ReadOnlyCollection<ExtensionUpdateFinding>([.. formation.Findings
            .Select(value => value ?? throw new ArgumentException(
                "Extension Update findings cannot contain null members.",
                nameof(formation)))
            .OrderBy(value => value.Code)
            .ThenBy(value => value.Target is null ? 0 : 1)
            .ThenBy(value => value.Target, StringComparer.Ordinal)
            .ThenBy(value => value.Cause, StringComparer.Ordinal)]);
        Status = ReadStatus(Findings);
        Next = ExtensionUpdateDefinitions.ReadNextAction(
            Status,
            Findings,
            Force,
            Prune,
            Automatic,
            Mode);
    }

    public string Command => ExtensionUpdateDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    public CliNextAction? Next { get; }

    internal ExtensionUpdateMode Mode { get; }

    internal bool Force { get; }

    internal bool Prune { get; }

    internal bool Automatic { get; }

    internal ExtensionUpdateSelection? Selection { get; }

    internal ExtensionUpdateSource? Source { get; }

    internal IReadOnlyList<ExtensionUpdatePackage> Packages { get; }

    internal IReadOnlyList<ExtensionUpdateComparison> Comparisons { get; }

    internal ExtensionUpdateGeneratedNavigation? GeneratedNavigation { get; }

    internal IReadOnlyList<ExtensionUpdateEffect> Effects { get; }

    internal WorkspacePermissionResult Permissions { get; init; }

    internal ExtensionUpdateLifecycle Lifecycle { get; }

    internal ExtensionUpdateRecovery Recovery { get; }

    internal ExtensionUpdateVerification Verification { get; }

    internal IReadOnlyList<ExtensionUpdateFinding> Findings { get; }

    internal static ExtensionUpdateResult Empty(
        CliWorkspace? workspace,
        ExtensionUpdateMode mode,
        bool force,
        bool prune,
        bool automatic,
        params ExtensionUpdateFinding[] findings)
        => new(new ExtensionUpdateResultFormation
        {
            Workspace = workspace,
            Mode = mode,
            Force = force,
            Prune = prune,
            Automatic = automatic,
            Facts = new ExtensionUpdateResultFacts
            {
                Selection = null,
                Source = null,
                Packages = [],
                Comparisons = [],
                GeneratedNavigation = null,
                Effects = [],
                Lifecycle = new ExtensionUpdateLifecycle(
                    ExtensionUpdateLifecycleTrust.NotRequested,
                    ExtensionUpdateLifecycleCoverage.NotRequested,
                    ExtensionUpdateLifecycleAction.None,
                    ExtensionUpdateLifecycleOutcome.NotRequested),
                Recovery = new ExtensionUpdateRecovery(
                    ExtensionUpdateRecoveryState.NotRequired,
                    [],
                    residualPath: null),
                Verification = new ExtensionUpdateVerification(
                    ExtensionUpdateVerificationState.NotRequested,
                    ExtensionUpdateVerificationState.NotRequested,
                    ExtensionUpdateVerificationState.NotRequested),
            },
            Findings = findings ?? throw new ArgumentNullException(nameof(findings)),
        });

    private static CliSemanticStatus ReadStatus(
        IReadOnlyList<ExtensionUpdateFinding> findings)
    {
        CliSemanticStatus[] precedence =
        [
            CliSemanticStatus.Failed,
            CliSemanticStatus.Interrupted,
            CliSemanticStatus.Invalid,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Attention,
        ];
        return precedence.FirstOrDefault(
            status => findings.Any(finding => finding.Status == status),
            CliSemanticStatus.Complete);
    }

    private static ReadOnlyCollection<T> Snapshot<T>(
        IEnumerable<T> values,
        string parameterName)
        where T : class
        => new([.. values
            .Select(value => value ?? throw new ArgumentException(
                "Extension Update result collections cannot contain null members.",
                parameterName))]);
}
