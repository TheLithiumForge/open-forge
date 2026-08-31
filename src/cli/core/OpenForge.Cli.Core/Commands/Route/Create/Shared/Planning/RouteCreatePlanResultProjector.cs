using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

internal sealed class RouteCreatePlanResultProjector
{
    internal RouteCreatePlanBuild Stop(
        RouteCreateRequest request,
        RouteCreatePlanningBoundary boundary)
        => new()
        {
            Plan = null,
            Formation = new RouteCreateResultFormation
            {
                Workspace = request.Workspace,
                Mode = request.Mode,
                Target = boundary.Target,
                Parent = boundary.Parent,
                Metadata = Metadata(request),
                Template = boundary.Template,
                Plan = new RouteCreatePlanFacts
                {
                    Completeness = boundary.IsIncomplete
                        ? RouteCreatePlanCompleteness.Incomplete
                        : RouteCreatePlanCompleteness.NotEstablished,
                    Safety = boundary.IsIncomplete
                        ? RouteCreatePlanSafety.NotEstablished
                        : RouteCreatePlanSafety.Blocked,
                },
                Effects = [],
                UnchangedPaths = [],
                Recovery = new RouteCreateRecovery
                {
                    State = RouteCreateRecoveryState.NotRequired,
                    ResidualPath = null,
                },
                Verification = RouteCreateVerificationState.NotRequested,
                Findings = [boundary.Finding],
            },
        };

    internal RouteCreateResultFormation Preview(RouteCreateCompletePlanStage stage)
    {
        var request = stage.Destination.Inspection.Request;
        var effectPaths = stage.FileChanges
            .Select(change => Relative(request, change.LogicalPath))
            .ToHashSet(StringComparer.Ordinal);
        var effects = stage.FileChanges
            .Select(change => Effect(stage, change))
            .ToImmutableArray();
        var unchangedPaths = stage.Destination.Inspection.Catalogue.Sources
            .SelectMany(Paths)
            .Where(path => !effectPaths.Contains(path))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToImmutableArray();
        var noOp = stage.FileChanges.IsEmpty;
        return new RouteCreateResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Target = stage.Destination.Inspection.Target,
            Parent = Parent(stage.Navigation.ParentSource),
            Metadata = Metadata(request),
            Template = stage.Destination.Template.Template,
            Plan = new RouteCreatePlanFacts
            {
                Completeness = RouteCreatePlanCompleteness.Complete,
                Safety = RouteCreatePlanSafety.Safe,
            },
            Effects = effects,
            UnchangedPaths = unchangedPaths,
            Recovery = new RouteCreateRecovery
            {
                State = stage.RecoveryTargets.IsEmpty
                    ? RouteCreateRecoveryState.NotRequired
                    : RouteCreateRecoveryState.NotCreated,
                ResidualPath = null,
            },
            Verification = noOp
                ? RouteCreateVerificationState.Verified
                : RouteCreateVerificationState.NotRequested,
            Findings = [],
        };
    }

    internal static RouteCreateParent Parent(SourceLogicalSource source)
        => new()
        {
            Id = source.Identity.AutomaticId,
            Path = source.Identity.CanonicalBasePath,
            Form = source.Base.Form == SourceDocumentForm.CanonicalEntrypoint
                ? RouteCreateParentForm.Canonical
                : RouteCreateParentForm.Compatibility,
        };

    private static RouteCreateEffect Effect(
        RouteCreateCompletePlanStage stage,
        PlannedFileChange change)
    {
        var isTarget = change.Kind == PlannedFileChangeKind.Create;
        var expected = isTarget
            ? FileExpectation.Hash(stage.Destination.IntendedBytes.AsSpan())
            : FileExpectation.Hash(stage.Navigation.Change.ExpectedDocumentBytes.AsSpan());
        return new RouteCreateEffect
        {
            Path = Relative(stage.Destination.Inspection.Request, change.LogicalPath),
            Kind = isTarget
                ? RouteCreateEffectKind.RoutedFile
                : RouteCreateEffectKind.GeneratedRegion,
            Action = isTarget
                ? RouteCreateEffectAction.Create
                : RouteCreateEffectAction.Replace,
            Change = new RouteCreateEffectChange
            {
                Before = isTarget
                    ? stage.Destination.Inspection.Snapshot.ContentHash
                    : stage.Navigation.ParentSnapshot.ContentHash,
                Expected = expected,
            },
            Outcome = RouteCreateEffectOutcome.Planned,
            Residual = RouteCreateEffectResidual.None,
        };
    }

    private static IEnumerable<string> Paths(SourceLogicalSource source)
    {
        yield return source.Identity.CanonicalBasePath;
        if (source.Overwrite is { } overwrite)
        {
            yield return overwrite.CanonicalPath;
        }
    }

    private static RouteCreateMetadata Metadata(RouteCreateRequest request)
        => new()
        {
            Description = request.Metadata.Description,
            Responsibility = request.Metadata.Responsibility,
            Tags = request.Metadata.Tags,
        };

    private static string Relative(RouteCreateRequest request, string absolutePath)
        => Path.GetRelativePath(request.Workspace.LexicalRoot, absolutePath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
}
