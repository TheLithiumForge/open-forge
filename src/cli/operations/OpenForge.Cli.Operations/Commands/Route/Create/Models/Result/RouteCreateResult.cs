using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Result;

internal sealed record RouteCreateResult : ICliCommandResult
{
    internal RouteCreateResult(
        RouteCreateResultFormation formation,
        CliSemanticStatus status,
        CliNextAction? next)
    {
        CliStatusDefinitions.Read(status);
        if (!Enum.IsDefined(formation.Mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(formation),
                formation.Mode,
                "The Route Create mode is not defined.");
        }

        RouteCreateDefinitions.ReadMachineName(formation.Verification);
        RouteCreateDefinitions.ReadMachineName(formation.Plan.Completeness);
        RouteCreateDefinitions.ReadMachineName(formation.Plan.Safety);
        RouteCreateDefinitions.ReadMachineName(formation.Recovery.State);
        if (string.IsNullOrWhiteSpace(formation.Target.Requested))
        {
            throw new ArgumentException(
                "A Route Create result requires the attempted target.",
                nameof(formation));
        }

        if (formation.Parent is { } parent)
        {
            RouteCreateDefinitions.ReadMachineName(parent.Form);
            if (string.IsNullOrWhiteSpace(parent.Id)
                || string.IsNullOrWhiteSpace(parent.Path))
            {
                throw new ArgumentException(
                    "A resolved Route Create parent requires its ID and path.",
                    nameof(formation));
            }
        }

        if (formation.Template is { } template)
        {
            RouteCreateDefinitions.ReadMachineName(template.Classification);
            if (string.IsNullOrWhiteSpace(template.Requested)
                || string.IsNullOrWhiteSpace(template.Id)
                || string.IsNullOrWhiteSpace(template.Path)
                || template.BodyByteLength < 0)
            {
                throw new ArgumentException(
                    "Resolved Route Create Template facts are invalid.",
                    nameof(formation));
            }
        }

        var findingValues = formation.Findings;
        var allowsOptionalMetadata = findingValues.Any(
            finding => finding.Code == RouteCreateFindingCode.OptionalMetadata);
        ValidateMetadata(
            formation.Metadata,
            status,
            allowsOptionalMetadata,
            nameof(formation));
        ValidateRecovery(formation.Recovery, nameof(formation));
        if (findingValues.Any(finding => finding.Status == CliSemanticStatus.Complete))
        {
            throw new ArgumentException(
                "Route Create findings cannot have complete status.",
                nameof(formation));
        }

        if ((status == CliSemanticStatus.Complete) != (findingValues.Length == 0))
        {
            throw new ArgumentException(
                "Route Create result status and findings are inconsistent.",
                nameof(formation));
        }

        var recoveryRetainedFinding = findingValues.Any(
            finding => finding.Code == RouteCreateFindingCode.RecoveryArtifactRetained);
        var optionalMetadataFinding = findingValues.Any(
            finding => finding.Code == RouteCreateFindingCode.OptionalMetadata);
        var optionalAttention = status == CliSemanticStatus.Attention
            && optionalMetadataFinding
            && !recoveryRetainedFinding;
        if ((recoveryRetainedFinding
                && (status != CliSemanticStatus.Attention
                || formation.Verification != RouteCreateVerificationState.Verified
                || formation.Recovery.State != RouteCreateRecoveryState.Retained))
            || (status == CliSemanticStatus.Attention
                && !recoveryRetainedFinding
                && !optionalMetadataFinding)
            || (optionalAttention
                && (formation.Plan.Completeness != RouteCreatePlanCompleteness.Complete
                    || formation.Plan.Safety != RouteCreatePlanSafety.Safe
                    || formation.Verification is not (RouteCreateVerificationState.NotRequested
                        or RouteCreateVerificationState.Verified)
                    || formation.Recovery.State is RouteCreateRecoveryState.Retained
                        or RouteCreateRecoveryState.Unknown)))
        {
            throw new ArgumentException(
                "Route Create attention requires a safe optional-metadata plan or a positively retained recovery artifact.",
                nameof(formation));
        }

        Workspace = formation.Workspace;
        Mode = formation.Mode;
        Target = formation.Target;
        Parent = formation.Parent;
        Metadata = formation.Metadata;
        Template = formation.Template;
        Content = formation.Content;
        Sections = formation.Sections.IsDefault ? [] : formation.Sections;
        Plan = formation.Plan;
        Effects = formation.Effects.Select(ValidateEffect).ToImmutableArray();
        if (Effects.Select(effect => effect.Path).Distinct(StringComparer.Ordinal).Count()
            != Effects.Length)
        {
            throw new ArgumentException(
                "Route Create effects must identify distinct paths.",
                nameof(formation));
        }

        if (!HasValidEffectOrder(Effects))
        {
            throw new ArgumentException(
                "Route Create effects must retain destination-before-parent order.",
                nameof(formation));
        }

        UnchangedPaths = formation.UnchangedPaths;
        if (UnchangedPaths.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException(
                "Route Create unchanged paths cannot be blank.",
                nameof(formation));
        }

        if (Effects.Any(effect => UnchangedPaths.Contains(
                effect.Path,
                StringComparer.Ordinal)))
        {
            throw new ArgumentException(
                "Route Create changed and unchanged paths cannot overlap.",
                nameof(formation));
        }

        if (UnchangedPaths.Distinct(StringComparer.Ordinal).Count() != UnchangedPaths.Length
            || UnchangedPaths.Zip(
                UnchangedPaths.Skip(1),
                (prior, next) => string.CompareOrdinal(prior, next) >= 0)
                .Any(isOutOfOrder => isOutOfOrder))
        {
            throw new ArgumentException(
                "Route Create unchanged paths must be unique and ordinally ordered.",
                nameof(formation));
        }
        Recovery = formation.Recovery;
        Verification = formation.Verification;
        Findings = findingValues;
        Status = status;
        Next = next;
    }

