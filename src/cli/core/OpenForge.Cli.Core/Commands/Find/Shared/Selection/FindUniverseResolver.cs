using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Selection;

internal sealed class FindUniverseResolver
{
    private readonly SourceUniverseFilterResolver _filterResolver;

    internal FindUniverseResolver(SourceUniverseFilterResolver filterResolver)
    {
        ArgumentNullException.ThrowIfNull(filterResolver);
        _filterResolver = filterResolver;
    }

    internal FindUniverseResolver(SourcePhysicalPathResolver physicalPathResolver)
        : this(new SourceUniverseFilterResolver(new SourceReferenceResolver(physicalPathResolver)))
    {
    }

    internal FindUniverseResolution Resolve(FindUniverseInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var request = input.Request;
        var catalogue = input.SourceContext.Catalogue;
        var occurrences = CreateOccurrences(
            request.UniverseFilter.Include,
            request.UniverseFilter.Exclude);
        var resolution = _filterResolver.Resolve(new SourceUniverseFilterRequest(
            catalogue,
            input.SourceContext.DefaultSelectionScope,
            occurrences));
        var selectors = resolution.Selectors
            .Select(selector => ProjectSelector(selector, catalogue))
            .ToArray();
        var include = selectors
            .Where(selector => selector.Role == FindSelectorRole.Include)
            .ToArray();
        var exclude = selectors
            .Where(selector => selector.Role == FindSelectorRole.Exclude)
            .ToArray();
        var findings = selectors
            .Select(CreateSelectorFinding)
            .Where(finding => finding is not null)
            .Cast<FindFinding>()
            .Concat(ProjectCatalogueFindings(catalogue, resolution.Selection))
            .ToList();
        if (catalogue.IsCancelled)
        {
            findings.Add(new FindFinding(
                FindFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                null,
                "Source catalogue discovery was interrupted.",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                []));
        }

        var universe = new FindUniverse(
            occurrences.Count == 0 ? FindUniverseMode.Default : FindUniverseMode.Filtered,
            include.Select(selector => selector.Selector),
            exclude.Select(selector => selector.Selector),
            resolution.IsResolved ? ReadCandidateCount(catalogue, resolution.Selection) : null,
            null,
            null);
        return new FindUniverseResolution(
            universe,
            resolution.Selection,
            OrderFindings(findings));
    }

    private static IReadOnlyList<SourceUniverseSelectorOccurrence> CreateOccurrences(
        IReadOnlyList<string> include,
        IReadOnlyList<string> exclude)
    {
        var occurrences = new List<SourceUniverseSelectorOccurrence>(include.Count + exclude.Count);
        foreach (var value in include)
        {
            occurrences.Add(new SourceUniverseSelectorOccurrence(
                SourceUniverseSelectorRole.Include,
                value,
                occurrences.Count + 1));
        }

        foreach (var value in exclude)
        {
            occurrences.Add(new SourceUniverseSelectorOccurrence(
                SourceUniverseSelectorRole.Exclude,
                value,
                occurrences.Count + 1));
        }

        return occurrences;
    }

    private static ProjectedSelector ProjectSelector(
        SourceUniverseSelectorResolution selector,
        SourceCatalogue catalogue)
    {
        var source = selector.Reference.Source;
        var projected = new FindSelector(
            selector.Occurrence.Value,
            selector.Reference.Form,
            ReadSelectorResolution(selector.Reference.State),
            source is null ? null : CreateIdentity(source),
            source is null ? null : ReadSourceKind(source.Base.Form),
            ReadExpansion(selector.Expansion),
            selector.Reference.Candidates.Select(CreateIdentity));
        return new ProjectedSelector(
            selector.Occurrence.Role == SourceUniverseSelectorRole.Include
                ? FindSelectorRole.Include
                : FindSelectorRole.Exclude,
            selector.RoleOccurrence,
            projected,
            selector.Reference.CanonicalPath,
            ReadFindCause(selector.Reference, catalogue));
    }

    private static FindSelectorResolution ReadSelectorResolution(SourceReferenceResolutionState state)
        => state switch
        {
            SourceReferenceResolutionState.Resolved => FindSelectorResolution.Resolved,
            SourceReferenceResolutionState.Invalid => FindSelectorResolution.Invalid,
            SourceReferenceResolutionState.Unknown => FindSelectorResolution.Unknown,
            SourceReferenceResolutionState.Unsupported => FindSelectorResolution.Unsupported,
            SourceReferenceResolutionState.Ambiguous => FindSelectorResolution.Ambiguous,
            SourceReferenceResolutionState.Unsafe => FindSelectorResolution.Unsafe,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The source-reference resolution state is not defined."),
        };

