using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Shared.Result;

internal static class ReferencesSourceFindingMapper
{
    internal static void AddSelectorFindings(
        ICollection<ReferencesFinding> findings,
        SourceUniverseFilterResolution resolution)
    {
        foreach (var selector in resolution.Selectors)
        {
            var code = selector.Reference.State switch
            {
                SourceReferenceResolutionState.Ambiguous => ReferencesFindingCode.SelectorAmbiguous,
                SourceReferenceResolutionState.Unsafe => ReferencesFindingCode.SelectorUnsafe,
                SourceReferenceResolutionState.Invalid
                    or SourceReferenceResolutionState.Unknown
                    or SourceReferenceResolutionState.Unsupported => ReferencesFindingCode.InvalidFilter,
                SourceReferenceResolutionState.Resolved => (ReferencesFindingCode?)null,
                _ => throw new ArgumentOutOfRangeException(nameof(selector), selector.Reference.State, "The source selector resolution is not defined."),
            };
            if (code is null)
            {
                continue;
            }

            ReferencesFindingFactory.AddFinding(
                findings,
                code.Value,
                ReferencesDirection.In,
                null,
                null,
                selector.Reference.CanonicalPath,
                null,
                null,
                selector.Reference.Cause ?? "The source selector could not be resolved.",
                selector.Reference.Candidates.Select(ToSourceIdentity),
                selector.Occurrence.Role,
                selector.RoleOccurrence,
                selector.Occurrence.Value);
        }
    }

    internal static void AddSourceResolutionFinding(
        ICollection<ReferencesFinding> findings,
        SourceReferenceResolution resolution)
    {
        var code = resolution.State switch
        {
            SourceReferenceResolutionState.Ambiguous => ReferencesFindingCode.SourceAmbiguous,
            SourceReferenceResolutionState.Unsafe => ReferencesFindingCode.SourceUnsafe,
            SourceReferenceResolutionState.Invalid
                or SourceReferenceResolutionState.Unknown
                or SourceReferenceResolutionState.Unsupported => ReferencesFindingCode.InvalidSource,
            SourceReferenceResolutionState.Resolved => (ReferencesFindingCode?)null,
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution.State, "The source resolution is not defined."),
        };
        if (code is null)
        {
            return;
        }

        ReferencesFindingFactory.AddFinding(
            findings,
            code.Value,
            null,
            null,
            null,
            resolution.CanonicalPath,
            null,
            null,
            resolution.Cause ?? "The source reference could not be resolved.",
            resolution.Candidates.Select(ToSourceIdentity),
            subject: resolution.Value);
    }

    internal static void AddRootIssues(
        ICollection<ReferencesFinding> findings,
        IEnumerable<SourceCatalogueIssue> issues)
    {
        foreach (var issue in issues)
        {
            var code = issue.Code switch
            {
                SourceCatalogueIssueCode.RootMissing => ReferencesFindingCode.WorkspaceUnavailable,
                SourceCatalogueIssueCode.RootUnsafe => ReferencesFindingCode.WorkspaceUnsafe,
                SourceCatalogueIssueCode.RootUnavailable => ReferencesFindingCode.WorkspaceUnavailable,
                _ => (ReferencesFindingCode?)null,
            };
            if (code is null)
            {
                continue;
            }

            ReferencesFindingFactory.AddFinding(
                findings,
                code.Value,
                null,
                null,
                null,
                issue.AttemptedCanonicalPath,
                null,
                null,
                "The selected workspace source root could not be established.");
        }
    }

    internal static void AddSourceCatalogueIssues(
        ICollection<ReferencesFinding> findings,
        SourceCatalogue catalogue,
        ReferencesSourceIdentity? source,
        IReadOnlySet<string>? sourcePaths,
        SourceCatalogueSelection? selection,
        ReferencesDirection? direction = null)
    {
        var issues = selection?.Issues ?? catalogue.Issues;
        foreach (var issue in issues)
        {
            if (sourcePaths is not null
                && issue.Stage is not SourceCatalogueIssueStage.Root
                && !sourcePaths.Contains(issue.AttemptedCanonicalPath)
                && !issue.RelatedPaths.Any(sourcePaths.Contains))
            {
                continue;
            }

            var code = issue.Code switch
            {
                SourceCatalogueIssueCode.IdentityCollision or SourceCatalogueIssueCode.PhysicalAlias
                    => ReferencesFindingCode.IdentityCollision,
                SourceCatalogueIssueCode.CandidateUnsafe => ReferencesFindingCode.CandidateUnsafe,
                SourceCatalogueIssueCode.CandidateUnavailable or SourceCatalogueIssueCode.DirectoryUnavailable
                    => ReferencesFindingCode.InspectionUnavailable,
                SourceCatalogueIssueCode.OrphanOverwrite => ReferencesFindingCode.LayerUnresolved,
                SourceCatalogueIssueCode.IdentityUnavailable => ReferencesFindingCode.IdentityCollision,
                _ => (ReferencesFindingCode?)null,
            };
            if (code is null)
            {
                continue;
            }

            ReferencesFindingFactory.AddFinding(
                findings,
                code.Value,
                direction,
                source,
                null,
                issue.AttemptedCanonicalPath,
                null,
                null,
                "The source catalogue retained an unresolved boundary.",
                ReadIssueCandidates(catalogue, issue));
        }
    }

    private static IReadOnlyList<ReferencesSourceIdentity> ReadIssueCandidates(
        SourceCatalogue catalogue,
        SourceCatalogueIssue issue)
        => issue.RelatedPaths
            .Select(path => catalogue.FindByPath(path))
            .Where(source => source is not null)
            .Cast<SourceLogicalSource>()
            .Select(ToSourceIdentity)
            .Distinct()
            .ToArray();

    private static ReferencesSourceIdentity ToSourceIdentity(SourceLogicalSource source)
        => new(source.Identity.AutomaticId, source.Identity.CanonicalBasePath);
}