    public string Command => RouteCreateDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    public CliNextAction? Next { get; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    internal RouteCreateMode Mode { get; }

    internal RouteCreateTarget Target { get; }

    internal RouteCreateParent? Parent { get; }

    internal RouteCreateMetadata Metadata { get; }

    internal RouteCreateTemplate? Template { get; }

    internal string? Content { get; }

    internal ImmutableArray<RouteCreateSection> Sections { get; }

    internal RouteCreatePlanFacts Plan { get; }

    internal ImmutableArray<RouteCreateEffect> Effects { get; }

    internal ImmutableArray<string> UnchangedPaths { get; }

    internal RouteCreateRecovery Recovery { get; }

    internal RouteCreateVerificationState Verification { get; }

    internal ImmutableArray<RouteCreateFinding> Findings { get; }

    private static RouteCreateEffect ValidateEffect(RouteCreateEffect effect)
    {
        RouteCreateDefinitions.ReadMachineName(effect.Kind);
        RouteCreateDefinitions.ReadMachineName(effect.Action);
        RouteCreateDefinitions.ReadMachineName(effect.Outcome);
        RouteCreateDefinitions.ReadMachineName(effect.Residual);
        if (string.IsNullOrWhiteSpace(effect.Path))
        {
            throw new ArgumentException("A Route Create effect path cannot be blank.");
        }

        if ((effect.Kind, effect.Action)
            is not (RouteCreateEffectKind.Directory, RouteCreateEffectAction.Create)
            and not (RouteCreateEffectKind.Entrypoint, RouteCreateEffectAction.Create)
            and not (RouteCreateEffectKind.RoutedFile, RouteCreateEffectAction.Create)
            and not (RouteCreateEffectKind.GeneratedRegion, RouteCreateEffectAction.Replace))
        {
            throw new ArgumentException(
                "A Route Create effect kind and action are inconsistent.");
        }

        var change = effect.Change;
        if (effect.Kind == RouteCreateEffectKind.Directory)
        {
            if (change is not null)
            {
                throw new ArgumentException(
                    "A Route Create directory effect cannot expose a file change.");
            }
        }
        else if (change is null
            || change.Expected is null
            || effect.Action == RouteCreateEffectAction.Create && change.Before is not null
            || effect.Action == RouteCreateEffectAction.Replace && change.Before is null)
        {
            throw new ArgumentException(
                "A Route Create effect change is inconsistent with its action.");
        }

        return effect;
    }

    private static bool HasValidEffectOrder(
        ImmutableArray<RouteCreateEffect> effects)
    {
        var lastRank = -1;
        var targetSeen = false;
        foreach (var effect in effects)
        {
            var rank = effect.Kind switch
            {
                RouteCreateEffectKind.Directory => 0,
                RouteCreateEffectKind.Entrypoint => 1,
                RouteCreateEffectKind.RoutedFile => 2,
                RouteCreateEffectKind.GeneratedRegion => 3,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(effects),
                    effect.Kind,
                    "The Route Create effect kind is not defined."),
            };
            if (rank < lastRank)
            {
                return false;
            }

            if (effect.Kind == RouteCreateEffectKind.RoutedFile)
            {
                if (targetSeen)
                {
                    return false;
                }

                targetSeen = true;
            }

            lastRank = rank;
        }

        return true;
    }

    private static void ValidateMetadata(
        RouteCreateMetadata metadata,
        CliSemanticStatus status,
        bool allowsOptionalMetadata,
        string parameterName)
    {
        var allowsUnresolvedMetadata = status == CliSemanticStatus.Invalid;
        var allowsPartialMetadata = allowsUnresolvedMetadata || allowsOptionalMetadata;
        var description = metadata.Description;
        if (description is null)
        {
            if (!allowsPartialMetadata)
            {
                throw new ArgumentException(
                    "A resolved Route Create result requires its destination description.",
                    parameterName);
            }
        }
        else if (description.Length == 0)
        {
            if (!allowsUnresolvedMetadata)
            {
                throw new ArgumentException(
                    "A resolved Route Create result requires its destination description.",
                    parameterName);
            }
        }
        else if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "A present Route Create description cannot be blank.",
                parameterName);
        }

        if (metadata.Responsibility is not null
            && string.IsNullOrWhiteSpace(metadata.Responsibility))
        {
            throw new ArgumentException(
                "A present Route Create responsibility cannot be blank.",
                parameterName);
        }

        if (metadata.Tags.IsDefault
            || !allowsPartialMetadata && metadata.Tags.IsEmpty
            || metadata.Tags.Any(tag => !FrameworkDocumentMetadataTagGrammar.IsValid(tag))
            || metadata.Tags.Distinct(StringComparer.Ordinal).Count() != metadata.Tags.Length)
        {
            throw new ArgumentException(
                "Route Create result tags must be initialized, canonical, and unique.",
                parameterName);
        }
    }

    private static void ValidateRecovery(
        RouteCreateRecovery recovery,
        string parameterName)
    {
        if (recovery.State == RouteCreateRecoveryState.Retained
            && string.IsNullOrWhiteSpace(recovery.ResidualPath))
        {
            throw new ArgumentException(
                "A retained Route Create recovery artifact requires its path.",
                parameterName);
        }

        if (recovery.State is (RouteCreateRecoveryState.NotRequired
                or RouteCreateRecoveryState.NotCreated
                or RouteCreateRecoveryState.Removed)
            && recovery.ResidualPath is not null)
        {
            throw new ArgumentException(
                "A Route Create recovery state without a residual cannot expose a path.",
                parameterName);
        }
    }
}
