using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

internal sealed record RouteUpdateResult : ICliCommandResult
{
    internal RouteUpdateResult(
        RouteUpdateResultFormation formation,
        CliSemanticStatus status,
        CliNextAction? next)
    {
        ArgumentNullException.ThrowIfNull(formation);
        CliStatusDefinitions.Read(status);
        RouteUpdateDefinitions.ReadMachineName(formation.Mode);
        RouteUpdateDefinitions.ReadMachineName(formation.Plan.Completeness);
        RouteUpdateDefinitions.ReadMachineName(formation.Plan.Safety);
        RouteUpdateDefinitions.ReadMachineName(formation.Plan.Body);
        RouteUpdateDefinitions.ReadMachineName(formation.Recovery.State);
        RouteUpdateDefinitions.ReadMachineName(formation.Verification);
        ValidateTarget(formation.Target);
        ValidatePatch(formation.Patch);
        ValidateTemplate(formation.Template);
        ValidateRecovery(formation.Recovery);

        if (formation.Findings.IsDefault
            || formation.Findings.Any(finding => finding.Status == CliSemanticStatus.Complete))
        {
            throw new ArgumentException(
                "Route Update findings must be initialized and non-complete.",
                nameof(formation));
        }

        if ((status == CliSemanticStatus.Complete) != formation.Findings.IsEmpty)
        {
            throw new ArgumentException(
                "Route Update result status and findings are inconsistent.",
                nameof(formation));
        }

        Workspace = formation.Workspace;
        Mode = formation.Mode;
        Target = formation.Target;
        Patch = formation.Patch;
        Template = formation.Template;
        Plan = formation.Plan;
        Effects = formation.Effects.Select(ValidateEffect).ToImmutableArray();
        ValidateEffectPaths(Effects);
        UnchangedPaths = formation.UnchangedPaths;
        ValidateUnchangedPaths(UnchangedPaths, Effects);
        Recovery = formation.Recovery;
        Verification = formation.Verification;
        Findings = formation.Findings;
        Status = status;
        Next = next;
    }

    public string Command => RouteUpdateDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit
        => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    public CliNextAction? Next { get; }

    internal RouteUpdateMode Mode { get; }

    internal RouteUpdateTarget Target { get; }

    internal RouteUpdatePatch Patch { get; }

    internal RouteUpdateTemplate? Template { get; }

    internal RouteUpdatePlanFacts Plan { get; }

    internal ImmutableArray<RouteUpdateEffect> Effects { get; }

    internal ImmutableArray<string> UnchangedPaths { get; }

    internal RouteUpdateRecovery Recovery { get; }

    internal RouteUpdateVerificationState Verification { get; }

    internal ImmutableArray<RouteUpdateFinding> Findings { get; }

    private static void ValidateTarget(RouteUpdateTarget target)
    {
        if (string.IsNullOrWhiteSpace(target.Requested)
            || target.OverwritePaths.IsDefault
            || target.OverwritePaths.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Route Update target facts are invalid.");
        }

        if (target.SelectedBy is { } selectedBy)
        {
            RouteUpdateDefinitions.ReadMachineName(selectedBy);
        }

        if (target.Form is { } form)
        {
            RouteUpdateDefinitions.ReadMachineName(form);
        }
    }

    private static void ValidatePatch(RouteUpdatePatch patch)
    {
        RouteUpdateDefinitions.ReadMachineName(patch.Description.State);
        RouteUpdateDefinitions.ReadMachineName(patch.Responsibility.Operation);
        RouteUpdateDefinitions.ReadMachineName(patch.Responsibility.State);
        RouteUpdateDefinitions.ReadMachineName(patch.Tags.State);
        if (patch.Tags.Before is { IsDefault: true }
            || patch.Tags.Expected is { IsDefault: true })
        {
            throw new ArgumentException("Route Update tag patch arrays must be initialized when present.");
        }
    }

    private static void ValidateTemplate(RouteUpdateTemplate? template)
    {
        if (template is null)
        {
            return;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(template.Requested);
        RouteUpdateDefinitions.ReadMachineName(template.Decision);
        if (template.Classification is { } classification)
        {
            RouteUpdateDefinitions.ReadMachineName(classification);
        }

        if (template.BodyByteLength < 0)
        {
            throw new ArgumentException("A Route Update Template body length cannot be negative.");
        }
    }

    private static RouteUpdateEffect ValidateEffect(RouteUpdateEffect effect)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(effect.Path);
        ArgumentException.ThrowIfNullOrWhiteSpace(effect.Change.Before);
        ArgumentException.ThrowIfNullOrWhiteSpace(effect.Change.Expected);
        RouteUpdateDefinitions.ReadMachineName(effect.Kind);
        RouteUpdateDefinitions.ReadMachineName(effect.Action);
        RouteUpdateDefinitions.ReadMachineName(effect.Outcome);
        RouteUpdateDefinitions.ReadMachineName(effect.Residual);
        if (effect.Preview.IsDefault)
        {
            throw new ArgumentException("Route Update effect previews must be initialized.");
        }

        foreach (var preview in effect.Preview)
        {
            RouteUpdateDefinitions.ReadMachineName(preview.Kind);
        }

        return effect;
    }

    private static void ValidateEffectPaths(ImmutableArray<RouteUpdateEffect> effects)
    {
        if (effects.Select(effect => effect.Path).Distinct(StringComparer.Ordinal).Count()
            != effects.Length)
        {
            throw new ArgumentException("Route Update effects must identify distinct paths.");
        }

        if (effects.Length > 2
            || effects.Length == 2
                && (effects[0].Kind != RouteUpdateEffectKind.RoutedFile
                    || effects[1].Kind != RouteUpdateEffectKind.GeneratedRegion))
        {
            throw new ArgumentException(
                "Route Update effects must retain target-before-parent order.");
        }
    }

    private static void ValidateUnchangedPaths(
        ImmutableArray<string> unchangedPaths,
        ImmutableArray<RouteUpdateEffect> effects)
    {
        if (unchangedPaths.IsDefault
            || unchangedPaths.Any(string.IsNullOrWhiteSpace)
            || unchangedPaths.Distinct(StringComparer.Ordinal).Count() != unchangedPaths.Length
            || unchangedPaths.Zip(
                unchangedPaths.Skip(1),
                (prior, next) => string.CompareOrdinal(prior, next) >= 0)
                .Any(isOutOfOrder => isOutOfOrder)
            || effects.Any(effect => unchangedPaths.Contains(effect.Path, StringComparer.Ordinal)))
        {
            throw new ArgumentException(
                "Route Update unchanged paths must be initialized, unique, ordered, and disjoint from effects.");
        }
    }

    private static void ValidateRecovery(RouteUpdateRecovery recovery)
    {
        if (recovery.State == RouteUpdateRecoveryState.Retained
            && string.IsNullOrWhiteSpace(recovery.ResidualPath))
        {
            throw new ArgumentException(
                "A retained Route Update recovery artifact requires its path.");
        }

        if (recovery.State is (RouteUpdateRecoveryState.NotRequired
                or RouteUpdateRecoveryState.NotCreated
                or RouteUpdateRecoveryState.Removed)
            && recovery.ResidualPath is not null)
        {
            throw new ArgumentException(
                "A Route Update recovery state without a residual cannot expose a path.");
        }
    }
}
