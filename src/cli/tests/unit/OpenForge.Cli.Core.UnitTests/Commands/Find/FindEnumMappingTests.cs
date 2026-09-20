using OpenForge.Cli.Core.Commands.Find.Models.Projection;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Commands.Find.Shared.Projection;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find;

public sealed class FindEnumMappingTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Find file read mapping is exhaustive and names null"), Trait("Feature", "find-enum-mapping"), Trait("Evidence", "Unit")]
    public void FileReadMappingIsExhaustiveAndNamesNull()
    {
        var expected = new (FileReadState? State, FindFindingCode? Code)[]
        {
            (FileReadState.Complete, null),
            (FileReadState.Missing, FindFindingCode.InspectionUnavailable),
            (FileReadState.InvalidEncoding, FindFindingCode.InvalidEncoding),
            (FileReadState.InvalidSyntax, FindFindingCode.InspectionUnavailable),
            (FileReadState.AccessDenied, FindFindingCode.InspectionUnavailable),
            (FileReadState.InputOutputFailure, FindFindingCode.InspectionUnavailable),
            (FileReadState.Cancelled, FindFindingCode.Interrupted),
            (null, FindFindingCode.InspectionUnavailable),
        };
        foreach (var (state, code) in expected)
        {
            Assert.Equal(code, FindLayerInspector.ReadFileFindingCode(state));
        }

        var undefined = (FileReadState)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => FindLayerInspector.ReadFileFindingCode(undefined));
        Assert.Equal("state", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Find layer finding cause mapping is exhaustive"), Trait("Feature", "find-enum-mapping"), Trait("Evidence", "Unit")]
    public void LayerFindingCauseMappingIsExhaustive()
    {
        const string unavailable = "The source layer could not be read or verified.";
        var expected = new (FindFindingCode Code, string Cause)[]
        {
            (FindFindingCode.InvalidInput, unavailable),
            (FindFindingCode.InvalidSelector, unavailable),
            (FindFindingCode.WorkspaceUnavailable, unavailable),
            (FindFindingCode.WorkspaceUnsafe, unavailable),
            (FindFindingCode.SelectorAmbiguous, unavailable),
            (FindFindingCode.SelectorUnsafe, unavailable),
            (FindFindingCode.IdentityCollision, unavailable),
            (FindFindingCode.CandidateUnsafe, "The source layer left the established workspace boundary."),
            (FindFindingCode.LayerUnresolved, unavailable),
            (FindFindingCode.InspectionUnavailable, unavailable),
            (FindFindingCode.InvalidEncoding, "The source layer is not valid UTF-8."),
            (FindFindingCode.FrontmatterUnavailable, unavailable),
            (FindFindingCode.SectionAmbiguous, unavailable),
            (FindFindingCode.ProjectionMissing, unavailable),
            (FindFindingCode.ProjectionUnavailable, unavailable),
            (FindFindingCode.OperationFailed, unavailable),
            (FindFindingCode.Interrupted, "The source layer read was cancelled."),
        };
        foreach (var (code, cause) in expected)
        {
            Assert.Equal(cause, FindLayerInspector.ReadFindingCause(code));
        }

        var undefined = (FindFindingCode)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => FindLayerInspector.ReadFindingCause(undefined));
        Assert.Equal("code", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Find matching coverage mapping is exhaustive"), Trait("Feature", "find-enum-mapping"), Trait("Evidence", "Unit")]
    public void MatchingCoverageMappingIsExhaustive()
    {
        var expected = new (FindFindingCode Code, FindCoverageState Coverage)[]
        {
            (FindFindingCode.InvalidInput, FindCoverageState.Complete),
            (FindFindingCode.InvalidSelector, FindCoverageState.Complete),
            (FindFindingCode.WorkspaceUnavailable, FindCoverageState.Blocked),
            (FindFindingCode.WorkspaceUnsafe, FindCoverageState.Blocked),
            (FindFindingCode.SelectorAmbiguous, FindCoverageState.Blocked),
            (FindFindingCode.SelectorUnsafe, FindCoverageState.Blocked),
            (FindFindingCode.IdentityCollision, FindCoverageState.Complete),
            (FindFindingCode.CandidateUnsafe, FindCoverageState.Incomplete),
            (FindFindingCode.LayerUnresolved, FindCoverageState.Incomplete),
            (FindFindingCode.InspectionUnavailable, FindCoverageState.Incomplete),
            (FindFindingCode.InvalidEncoding, FindCoverageState.Incomplete),
            (FindFindingCode.FrontmatterUnavailable, FindCoverageState.Incomplete),
            (FindFindingCode.SectionAmbiguous, FindCoverageState.Incomplete),
            (FindFindingCode.ProjectionMissing, FindCoverageState.Complete),
            (FindFindingCode.ProjectionUnavailable, FindCoverageState.Incomplete),
            (FindFindingCode.OperationFailed, FindCoverageState.Failed),
            (FindFindingCode.Interrupted, FindCoverageState.Interrupted),
        };
        foreach (var (code, coverage) in expected)
        {
            Assert.Equal(coverage, FindMatchingFindingPolicy.ReadCoverage(code));
        }

        var undefined = (FindFindingCode)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => FindMatchingFindingPolicy.ReadCoverage(undefined));
        Assert.Equal("code", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Find projection layer rank mapping is exhaustive and names null"), Trait("Feature", "find-enum-mapping"), Trait("Evidence", "Unit")]
    public void ProjectionLayerRankMappingIsExhaustiveAndNamesNull()
    {
        Assert.Equal(0, FindProjectionFindingPolicy.ReadLayerRank(SourceLayerKind.Base));
        Assert.Equal(1, FindProjectionFindingPolicy.ReadLayerRank(SourceLayerKind.Overwrite));
        Assert.Equal(2, FindProjectionFindingPolicy.ReadLayerRank(null));

        var undefined = (SourceLayerKind)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => FindProjectionFindingPolicy.ReadLayerRank(undefined));
        Assert.Equal("layer", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Find route projection mapping is exhaustive and preserves the routed guard"), Trait("Feature", "find-enum-mapping"), Trait("Evidence", "Unit")]
    public void RouteProjectionMappingIsExhaustiveAndPreservesRoutedGuard()
    {
        var routed = Assert.IsType<FindSourceRouteProjection>(
            FindProjectionSourceResolver.ReadRouteProjection(SourceRouteState.Routed, "docs/guide", "docs/guide"));
        Assert.Equal(FindRouteState.Routed, routed.State);
        Assert.Equal("docs/guide", routed.Route);
        Assert.Null(FindProjectionSourceResolver.ReadRouteProjection(
            SourceRouteState.Routed,
            "docs/other",
            "docs/guide"));

        var unrouted = Assert.IsType<FindSourceRouteProjection>(
            FindProjectionSourceResolver.ReadRouteProjection(SourceRouteState.Unrouted, null, "docs/guide"));
        Assert.Equal(FindRouteState.Unrouted, unrouted.State);
        Assert.Null(unrouted.Route);
        Assert.Null(FindProjectionSourceResolver.ReadRouteProjection(
            SourceRouteState.Ambiguous,
            null,
            "docs/guide"));
        Assert.Null(FindProjectionSourceResolver.ReadRouteProjection(
            SourceRouteState.Unavailable,
            null,
            "docs/guide"));

        var undefined = (SourceRouteState)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => FindProjectionSourceResolver.ReadRouteProjection(undefined, null, "docs/guide"));
        Assert.Equal("state", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Find exact-path cause mapping is exhaustive and preserves candidate presence"), Trait("Feature", "find-enum-mapping"), Trait("Evidence", "Unit")]
    public void ExactPathCauseMappingIsExhaustiveAndPreservesCandidatePresence()
    {
        const string original = "original cause";
        var expected = new (SourceReferenceResolutionState State, bool CandidateExists, string? Cause)[]
        {
            (SourceReferenceResolutionState.Resolved, false, original),
            (SourceReferenceResolutionState.Invalid, false, original),
            (SourceReferenceResolutionState.Unknown, true, "The exact source path is no longer present."),
            (SourceReferenceResolutionState.Unknown, false, "The exact source path does not exist."),
            (SourceReferenceResolutionState.Unsupported, false, "The exact path is not an admitted logical Find source."),
            (SourceReferenceResolutionState.Ambiguous, false, original),
            (SourceReferenceResolutionState.Unsafe, false, "The exact source path is outside an established safe physical boundary."),
        };
        foreach (var (state, candidateExists, cause) in expected)
        {
            Assert.Equal(cause, FindUniverseResolver.ReadExactPathCause(state, candidateExists, original));
        }

        Assert.Null(FindUniverseResolver.ReadExactPathCause(
            SourceReferenceResolutionState.Resolved,
            false,
            null));
        var undefined = (SourceReferenceResolutionState)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => FindUniverseResolver.ReadExactPathCause(undefined, false, original));
        Assert.Equal("state", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Find source kind mapping is exhaustive"), Trait("Feature", "find-enum-mapping"), Trait("Evidence", "Unit")]
    public void SourceKindMappingIsExhaustive()
    {
        var expected = new (SourceDocumentForm Form, FindSourceKind Kind)[]
        {
            (SourceDocumentForm.Loader, FindSourceKind.Loader),
            (SourceDocumentForm.CanonicalEntrypoint, FindSourceKind.Entrypoint),
            (SourceDocumentForm.IndexEntrypoint, FindSourceKind.Entrypoint),
            (SourceDocumentForm.UnderscoreIndexEntrypoint, FindSourceKind.Entrypoint),
            (SourceDocumentForm.ReferencesEntrypoint, FindSourceKind.Entrypoint),
            (SourceDocumentForm.UnderscoreReferencesEntrypoint, FindSourceKind.Entrypoint),
            (SourceDocumentForm.Skill, FindSourceKind.Skill),
            (SourceDocumentForm.Markdown, FindSourceKind.Ordinary),
            (SourceDocumentForm.OverwriteCompanion, FindSourceKind.Ordinary),
        };
        foreach (var (form, kind) in expected)
        {
            Assert.Equal(kind, FindUniverseResolver.ReadSourceKind(form));
        }

        var undefined = (SourceDocumentForm)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => FindUniverseResolver.ReadSourceKind(undefined));
        Assert.Equal("form", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Find stage-state agreement mapping is exhaustive"), Trait("Feature", "find-enum-mapping"), Trait("Evidence", "Unit")]
    public void StageStateAgreementMappingIsExhaustive()
    {
        var supported = ReadSupportedStageStates();
        foreach (var status in Enum.GetValues<CliSemanticStatus>())
        {
            foreach (var matching in Enum.GetValues<FindCoverageState>())
            {
                foreach (var projection in Enum.GetValues<FindProjectionCoverageState>())
                {
                    foreach (var contentRequested in new[] { false, true })
                    {
                        Assert.Equal(
                            supported.Contains((status, matching, projection, contentRequested)),
                            FindResult.StageStatesAgree(status, matching, projection, contentRequested));
                    }
                }
            }
        }

        var undefined = (CliSemanticStatus)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => FindResult.StageStatesAgree(
            undefined,
            FindCoverageState.Complete,
            FindProjectionCoverageState.Complete,
            true));
        Assert.Equal("status", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Find allowed finding-status mapping is exhaustive"), Trait("Feature", "find-enum-mapping"), Trait("Evidence", "Unit")]
    public void AllowedFindingStatusMappingIsExhaustive()
    {
        var expected = new Dictionary<CliSemanticStatus, IReadOnlySet<CliSemanticStatus>>
        {
            [CliSemanticStatus.Complete] = new HashSet<CliSemanticStatus>(),
            [CliSemanticStatus.Attention] = new HashSet<CliSemanticStatus> { CliSemanticStatus.Attention },
            [CliSemanticStatus.Incomplete] = new HashSet<CliSemanticStatus> { CliSemanticStatus.Attention, CliSemanticStatus.Incomplete },
            [CliSemanticStatus.Invalid] = new HashSet<CliSemanticStatus> { CliSemanticStatus.Invalid },
            [CliSemanticStatus.Blocked] = new HashSet<CliSemanticStatus> { CliSemanticStatus.Attention, CliSemanticStatus.Incomplete, CliSemanticStatus.Blocked },
            [CliSemanticStatus.Failed] = new HashSet<CliSemanticStatus> { CliSemanticStatus.Attention, CliSemanticStatus.Incomplete, CliSemanticStatus.Failed },
            [CliSemanticStatus.Interrupted] = new HashSet<CliSemanticStatus> { CliSemanticStatus.Attention, CliSemanticStatus.Incomplete, CliSemanticStatus.Interrupted },
        };
        foreach (var resultStatus in Enum.GetValues<CliSemanticStatus>())
        {
            foreach (var findingStatus in Enum.GetValues<CliSemanticStatus>())
            {
                Assert.Equal(
                    expected[resultStatus].Contains(findingStatus),
                    FindResult.IsAllowedFindingStatus(resultStatus, findingStatus));
            }
        }

        var undefined = (CliSemanticStatus)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => FindResult.IsAllowedFindingStatus(undefined, CliSemanticStatus.Complete));
        Assert.Equal("resultStatus", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    private static IReadOnlySet<(CliSemanticStatus Status, FindCoverageState Matching, FindProjectionCoverageState Projection, bool ContentRequested)>
        ReadSupportedStageStates()
    {
        var supported = new HashSet<(CliSemanticStatus, FindCoverageState, FindProjectionCoverageState, bool)>();
        foreach (var status in new[] { CliSemanticStatus.Complete, CliSemanticStatus.Attention })
        {
            foreach (var projection in new[] { FindProjectionCoverageState.Complete, FindProjectionCoverageState.NotRequested })
            {
                supported.Add((status, FindCoverageState.Complete, projection, false));
                supported.Add((status, FindCoverageState.Complete, projection, true));
            }
        }

        foreach (var projection in new[]
                 {
                     FindProjectionCoverageState.Complete,
                     FindProjectionCoverageState.Incomplete,
                     FindProjectionCoverageState.NotRequested,
                 })
        {
            supported.Add((CliSemanticStatus.Incomplete, FindCoverageState.Incomplete, projection, false));
            supported.Add((CliSemanticStatus.Incomplete, FindCoverageState.Incomplete, projection, true));
        }

        supported.Add((CliSemanticStatus.Incomplete, FindCoverageState.Complete, FindProjectionCoverageState.Incomplete, false));
        supported.Add((CliSemanticStatus.Incomplete, FindCoverageState.Complete, FindProjectionCoverageState.Incomplete, true));
        supported.Add((CliSemanticStatus.Invalid, FindCoverageState.NotStarted, FindProjectionCoverageState.NotRequested, false));
        supported.Add((CliSemanticStatus.Invalid, FindCoverageState.NotStarted, FindProjectionCoverageState.NotStarted, true));
        supported.Add((CliSemanticStatus.Blocked, FindCoverageState.Blocked, FindProjectionCoverageState.NotRequested, false));
        supported.Add((CliSemanticStatus.Blocked, FindCoverageState.Blocked, FindProjectionCoverageState.Blocked, true));
        AddTerminalStageStates(supported, CliSemanticStatus.Failed, FindCoverageState.Failed, FindProjectionCoverageState.Failed);
        AddTerminalStageStates(supported, CliSemanticStatus.Interrupted, FindCoverageState.Interrupted, FindProjectionCoverageState.Interrupted);
        return supported;
    }

    private static void AddTerminalStageStates(
        ISet<(CliSemanticStatus, FindCoverageState, FindProjectionCoverageState, bool)> supported,
        CliSemanticStatus status,
        FindCoverageState terminalMatching,
        FindProjectionCoverageState terminalProjection)
    {
        supported.Add((status, terminalMatching, FindProjectionCoverageState.NotRequested, false));
        supported.Add((status, terminalMatching, terminalProjection, true));
        foreach (var matching in new[] { FindCoverageState.Complete, FindCoverageState.Incomplete })
        {
            foreach (var projection in new[]
                     {
                         FindProjectionCoverageState.Complete,
                         FindProjectionCoverageState.Incomplete,
                         terminalProjection,
                     })
            {
                supported.Add((status, matching, projection, false));
                supported.Add((status, matching, projection, true));
            }

            supported.Add((status, matching, FindProjectionCoverageState.NotRequested, false));
        }
    }
}
