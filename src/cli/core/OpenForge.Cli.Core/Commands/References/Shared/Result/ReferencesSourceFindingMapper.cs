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
                new ReferencesFindingInput(
                    code.Value,
                    selector.Reference.Cause ?? "The source selector could not be resolved.")
                {
                    Direction = ReferencesDirection.In,
                    Path = selector.Reference.CanonicalPath,
                    Candidates = selector.Reference.Candidates.Select(ToSourceIdentity),
                    SelectorRole = selector.Occurrence.Role,
                    SelectorOccurrence = selector.RoleOccurrence,
                    Subject = selector.Occurrence.Value,
                });
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
            new ReferencesFindingInput(
                code.Value,
                resolution.Cause ?? "The source reference could not be resolved.")
            {
                Path = resolution.CanonicalPath,
                Candidates = resolution.Candidates.Select(ToSourceIdentity),
                Subject = resolution.Value,
            });
    }

    internal static void AddRootIssues(
        ICollection<ReferencesFinding> findings,
        IEnumerable<SourceCatalogueIssue> issues)
    {
        foreach (var issue in issues)
        {
            var code = ReadRootIssueFindingCode(issue.Code);
            if (code is null)
            {
                continue;
            }

            ReferencesFindingFactory.AddFinding(
                findings,
                new ReferencesFindingInput(
                    code.Value,
                    "The selected workspace source root could not be established.")
                {
                    Path = issue.AttemptedCanonicalPath,
                });
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

            var code = ReadSourceCatalogueFindingCode(issue.Code);
            if (code is null)
            {
                continue;
            }

            ReferencesFindingFactory.AddFinding(
                findings,
                new ReferencesFindingInput(
                    code.Value,
                    "The source catalogue retained an unresolved boundary.")
                {
                    Direction = direction,
                    Source = source,
                    Path = issue.AttemptedCanonicalPath,
                    Candidates = ReadIssueCandidates(catalogue, issue),
                });
        }
    }

    internal static ReferencesFindingCode? ReadRootIssueFindingCode(SourceCatalogueIssueCode code)
        => code switch
        {
            SourceCatalogueIssueCode.RootMissing => ReferencesFindingCode.WorkspaceUnavailable,
            SourceCatalogueIssueCode.RootUnsafe => ReferencesFindingCode.WorkspaceUnsafe,
            SourceCatalogueIssueCode.RootUnavailable => ReferencesFindingCode.WorkspaceUnavailable,
            SourceCatalogueIssueCode.DirectoryUnavailable => null,
            SourceCatalogueIssueCode.CandidateUnsafe => null,
            SourceCatalogueIssueCode.CandidateUnavailable => null,
            SourceCatalogueIssueCode.IdentityUnavailable => null,
            SourceCatalogueIssueCode.IdentityCollision => null,
            SourceCatalogueIssueCode.PhysicalAlias => null,
            SourceCatalogueIssueCode.OrphanOverwrite => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The source catalogue issue code is not defined."),
        };

    internal static ReferencesFindingCode? ReadSourceCatalogueFindingCode(
        SourceCatalogueIssueCode code)
        => code switch
        {
            SourceCatalogueIssueCode.RootMissing => null,
            SourceCatalogueIssueCode.RootUnsafe => null,
            SourceCatalogueIssueCode.RootUnavailable => null,
            SourceCatalogueIssueCode.DirectoryUnavailable => ReferencesFindingCode.InspectionUnavailable,
            SourceCatalogueIssueCode.CandidateUnsafe => ReferencesFindingCode.CandidateUnsafe,
            SourceCatalogueIssueCode.CandidateUnavailable => ReferencesFindingCode.InspectionUnavailable,
            SourceCatalogueIssueCode.IdentityUnavailable => ReferencesFindingCode.IdentityCollision,
            SourceCatalogueIssueCode.IdentityCollision => ReferencesFindingCode.IdentityCollision,
            SourceCatalogueIssueCode.PhysicalAlias => ReferencesFindingCode.IdentityCollision,
            SourceCatalogueIssueCode.OrphanOverwrite => ReferencesFindingCode.LayerUnresolved,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The source catalogue issue code is not defined."),
        };

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
