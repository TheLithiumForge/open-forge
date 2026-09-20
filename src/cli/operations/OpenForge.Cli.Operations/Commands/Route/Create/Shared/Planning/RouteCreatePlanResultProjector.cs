using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
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
                Content = null,
                Sections = [],
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
                Findings = Findings(
                    request,
                    boundary.Target.Path ?? boundary.Target.Requested,
                    boundary.Finding),
            },
        };

    internal RouteCreateResultFormation Preview(RouteCreateCompletePlanStage stage)
    {
        var request = stage.Destination.Inspection.Request;
        var effectPaths = stage.FileChanges
            .Select(change => Relative(request, change.LogicalPath))
            .ToHashSet(StringComparer.Ordinal);
        var effects = new List<RouteCreateEffect>(
            stage.DirectoryCreations.Length
            + stage.Navigation.NavigationChanges.Count(change => change.IsCreate)
            + stage.FileChanges.Length);
        effects.AddRange(stage.DirectoryCreations.Select(creation => DirectoryEffect(request, creation)));
        effects.AddRange(stage.Navigation.NavigationChanges
            .Where(change => change.IsCreate)
            .Select(change => Effect(stage, change)));
        if (stage.Destination.Inspection.Snapshot.Kind == FileExpectationKind.Missing)
        {
            effects.Add(TargetEffect(stage));
        }

        effects.AddRange(stage.Navigation.NavigationChanges
            .Where(change => !change.IsCreate && change.RequiresFileChange)
            .OrderBy(change => change.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .Select(change => Effect(stage, change)));

        var unchangedPaths = stage.Destination.Inspection.Catalogue.Sources
            .SelectMany(Paths)
            .Where(path => !effectPaths.Contains(path))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToImmutableArray();
        var noOp = stage.DirectoryCreations.IsEmpty && stage.FileChanges.IsEmpty;
        return new RouteCreateResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Target = stage.Destination.Inspection.Target,
            Parent = Parent(stage.Navigation.ParentSource),
            Metadata = Metadata(request),
            Template = stage.Destination.Template.Template,
            Content = Encoding.UTF8.GetString(stage.Destination.IntendedBytes.AsSpan()),
            Sections = stage.Navigation.NavigationChanges
                .Where(change => !change.IsCreate)
                .OrderBy(change => change.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .Select(Section)
                .ToImmutableArray(),
            Plan = new RouteCreatePlanFacts
            {
                Completeness = RouteCreatePlanCompleteness.Complete,
                Safety = RouteCreatePlanSafety.Safe,
            },
            Effects = effects.ToImmutableArray(),
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
            Findings = Findings(
                request,
                stage.Destination.Inspection.Target.Path
                    ?? stage.Destination.Inspection.Target.Requested),
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

    private static RouteCreateEffect DirectoryEffect(
        RouteCreateRequest request,
        PlannedDirectoryCreation creation)
        => new()
        {
            Path = Relative(request, creation.LogicalPath),
            Kind = RouteCreateEffectKind.Directory,
            Action = RouteCreateEffectAction.Create,
            Change = null,
            Outcome = RouteCreateEffectOutcome.Planned,
            Residual = RouteCreateEffectResidual.None,
        };

    private static RouteCreateEffect Effect(
        RouteCreateCompletePlanStage stage,
        RouteCreateNavigationChange change)
        => new()
        {
            Path = Relative(
                stage.Destination.Inspection.Request,
                change.Before.LogicalPath),
            Kind = change.IsCreate
                ? RouteCreateEffectKind.Entrypoint
                : RouteCreateEffectKind.GeneratedRegion,
            Action = change.IsCreate
                ? RouteCreateEffectAction.Create
                : RouteCreateEffectAction.Replace,
            Change = new RouteCreateEffectChange
            {
                Before = change.IsCreate ? null : change.Before.ContentHash,
                Expected = FileExpectation.Hash(change.IntendedBytes.AsSpan()),
            },
            Outcome = RouteCreateEffectOutcome.Planned,
            Residual = RouteCreateEffectResidual.None,
        };

    private static RouteCreateEffect TargetEffect(RouteCreateCompletePlanStage stage)
        => new()
        {
            Path = Relative(
                stage.Destination.Inspection.Request,
                stage.Destination.Inspection.Snapshot.LogicalPath),
            Kind = RouteCreateEffectKind.RoutedFile,
            Action = RouteCreateEffectAction.Create,
            Change = new RouteCreateEffectChange
            {
                Before = null,
                Expected = FileExpectation.Hash(stage.Destination.IntendedBytes.AsSpan()),
            },
            Outcome = RouteCreateEffectOutcome.Planned,
            Residual = RouteCreateEffectResidual.None,
        };

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

    private static ImmutableArray<RouteCreateFinding> Findings(
        RouteCreateRequest request,
        string target,
        RouteCreateFinding? boundary = null)
    {
        var findings = ImmutableArray.CreateBuilder<RouteCreateFinding>(boundary is null ? 1 : 2);
        if (request.Metadata.Description is null || request.Metadata.Tags.IsEmpty)
        {
            findings.Add(new RouteCreateFinding(
                RouteCreateFindingCode.OptionalMetadata,
                "The Route Create target omits an optional description or tag.",
                target));
        }

        if (boundary is not null)
        {
            findings.Add(boundary);
        }

        return findings.ToImmutable();
    }

    private static RouteCreateSection Section(RouteCreateNavigationChange navigation)
    {
        var change = navigation.RegionChange
            ?? throw new InvalidOperationException(
                "An existing generated navigation section requires its bounded change.");
        return new RouteCreateSection
        {
            Path = navigation.Source.Identity.CanonicalBasePath,
            Before = FileExpectation.Hash(Encoding.UTF8.GetBytes(change.BeforeBody).AsSpan()),
            After = FileExpectation.Hash(Encoding.UTF8.GetBytes(change.ExpectedBody).AsSpan()),
        };
    }

    private static string Relative(RouteCreateRequest request, string absolutePath)
        => Path.GetRelativePath(request.Workspace.LexicalRoot, absolutePath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
}
