using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Selection;

internal sealed class FindUniverseResolver
{
    private readonly FindPhysicalPathResolver _physicalPathResolver;

    internal FindUniverseResolver(FindPhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal FindUniverseResolution Resolve(FindUniverseInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var request = input.Request;
        var catalogue = input.SourceContext.Catalogue;
        var include = ResolveSelectors(
            request.UniverseFilter.Include,
            FindSelectorRole.Include,
            request.Workspace,
            catalogue);
        var exclude = ResolveSelectors(
            request.UniverseFilter.Exclude,
            FindSelectorRole.Exclude,
            request.Workspace,
            catalogue);
        var allSelectors = include.Concat(exclude).ToArray();
        var selectorFindings = allSelectors
            .Select((resolution, index) => CreateSelectorFinding(resolution, index, include.Count))
            .Where(finding => finding is not null)
            .Cast<FindFinding>()
            .ToArray();

        var hasUnresolvedSelector = allSelectors.Any(
            resolution => resolution.Selector.Resolution != FindSelectorResolution.Resolved);
        SourceCatalogueSelection selection;
        int? candidateCount;
        if (hasUnresolvedSelector)
        {
            selection = SelectNothing(catalogue);
            candidateCount = null;
        }
        else
        {
            selection = SelectEffectiveSources(input.SourceContext, include, exclude);
            candidateCount = ReadCandidateCount(catalogue, selection);
        }

        var findings = selectorFindings
            .Concat(ProjectCatalogueFindings(catalogue, selection))
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
            include.Count == 0 && exclude.Count == 0
                ? FindUniverseMode.Default
                : FindUniverseMode.Filtered,
            include.Select(resolution => resolution.Selector),
            exclude.Select(resolution => resolution.Selector),
            candidateCount,
            null,
            null);

        return new FindUniverseResolution(
            universe,
            selection,
            OrderFindings(findings));
    }

    private IReadOnlyList<SelectorResolution> ResolveSelectors(
        IReadOnlyList<string> values,
        FindSelectorRole role,
        CliWorkspace workspace,
        SourceCatalogue catalogue)
    {
        var resolutions = new List<SelectorResolution>(values.Count);
        for (var index = 0; index < values.Count; index++)
        {
            resolutions.Add(ResolveSelector(values[index], role, workspace, catalogue));
        }

        return resolutions;
    }

    private SelectorResolution ResolveSelector(
        string value,
        FindSelectorRole role,
        CliWorkspace workspace,
        SourceCatalogue catalogue)
    {
        var parsed = SourceReferenceParser.Parse(value);
        if (parsed.State == SourceReferenceParseState.Invalid)
        {
            return Unresolved(
                value,
                role,
                parsed.Kind,
                FindSelectorResolution.Invalid,
                parsed.Cause,
                ReadCanonicalPath(parsed.AttemptedPath));
        }

        return parsed.Kind switch
        {
            SourceReferenceKind.SourceId => ResolveId(
                value,
                role,
                parsed,
                catalogue),
            SourceReferenceKind.SourcePath => ResolvePath(
                value,
                role,
                parsed,
                workspace,
                catalogue),
            _ => throw new ArgumentOutOfRangeException(nameof(parsed), parsed.Kind, "The source-reference kind is not defined."),
        };
    }

    private static SelectorResolution ResolveId(
        string value,
        FindSelectorRole role,
        SourceReferenceParseResult parsed,
        SourceCatalogue catalogue)
    {
        var id = parsed.AttemptedId
            ?? throw new InvalidOperationException("A valid source ID reference requires an attempted ID.");
        var sources = catalogue.FindAllById(id);
        if (sources.Count == 1)
        {
            return Resolved(value, role, parsed.Kind, sources[0], catalogue);
        }

        if (sources.Count > 1)
        {
            var candidates = sources
                .Select(CreateIdentity)
                .OrderBy(identity => identity.Id, StringComparer.Ordinal)
                .ThenBy(identity => identity.Path, StringComparer.Ordinal)
                .ToArray();
            return new SelectorResolution(
                value,
                role,
                new FindSelector(
                    value,
                    parsed.Kind,
                    FindSelectorResolution.Ambiguous,
                    null,
                    null,
                    null,
                    candidates),
                null,
                [],
                null,
                null,
                "The source ID resolves to more than one logical source.");
        }

        var candidatesById = catalogue.FindAllCandidatesById(id);
        if (candidatesById.Any(candidate => IsUnsafe(candidate.PhysicalState)))
        {
            return Unresolved(
                value,
                role,
                parsed.Kind,
                FindSelectorResolution.Unsafe,
                "The source ID has a candidate outside an established safe physical boundary.",
                null);
        }

        return Unresolved(
            value,
            role,
            parsed.Kind,
            FindSelectorResolution.Unknown,
            "The source ID does not identify a retained logical source.",
            null);
    }

