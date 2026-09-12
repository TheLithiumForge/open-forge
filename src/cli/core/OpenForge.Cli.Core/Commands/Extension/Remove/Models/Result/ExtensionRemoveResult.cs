using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;

internal sealed record ExtensionRemoveResult : ICliCommandResult
{
    internal ExtensionRemoveResult(ExtensionRemoveResultFormation formation)
    {
        ArgumentNullException.ThrowIfNull(formation);
        ArgumentNullException.ThrowIfNull(formation.Facts);
        ArgumentNullException.ThrowIfNull(formation.Facts.Paths);
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
                "The Extension Remove mode is not defined.");
        }

        Workspace = formation.Workspace;
        Mode = formation.Mode;
        Prune = formation.Prune;
        Automatic = formation.Automatic;
        Selection = formation.Facts.Selection;
        Dependencies = formation.Facts.Dependencies;
        Paths = Snapshot(formation.Facts.Paths, nameof(formation.Facts.Paths));
        GeneratedNavigation = formation.Facts.GeneratedNavigation;
        Effects = Snapshot(formation.Facts.Effects, nameof(formation.Facts.Effects));
        Permissions = formation.Facts.Permissions;
        Lifecycle = formation.Facts.Lifecycle;
        Recovery = formation.Facts.Recovery;
        Verification = formation.Facts.Verification;
        PackageSourceUnchanged = formation.Facts.PackageSourceUnchanged;
        Findings = new ReadOnlyCollection<ExtensionRemoveFinding>([.. formation.Findings
            .Select(value => value ?? throw new ArgumentException(
                "Extension Remove findings cannot contain null members.",
                nameof(formation)))
            .OrderBy(value => value.Code)
            .ThenBy(value => value.Target is null ? 0 : 1)
            .ThenBy(value => value.Target, StringComparer.Ordinal)
            .ThenBy(value => value.Cause, StringComparer.Ordinal)]);
        Status = ReadStatus(Findings);
        Next = ExtensionRemoveDefinitions.ReadNextAction(Status, Findings);
    }

    public string Command => ExtensionRemoveDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    public CliNextAction? Next { get; }

    internal ExtensionRemoveMode Mode { get; }

    internal bool Prune { get; }

    internal bool Automatic { get; }

    internal ExtensionRemoveSelection? Selection { get; }

    internal ExtensionRemoveDependencyPlan? Dependencies { get; }

    internal IReadOnlyList<ExtensionRemovePathPlan> Paths { get; }

    internal ExtensionRemoveGeneratedNavigation? GeneratedNavigation { get; }

    internal IReadOnlyList<ExtensionRemoveEffect> Effects { get; }

    internal WorkspacePermissionResult Permissions { get; init; }

    internal ExtensionRemoveLifecycle Lifecycle { get; }

    internal ExtensionRemoveRecovery Recovery { get; }

    internal ExtensionRemoveVerification Verification { get; }

    internal bool PackageSourceUnchanged { get; }

    internal IReadOnlyList<ExtensionRemoveFinding> Findings { get; }

    internal static ExtensionRemoveResult Empty(
        CliWorkspace? workspace,
        ExtensionRemoveMode mode,
        bool prune,
        bool automatic,
        params ExtensionRemoveFinding[] findings)
        => new(new ExtensionRemoveResultFormation
        {
            Workspace = workspace,
            Mode = mode,
            Prune = prune,
            Automatic = automatic,
            Facts = new ExtensionRemoveResultFacts
            {
                Selection = null,
                Dependencies = null,
                Paths = [],
                GeneratedNavigation = null,
                Effects = [],
                Lifecycle = new ExtensionRemoveLifecycle(
                    ExtensionRemoveLifecycleTrust.NotRequested,
                    ExtensionRemoveLifecycleCoverage.NotRequested,
                    ExtensionRemoveLifecycleAction.None,
                    ExtensionRemoveLifecycleOutcome.NotRequested),
                Recovery = new ExtensionRemoveRecovery(
                    ExtensionRemoveRecoveryState.NotRequired,
                    [],
                    residualPath: null),
                Verification = new ExtensionRemoveVerification(
                    ExtensionRemoveVerificationState.NotRequested,
                    ExtensionRemoveVerificationState.NotRequested,
                    ExtensionRemoveVerificationState.NotRequested),
                PackageSourceUnchanged = true,
            },
            Findings = findings ?? throw new ArgumentNullException(nameof(findings)),
        });

    private static ReadOnlyCollection<T> Snapshot<T>(
        IEnumerable<T> values,
        string parameterName)
        where T : class
        => new([.. values
            .Select(value => value ?? throw new ArgumentException(
                "Extension Remove result collections cannot contain null members.",
                parameterName))]);

    private static CliSemanticStatus ReadStatus(
        IReadOnlyList<ExtensionRemoveFinding> findings)
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
}