    private static FindSelectorExpansion? ReadExpansion(SourceUniverseSelectorExpansion? expansion)
        => expansion switch
        {
            SourceUniverseSelectorExpansion.Source => FindSelectorExpansion.Source,
            SourceUniverseSelectorExpansion.Folder => FindSelectorExpansion.Folder,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(expansion), expansion, "The source-universe expansion is not defined."),
        };

    private static string? ReadFindCause(
        SourceReferenceResolution reference,
        SourceCatalogue catalogue)
    {
        if (reference.State == SourceReferenceResolutionState.Resolved)
        {
            return null;
        }

        if (reference.Form == SourceReferenceKind.SourcePath
            && reference.CanonicalPath is { } path)
        {
            return reference.State switch
            {
                SourceReferenceResolutionState.Unsupported => "The exact path is not an admitted logical Find source.",
                SourceReferenceResolutionState.Unknown when catalogue.FindCandidateByPath(path) is not null
                    => "The exact source path is no longer present.",
                SourceReferenceResolutionState.Unknown => "The exact source path does not exist.",
                SourceReferenceResolutionState.Unsafe => "The exact source path is outside an established safe physical boundary.",
                _ => reference.Cause,
            };
        }

        return reference.Cause;
    }

    private static int? ReadCandidateCount(
        SourceCatalogue catalogue,
        SourceCatalogueSelection selection)
    {
        if (catalogue.IsCancelled
            || selection.RootIssues.Count != 0
            || selection.Issues.Any(issue => issue.Code == SourceCatalogueIssueCode.DirectoryUnavailable))
        {
            return null;
        }

        var sourceLessCount = selection.Candidates.Count(
            candidate => catalogue.FindByPath(candidate.CanonicalPath) is null);
        return selection.Sources.Count + sourceLessCount;
    }

    private static IEnumerable<FindFinding> ProjectCatalogueFindings(
        SourceCatalogue catalogue,
        SourceCatalogueSelection selection)
    {
        foreach (var issue in selection.RootIssues.Concat(selection.Issues))
        {
            var code = ReadFindingCode(issue.Code);
            if (code is null)
            {
                continue;
            }

            var candidates = issue.Code == SourceCatalogueIssueCode.IdentityCollision
                ? ReadCollisionCandidates(catalogue, issue)
                : [];
            yield return new FindFinding(
                code.Value,
                FindDefinitions.ReadFindingStatus(code.Value),
                issue.AttemptedCanonicalPath,
                ReadCatalogueCause(issue.Code),
                null,
                null,
                null,
                null,
                issue.AttemptedCanonicalPath,
                null,
                null,
                candidates);
        }
    }

    private static IReadOnlyList<FindSourceIdentity> ReadCollisionCandidates(
        SourceCatalogue catalogue,
        SourceCatalogueIssue issue)
        => issue.RelatedPaths
            .Select(catalogue.FindByPath)
            .Where(source => source is not null)
            .Cast<SourceLogicalSource>()
            .Select(CreateIdentity)
            .GroupBy(identity => identity.Path, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(identity => identity.Id, StringComparer.Ordinal)
            .ThenBy(identity => identity.Path, StringComparer.Ordinal)
            .ToArray();

    private static FindFinding? CreateSelectorFinding(ProjectedSelector selector)
    {
        var findingCode = selector.Selector.Resolution switch
        {
            FindSelectorResolution.Invalid
                or FindSelectorResolution.Unknown
                or FindSelectorResolution.Unsupported => FindFindingCode.InvalidSelector,
            FindSelectorResolution.Ambiguous => FindFindingCode.SelectorAmbiguous,
            FindSelectorResolution.Unsafe => FindFindingCode.SelectorUnsafe,
            FindSelectorResolution.Resolved => (FindFindingCode?)null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(selector),
                selector.Selector.Resolution,
                "The Find selector resolution is not defined."),
        };
        if (findingCode is null)
        {
            return null;
        }

        return new FindFinding(
            findingCode.Value,
            FindDefinitions.ReadFindingStatus(findingCode.Value),
            selector.Selector.Value,
            selector.Cause ?? ReadSelectorCause(selector.Selector.Resolution),
            selector.Role,
            selector.RoleOccurrence,
            null,
            null,
            selector.CanonicalPath,
            null,
            null,
            selector.Selector.Candidates);
    }

    private static string ReadSelectorCause(FindSelectorResolution resolution)
        => resolution switch
        {
            FindSelectorResolution.Invalid => "The source selector is not a valid shared source reference.",
            FindSelectorResolution.Unknown => "The source selector does not identify a known source.",
            FindSelectorResolution.Unsupported => "The source selector does not identify an admitted Find source.",
            FindSelectorResolution.Ambiguous => "The source selector identifies more than one source.",
            FindSelectorResolution.Unsafe => "The source selector cannot establish a safe physical boundary.",
            FindSelectorResolution.Resolved => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution,
                "A resolved selector does not have an unresolved cause."),
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The Find selector resolution is not defined."),
        };

    private static FindFindingCode? ReadFindingCode(SourceCatalogueIssueCode code)
        => code switch
        {
            SourceCatalogueIssueCode.RootMissing
                or SourceCatalogueIssueCode.RootUnavailable => FindFindingCode.WorkspaceUnavailable,
            SourceCatalogueIssueCode.RootUnsafe => FindFindingCode.WorkspaceUnsafe,
            SourceCatalogueIssueCode.DirectoryUnavailable
                or SourceCatalogueIssueCode.CandidateUnavailable => FindFindingCode.InspectionUnavailable,
            SourceCatalogueIssueCode.CandidateUnsafe => FindFindingCode.CandidateUnsafe,
            SourceCatalogueIssueCode.IdentityUnavailable
                or SourceCatalogueIssueCode.OrphanOverwrite => FindFindingCode.LayerUnresolved,
            SourceCatalogueIssueCode.IdentityCollision => FindFindingCode.IdentityCollision,
            SourceCatalogueIssueCode.PhysicalAlias => null,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The source catalogue issue code is not defined."),
        };

    private static string ReadCatalogueCause(SourceCatalogueIssueCode code)
        => code switch
        {
            SourceCatalogueIssueCode.RootMissing => "The .agents source root is missing.",
            SourceCatalogueIssueCode.RootUnsafe => "The .agents source root is outside the safe workspace boundary.",
            SourceCatalogueIssueCode.RootUnavailable => "The .agents source root is unavailable.",
            SourceCatalogueIssueCode.DirectoryUnavailable => "A source directory could not be inspected.",
            SourceCatalogueIssueCode.CandidateUnsafe => "A source candidate is outside the safe workspace boundary.",
            SourceCatalogueIssueCode.CandidateUnavailable => "A source candidate could not be inspected.",
            SourceCatalogueIssueCode.IdentityUnavailable => "A recognized source candidate has no usable logical identity.",
            SourceCatalogueIssueCode.IdentityCollision => "Several logical sources share one automatic source ID.",
            SourceCatalogueIssueCode.PhysicalAlias => "Distinct source paths share one physical identity.",
            SourceCatalogueIssueCode.OrphanOverwrite => "An overwrite companion has no adjacent base source.",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The source catalogue issue code is not defined."),
        };

    private static FindSourceIdentity CreateIdentity(SourceLogicalSource source)
        => new(source.Identity.AutomaticId, source.Identity.CanonicalBasePath);

    private static FindSourceKind ReadSourceKind(SourceDocumentForm form)
        => form switch
        {
            SourceDocumentForm.Loader => FindSourceKind.Loader,
            SourceDocumentForm.Skill => FindSourceKind.Skill,
            _ when SourceFormClassifier.IsEntrypoint(form) => FindSourceKind.Entrypoint,
            _ => FindSourceKind.Ordinary,
        };

    private static IReadOnlyList<FindFinding> OrderFindings(IEnumerable<FindFinding> findings)
        => findings
            .OrderBy(finding => ReadFindingOrder(finding.Code))
            .ThenBy(finding => finding.SelectorRole == FindSelectorRole.Include ? 0 : 1)
            .ThenBy(finding => finding.SelectorOccurrence ?? int.MaxValue)
            .ThenBy(finding => finding.Source?.Id ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(finding => finding.Source?.Path ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(finding => finding.Path ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(
                finding => string.Join(
                    "\u001f",
                    finding.Candidates.Select(candidate => $"{candidate.Id}\u001e{candidate.Path}")),
                StringComparer.Ordinal)
            .ToArray();

    private static int ReadFindingOrder(FindFindingCode code)
        => code switch
        {
            FindFindingCode.InvalidInput => 0,
            FindFindingCode.InvalidSelector => 1,
            FindFindingCode.WorkspaceUnavailable => 2,
            FindFindingCode.WorkspaceUnsafe => 3,
            FindFindingCode.SelectorAmbiguous => 4,
            FindFindingCode.SelectorUnsafe => 5,
            FindFindingCode.IdentityCollision => 6,
            FindFindingCode.CandidateUnsafe => 7,
            FindFindingCode.LayerUnresolved => 8,
            FindFindingCode.InspectionUnavailable => 9,
            FindFindingCode.InvalidEncoding => 10,
            FindFindingCode.FrontmatterUnavailable => 11,
            FindFindingCode.SectionAmbiguous => 12,
            FindFindingCode.ProjectionMissing => 13,
            FindFindingCode.ProjectionUnavailable => 14,
            FindFindingCode.OperationFailed => 15,
            FindFindingCode.Interrupted => 16,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Find finding code is not defined."),
        };

    private sealed record ProjectedSelector(
        FindSelectorRole Role,
        int RoleOccurrence,
        FindSelector Selector,
        string? CanonicalPath,
        string? Cause);
}