    private SelectorResolution ResolvePath(
        string value,
        FindSelectorRole role,
        SourceReferenceParseResult parsed,
        CliWorkspace workspace,
        SourceCatalogue catalogue)
    {
        var path = parsed.AttemptedPath
            ?? throw new InvalidOperationException("A valid source path reference requires an attempted path.");
        var source = catalogue.FindByPath(path);
        if (source is not null)
        {
            return Resolved(value, role, parsed.Kind, source, catalogue);
        }

        var candidate = catalogue.FindCandidateByPath(path);
        if (candidate is not null)
        {
            return Unresolved(
                value,
                role,
                parsed.Kind,
                ReadSelectorResolution(candidate.PhysicalState),
                ReadCandidateCause(candidate.PhysicalState),
                path);
        }

        var physical = _physicalPathResolver(workspace, path);
        return Unresolved(
            value,
            role,
            parsed.Kind,
            ReadSelectorResolution(physical.State),
            ReadPhysicalCause(physical.State),
            path);
    }

    private static SelectorResolution Resolved(
        string value,
        FindSelectorRole role,
        SourceReferenceKind form,
        SourceLogicalSource source,
        SourceCatalogue catalogue)
    {
        var identity = CreateIdentity(source);
        var sourceKind = ReadSourceKind(source.Base.Form);
        var expansion = sourceKind == FindSourceKind.Ordinary
            ? FindSelectorExpansion.Source
            : FindSelectorExpansion.Folder;
        SourceCatalogueSelectionScope? scope = null;
        IReadOnlyList<SourceLogicalSource> expandedSources;
        if (expansion == FindSelectorExpansion.Folder)
        {
            scope = CreateScope(source);
            expandedSources = catalogue.Sources
                .Where(candidate => PhysicalContainment.Contains(
                    scope.PhysicalDirectoryPath,
                    candidate.Base.PhysicalPath))
                .ToArray();
        }
        else
        {
            expandedSources = [source];
        }

        return new SelectorResolution(
            value,
            role,
            new FindSelector(
                value,
                form,
                FindSelectorResolution.Resolved,
                identity,
                sourceKind,
                expansion,
                []),
            source,
            expandedSources,
            scope,
            ReadCanonicalPath(value),
            null);
    }

    private static SelectorResolution Unresolved(
        string value,
        FindSelectorRole role,
        SourceReferenceKind form,
        FindSelectorResolution resolution,
        string? cause,
        string? canonicalPath)
    {
        return new SelectorResolution(
            value,
            role,
            new FindSelector(
                value,
                form,
                resolution,
                null,
                null,
                null,
                []),
            null,
            [],
            null,
            canonicalPath,
            cause ?? ReadSelectorCause(resolution));
    }

    private static SourceCatalogueSelection SelectEffectiveSources(
        FindSourceReadContext sourceContext,
        IReadOnlyList<SelectorResolution> include,
        IReadOnlyList<SelectorResolution> exclude)
    {
        var catalogue = sourceContext.Catalogue;
        if (include.Count == 0 && exclude.Count == 0)
        {
            return catalogue.SelectAll();
        }

        var excludedPaths = exclude
            .SelectMany(resolution => resolution.ExpandedSources)
            .Select(source => source.Identity.CanonicalBasePath)
            .ToHashSet(StringComparer.Ordinal);
        var selectedSources = (include.Count == 0
                ? catalogue.Sources
                : include.SelectMany(resolution => resolution.ExpandedSources))
            .Where(source => !excludedPaths.Contains(source.Identity.CanonicalBasePath))
            .GroupBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();

        var includedScopes = include.Count == 0
            ? ReadDefaultScope(sourceContext.DefaultSelectionScope)
            : ReadScopes(include);
        var excludedScopes = ReadScopes(exclude);
        return catalogue.Select(new SourceCatalogueSelectionRequest(
            selectedSources,
            includedScopes,
            excludedScopes));
    }

    private static IReadOnlyList<SourceCatalogueSelectionScope> ReadDefaultScope(
        SourceCatalogueSelectionScope? scope)
        => scope is null ? [] : [scope];

