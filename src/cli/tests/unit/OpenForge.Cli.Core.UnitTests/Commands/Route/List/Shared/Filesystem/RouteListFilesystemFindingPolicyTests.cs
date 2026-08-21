using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Filesystem;

public sealed class RouteListFilesystemFindingPolicyTests
{
    public static TheoryData<object, object, object> PhysicalCases => new()
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

    public static TheoryData<object, object, object> DirectoryCases => new()
    {
        { DirectoryEnumerationState.Missing, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { DirectoryEnumerationState.AccessDenied, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { DirectoryEnumerationState.InputOutputFailure, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { DirectoryEnumerationState.Cancelled, RouteListFindingCode.Interrupted, CliSemanticStatus.Interrupted },
    };

    public static TheoryData<object, object, object> FilesystemEntryCases => new()
    {
        { RouteListFilesystemEntryState.Missing, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { RouteListFilesystemEntryState.Inaccessible, RouteListFindingCode.PhysicalBoundary, CliSemanticStatus.Blocked },
        { RouteListFilesystemEntryState.Unsupported, RouteListFindingCode.PhysicalBoundary, CliSemanticStatus.Blocked },
        { RouteListFilesystemEntryState.InputOutputFailure, RouteListFindingCode.PhysicalBoundary, CliSemanticStatus.Blocked },
    };

    public static TheoryData<object, object, object> FileCases => new()
    {
        { FileReadState.Missing, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { FileReadState.InvalidEncoding, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { FileReadState.AccessDenied, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { FileReadState.InputOutputFailure, RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete },
        { FileReadState.Cancelled, RouteListFindingCode.Interrupted, CliSemanticStatus.Interrupted },
    };

    [Theory(DisplayName = "Route-list finding policy maps every non-contained physical state"),
        MemberData(nameof(PhysicalCases))]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void PhysicalStatesHaveFiniteMappings(
        object stateValue,
        object expectedCodeValue,
        object expectedStatusValue)
    {
        var state = Assert.IsType<PhysicalPathState>(stateValue);
        var expectedCode = Assert.IsType<RouteListFindingCode>(expectedCodeValue);
        var expectedStatus = Assert.IsType<CliSemanticStatus>(expectedStatusValue);
        var resolution = PhysicalResolution(state);
        var finding = RouteListFilesystemFindingPolicy.FromPhysical(".agents/subject", resolution);

        Assert.NotNull(finding);
        Assert.Equal(expectedCode, finding.Code);
        Assert.Equal(expectedStatus, finding.Status);
        Assert.Equal(".agents/subject", finding.CanonicalLogicalSubject);
    }

    [Theory(DisplayName = "Route-list finding policy maps every incomplete or cancelled directory state"),
        MemberData(nameof(DirectoryCases))]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DirectoryStatesHaveFiniteMappings(
        object stateValue,
        object expectedCodeValue,
        object expectedStatusValue)
    {
        var state = Assert.IsType<DirectoryEnumerationState>(stateValue);
        var expectedCode = Assert.IsType<RouteListFindingCode>(expectedCodeValue);
        var expectedStatus = Assert.IsType<CliSemanticStatus>(expectedStatusValue);
        var result = DirectoryResult(state);
        var finding = RouteListFilesystemFindingPolicy.FromDirectory(result);

        Assert.NotNull(finding);
        Assert.Equal(expectedCode, finding.Code);
        Assert.Equal(expectedStatus, finding.Status);
    }

    [Theory(DisplayName = "Route-list finding policy maps every unavailable filesystem entry state"),
        MemberData(nameof(FilesystemEntryCases))]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void FilesystemEntryStatesHaveFiniteMappings(
        object stateValue,
        object expectedCodeValue,
        object expectedStatusValue)
    {
        var state = Assert.IsType<RouteListFilesystemEntryState>(stateValue);
        var expectedCode = Assert.IsType<RouteListFindingCode>(expectedCodeValue);
        var expectedStatus = Assert.IsType<CliSemanticStatus>(expectedStatusValue);
        var result = FilesystemEntry(state);
        var finding = RouteListFilesystemFindingPolicy.FromEntry(result);

        Assert.NotNull(finding);
        Assert.Equal(expectedCode, finding.Code);
        Assert.Equal(expectedStatus, finding.Status);
    }

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

    [Fact(DisplayName = "Route-list finding policy emits no finding for complete filesystem facts")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CompleteFactsHaveNoFinding()
    {
        var physical = PhysicalPathResolution.Contained("logical", Physical("complete"));
        var directory = new RouteListDirectoryEnumeration(
            DirectoryEnumerationState.Complete,
            ".agents",
            [],
            null);
        var file = FileReadResult<string>.Complete(".agents/file.md", "body");

        Assert.Null(RouteListFilesystemFindingPolicy.FromPhysical(".agents", physical));
        Assert.Null(RouteListFilesystemFindingPolicy.FromDirectory(directory));
        Assert.Null(RouteListFilesystemFindingPolicy.FromEntry(new RouteListFilesystemEntry(
            RouteListFilesystemEntryState.File,
            ".agents/file.md")));
        Assert.Null(RouteListFilesystemFindingPolicy.FromEntry(new RouteListFilesystemEntry(
            RouteListFilesystemEntryState.Directory,
            ".agents/directory")));
        Assert.Null(RouteListFilesystemFindingPolicy.FromFile(file));
    }

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
                (RouteListFindingCode.MetadataMissing, CliSemanticStatus.Incomplete),
                (RouteListFindingCode.MetadataMalformed, CliSemanticStatus.Incomplete),
                (RouteListFindingCode.AuthoredForm, CliSemanticStatus.Attention),
                (RouteListFindingCode.AuthoredForm, CliSemanticStatus.Attention),
                (RouteListFindingCode.IdentityCollision, CliSemanticStatus.Attention),
                (RouteListFindingCode.Interrupted, CliSemanticStatus.Interrupted),
            ],
            findings.Select(finding => (finding.Code, finding.Status)));
    }

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

    private static PhysicalPathResolution PhysicalResolution(PhysicalPathState state)
    {
        return state switch
        {
            PhysicalPathState.Missing => PhysicalPathResolution.Classified(state, "logical"),
            PhysicalPathState.Dangling
                or PhysicalPathState.External
                or PhysicalPathState.Cycle => PhysicalPathResolution.Classified(state, "logical", Physical("target")),
            PhysicalPathState.Inaccessible => PhysicalPathResolution.Failed(
                state,
                "logical",
                new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Filesystem access was denied.")),
            PhysicalPathState.Invalid => PhysicalPathResolution.Failed(
                state,
                "logical",
                new FilesystemFailure(FilesystemFailureKind.InvalidPath, "The path is invalid.")),
            PhysicalPathState.Unsupported => PhysicalPathResolution.Failed(
                state,
                "logical",
                new FilesystemFailure(FilesystemFailureKind.Unsupported, "The operation is unsupported.")),
            PhysicalPathState.InputOutputFailure => PhysicalPathResolution.Failed(
                state,
                "logical",
                new FilesystemFailure(FilesystemFailureKind.InputOutput, "The filesystem operation failed.")),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The test state is not a non-contained state."),
        };
    }

    private static RouteListDirectoryEnumeration DirectoryResult(DirectoryEnumerationState state)
    {
        var failure = state switch
        {
            DirectoryEnumerationState.AccessDenied => new FilesystemFailure(
                FilesystemFailureKind.AccessDenied,
                "Filesystem access was denied."),
            DirectoryEnumerationState.InputOutputFailure => new FilesystemFailure(
                FilesystemFailureKind.InputOutput,
                "The filesystem operation failed."),
            _ => null,
        };
        return new RouteListDirectoryEnumeration(state, ".agents", null, failure);
    }

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

    private static RouteListFilesystemEntry FilesystemEntry(RouteListFilesystemEntryState state)
    {
        var failure = state switch
        {
            RouteListFilesystemEntryState.Inaccessible => new FilesystemFailure(
                FilesystemFailureKind.AccessDenied,
                "Filesystem access was denied."),
            RouteListFilesystemEntryState.Unsupported => new FilesystemFailure(
                FilesystemFailureKind.Unsupported,
                "The operation is unsupported."),
            RouteListFilesystemEntryState.InputOutputFailure => new FilesystemFailure(
                FilesystemFailureKind.InputOutput,
                "The filesystem operation failed."),
            _ => null,
        };
        return new RouteListFilesystemEntry(state, ".agents/entry", failure);
    }

    private static string Physical(string name)
    {
        return Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-route-list-policy", name));
    }
}
