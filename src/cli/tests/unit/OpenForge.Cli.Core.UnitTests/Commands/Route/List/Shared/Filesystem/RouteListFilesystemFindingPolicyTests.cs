using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Filesystem;

public sealed class RouteListFilesystemFindingPolicyTests
{
    public static TheoryData<object, object, object> CandidatePhysicalCases => new()
    {
        { PhysicalPathState.Missing, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { PhysicalPathState.Dangling, RouteListFindingCode.PhysicalBoundary, CliSemanticStatus.Blocked },
        { PhysicalPathState.Inaccessible, RouteListFindingCode.PhysicalBoundary, CliSemanticStatus.Blocked },
        { PhysicalPathState.External, RouteListFindingCode.PhysicalBoundary, CliSemanticStatus.Blocked },
        { PhysicalPathState.Cycle, RouteListFindingCode.PhysicalBoundary, CliSemanticStatus.Blocked },
        { PhysicalPathState.Invalid, RouteListFindingCode.PhysicalBoundary, CliSemanticStatus.Blocked },
        { PhysicalPathState.Unsupported, RouteListFindingCode.PhysicalBoundary, CliSemanticStatus.Blocked },
        { PhysicalPathState.InputOutputFailure, RouteListFindingCode.PhysicalBoundary, CliSemanticStatus.Blocked },
    };

    public static TheoryData<object, object, object> FileCases => new()
    {
        { FileReadState.Missing, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { FileReadState.InvalidEncoding, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { FileReadState.AccessDenied, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { FileReadState.InputOutputFailure, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { FileReadState.Cancelled, RouteListFindingCode.Interrupted, CliSemanticStatus.Interrupted },
    };

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route-list catalogue finding policy maps every non-contained candidate state"), MemberData(nameof(CandidatePhysicalCases))]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CatalogueCandidatesHaveFinitePhysicalMappings(
        object stateValue,
        object expectedCodeValue,
        object expectedStatusValue)
    {
        var state = Assert.IsType<PhysicalPathState>(stateValue);
        var expectedCode = Assert.IsType<RouteListFindingCode>(expectedCodeValue);
        var expectedStatus = Assert.IsType<CliSemanticStatus>(expectedStatusValue);
        var candidate = new SourceCandidate(
            canonicalPath: ".agents/subject.md",
            form: SourceDocumentForm.Markdown,
            automaticId: "subject",
            physicalState: state,
            physicalPath: null,
            physicalParentPath: Physical("parent"));
        var issue = new SourceCatalogueIssue(
            code: SourceCatalogueIssueCode.CandidateUnsafe,
            attemptedCanonicalPath: ".agents/subject.md",
            relatedPaths: [],
            scopePhysicalPath: candidate.PhysicalParentPath,
            failure: PhysicalFailure(state));
        var finding = RouteListFilesystemFindingPolicy.FromCatalogueIssue(issue, candidate);

        Assert.NotNull(finding);
        Assert.Equal(expectedCode, finding.Code);
        Assert.Equal(expectedStatus, finding.Status);
        Assert.Equal(".agents/subject.md", finding.CanonicalLogicalSubject);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route-list finding policy maps every strict file-read outcome"),
        MemberData(nameof(FileCases))]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void FileStatesHaveFiniteMappings(
        object stateValue,
        object expectedCodeValue,
        object expectedStatusValue)
    {
        var state = Assert.IsType<FileReadState>(stateValue);
        var expectedCode = Assert.IsType<RouteListFindingCode>(expectedCodeValue);
        var expectedStatus = Assert.IsType<CliSemanticStatus>(expectedStatusValue);
        var result = FileResult(state);
        var finding = RouteListFilesystemFindingPolicy.FromFile(result);

        Assert.NotNull(finding);
        Assert.Equal(expectedCode, finding.Code);
        Assert.Equal(expectedStatus, finding.Status);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list finding policy emits no finding for a complete file read")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CompleteFileFactsHaveNoFinding()
    {
        var file = FileReadResult<string>.Complete(".agents/file.md", "body");

        Assert.Null(RouteListFilesystemFindingPolicy.FromFile(file));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list finding policy keeps metadata and authored-form severities exact")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CommandLocalFindingsUseExactCodesAndStatuses()
    {
        var findings = new[]
        {
            RouteListFilesystemFindingPolicy.MetadataMissing(".agents/missing.md"),
            RouteListFilesystemFindingPolicy.MetadataMalformed(".agents/malformed.md"),
            RouteListFilesystemFindingPolicy.CompatibilityEntrypoint(".agents/root/index.md"),
            RouteListFilesystemFindingPolicy.OrphanOverwrite(".agents/orphan.overwrite.md"),
            RouteListFilesystemFindingPolicy.IdentityCollision(".agents/alias.md"),
            RouteListFilesystemFindingPolicy.Interrupted(".agents/next"),
        };

        Assert.Equal(
            [
                (RouteListFindingCode.MetadataMissing, CliSemanticStatus.Attention),
                (RouteListFindingCode.MetadataMalformed, CliSemanticStatus.Attention),
                (RouteListFindingCode.AuthoredForm, CliSemanticStatus.Attention),
                (RouteListFindingCode.AuthoredForm, CliSemanticStatus.Attention),
                (RouteListFindingCode.IdentityCollision, CliSemanticStatus.Attention),
                (RouteListFindingCode.Interrupted, CliSemanticStatus.Interrupted),
            ],
            findings.Select(finding => (finding.Code, finding.Status)));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list finding policy bounds direct causes without exception framing")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DirectCauseIsBoundedAndRemovesExceptionType()
    {
        var directCause = $"DecoderFallbackException (0x80070000): {new string('x', 400)}";
        var read = FileReadResult<string>.Failed(
            FileReadState.InvalidEncoding,
            ".agents/invalid.md",
            new FilesystemFailure(FilesystemFailureKind.InvalidEncoding, directCause));

        var finding = RouteListFilesystemFindingPolicy.FromFile(read);

        Assert.NotNull(finding);
        Assert.DoesNotContain("Exception", finding.Cause, StringComparison.Ordinal);
        Assert.True(finding.Cause.Length <= 256);
        Assert.DoesNotContain('\n', finding.Cause);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list finding policy rejects Foundation syntax outcomes owned by metadata parsing")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void FoundationInvalidSyntaxDoesNotCollapseIntoReadUnavailable()
    {
        var read = FileReadResult<string>.Failed(
            FileReadState.InvalidSyntax,
            ".agents/invalid.md",
            new FilesystemFailure(FilesystemFailureKind.InvalidSyntax, "Invalid syntax."));

        Assert.Throws<ArgumentOutOfRangeException>(() => RouteListFilesystemFindingPolicy.FromFile(read));
    }

    private static FilesystemFailure? PhysicalFailure(PhysicalPathState state)
        => state switch
        {
            PhysicalPathState.Missing or PhysicalPathState.Dangling
                or PhysicalPathState.External or PhysicalPathState.Cycle => null,
            PhysicalPathState.Inaccessible =>
                new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Filesystem access was denied."),
            PhysicalPathState.Invalid =>
                new FilesystemFailure(FilesystemFailureKind.InvalidPath, "The path is invalid."),
            PhysicalPathState.Unsupported =>
                new FilesystemFailure(FilesystemFailureKind.Unsupported, "The operation is unsupported."),
            PhysicalPathState.InputOutputFailure =>
                new FilesystemFailure(FilesystemFailureKind.InputOutput, "The filesystem operation failed."),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The test state is not a non-contained state."),
        };

    private static FileReadResult<string> FileResult(FileReadState state)
    {
        return state switch
        {
            FileReadState.Missing => FileReadResult<string>.Missing(".agents/file.md"),
            FileReadState.InvalidEncoding => FileReadResult<string>.Failed(
                state,
                ".agents/file.md",
                new FilesystemFailure(FilesystemFailureKind.InvalidEncoding, "The file is not valid UTF-8.")),
            FileReadState.AccessDenied => FileReadResult<string>.Failed(
                state,
                ".agents/file.md",
                new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Filesystem access was denied.")),
            FileReadState.InputOutputFailure => FileReadResult<string>.Failed(
                state,
                ".agents/file.md",
                new FilesystemFailure(FilesystemFailureKind.InputOutput, "The filesystem operation failed.")),
            FileReadState.Cancelled => FileReadResult<string>.Cancelled(".agents/file.md"),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The test state is not a mapped state."),
        };
    }

    private static string Physical(string name)
    {
        return Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-route-list-policy", name));
    }
}