    private static IReadOnlyList<SourceCatalogueSelectionScope> ReadScopes(
        IEnumerable<SelectorResolution> resolutions)
    {
        return resolutions
            .Select(resolution => resolution.Scope)
            .Where(scope => scope is not null)
            .Cast<SourceCatalogueSelectionScope>()
            .GroupBy(scope => scope.CanonicalDirectoryPath, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(scope => scope.CanonicalDirectoryPath, StringComparer.Ordinal)
            .ToArray();
    }

    private static SourceCatalogueSelection SelectNothing(SourceCatalogue catalogue)
        => catalogue.Select(new SourceCatalogueSelectionRequest([], [], []));

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
    {
        return issue.RelatedPaths
            .Select(catalogue.FindByPath)
            .Where(source => source is not null)
            .Cast<SourceLogicalSource>()
            .Select(CreateIdentity)
            .GroupBy(identity => identity.Path, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(identity => identity.Id, StringComparer.Ordinal)
            .ThenBy(identity => identity.Path, StringComparer.Ordinal)
            .ToArray();
    }

    private static FindFinding? CreateSelectorFinding(
        SelectorResolution resolution,
        int index,
        int includeCount)
    {
        var findingCode = resolution.Selector.Resolution switch
        {
            FindSelectorResolution.Invalid
                or FindSelectorResolution.Unknown
                or FindSelectorResolution.Unsupported => FindFindingCode.InvalidSelector,
            FindSelectorResolution.Ambiguous => FindFindingCode.SelectorAmbiguous,
            FindSelectorResolution.Unsafe => FindFindingCode.SelectorUnsafe,
            FindSelectorResolution.Resolved => (FindFindingCode?)null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.Selector.Resolution,
                "The Find selector resolution is not defined."),
        };
        if (findingCode is null)
        {
            return null;
        }

        var isInclude = index < includeCount;
        var occurrence = isInclude ? index + 1 : index - includeCount + 1;
        return new FindFinding(
            findingCode.Value,
            FindDefinitions.ReadFindingStatus(findingCode.Value),
            resolution.Value,
            resolution.Cause ?? ReadSelectorCause(resolution.Selector.Resolution),
            isInclude ? FindSelectorRole.Include : FindSelectorRole.Exclude,
            occurrence,
            null,
            null,
            resolution.CanonicalPath,
            null,
            null,
            resolution.Selector.Candidates);
    }

    private static FindSelectorResolution ReadSelectorResolution(PhysicalPathState state)
        => state switch
        {
            PhysicalPathState.Contained => FindSelectorResolution.Unsupported,
            PhysicalPathState.Missing => FindSelectorResolution.Unknown,
            _ => FindSelectorResolution.Unsafe,
        };

    private static bool IsUnsafe(PhysicalPathState state)
        => state is not (PhysicalPathState.Contained or PhysicalPathState.Missing);

    private static string ReadCandidateCause(PhysicalPathState state)
        => state switch
        {
            PhysicalPathState.Contained => "The exact path is not an admitted logical Find source.",
            PhysicalPathState.Missing => "The exact source path is no longer present.",
            _ => "The exact source path is outside an established safe physical boundary.",
        };

    private static string ReadPhysicalCause(PhysicalPathState state)
        => state switch
        {
            PhysicalPathState.Contained => "The exact path is not an admitted logical Find source.",
            PhysicalPathState.Missing => "The exact source path does not exist.",
            _ => "The exact source path is outside an established safe physical boundary.",
        };

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

    private static string? ReadCanonicalPath(string? path)
        => path is not null && SourceLogicalPath.IsCanonicalRoot(path) ? path : null;

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

    private static SourceCatalogueSelectionScope CreateScope(SourceLogicalSource source)
    {
        var physicalDirectory = Path.GetDirectoryName(source.Base.PhysicalPath)
            ?? throw new InvalidOperationException("A source layer must have a physical parent directory.");
        return new SourceCatalogueSelectionScope(
            SourceLogicalPath.ReadParent(source.Base.CanonicalPath),
            physicalDirectory);
    }

    private static IReadOnlyList<FindFinding> OrderFindings(IEnumerable<FindFinding> findings)
    {
        return findings
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
    }

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

    private sealed record SelectorResolution(
        string Value,
        FindSelectorRole Role,
        FindSelector Selector,
        SourceLogicalSource? Source,
        IReadOnlyList<SourceLogicalSource> ExpandedSources,
        SourceCatalogueSelectionScope? Scope,
        string? CanonicalPath,
        string? Cause);
}
