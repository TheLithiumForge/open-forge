using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Identity;

public sealed class SourceReferenceResolverPhysicalMappingTests
{
    private const string CanonicalPath = ".agents/subject.md";

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Returned physical states retain exact source-reference resolution and causes"),
        InlineData((int)PhysicalPathState.Contained, (int)SourceReferenceResolutionState.Unsupported, "The exact path is not an admitted logical source."),
        InlineData((int)PhysicalPathState.Missing, (int)SourceReferenceResolutionState.Unknown, "The exact source path does not exist."),
        InlineData((int)PhysicalPathState.Dangling, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.Inaccessible, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.External, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.Cycle, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.Invalid, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.Unsupported, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.InputOutputFailure, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        Trait("Feature", "source-selection"), Trait("Evidence", "Unit")]
    public void ReturnedPhysicalStatesRetainExactCauses(int state, int expectedResolution, string expectedCause)
    {
        var catalogue = new SourceCatalogue(
            workspace: SourceInventoryTestData.Workspace(),
            candidates: [],
            sources: [],
            issues: [],
            isCancelled: false);
        var physicalCalls = 0;
        var resolver = new SourceReferenceResolver((_, path) =>
        {
            physicalCalls++;
            Assert.Equal(CanonicalPath, path);
            return PhysicalResult((PhysicalPathState)state, path);
        });

        var result = resolver.Resolve(CanonicalPath, catalogue);

        Assert.Equal(1, physicalCalls);
        Assert.Equal((SourceReferenceResolutionState)expectedResolution, result.State);
        Assert.Equal(expectedCause, result.Cause);
        Assert.Equal(CanonicalPath, result.CanonicalPath);
        Assert.Null(result.Source);
        Assert.Empty(result.Candidates);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Retained candidate states keep exact causes without invoking physical resolution"),
        InlineData((int)PhysicalPathState.Contained, (int)SourceReferenceResolutionState.Unsupported, "The exact path is not an admitted logical source."),
        InlineData((int)PhysicalPathState.Missing, (int)SourceReferenceResolutionState.Unknown, "The exact source path is no longer present."),
        InlineData((int)PhysicalPathState.Dangling, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.Inaccessible, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.External, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.Cycle, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.Invalid, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.Unsupported, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        InlineData((int)PhysicalPathState.InputOutputFailure, (int)SourceReferenceResolutionState.Unsafe, "The exact source path is outside an established safe physical boundary."),
        Trait("Feature", "source-selection"), Trait("Evidence", "Unit")]
    public void RetainedCandidatesPreserveTheirOwnCauses(int state, int expectedResolution, string expectedCause)
    {
        var physicalState = (PhysicalPathState)state;
        var candidate = SourceInventoryTestData.Candidate(
            canonicalPath: CanonicalPath,
            form: SourceDocumentForm.Markdown,
            automaticId: null,
            state: physicalState,
            physicalPath: physicalState == PhysicalPathState.Contained ? SourceInventoryTestData.Physical(CanonicalPath) : null,
            physicalParentPath: SourceInventoryTestData.Physical(".agents"));
        var catalogue = new SourceCatalogue(
            workspace: SourceInventoryTestData.Workspace(),
            candidates: [candidate],
            sources: [],
            issues: [],
            isCancelled: false);
        Assert.Same(candidate, catalogue.FindCandidateByPath(CanonicalPath));
        Assert.Null(catalogue.FindByPath(CanonicalPath));
        var resolver = new SourceReferenceResolver((_, _) => throw new InvalidOperationException("An admitted candidate must avoid physical resolution."));

        var result = resolver.Resolve(CanonicalPath, catalogue);

        Assert.Equal((SourceReferenceResolutionState)expectedResolution, result.State);
        Assert.Equal(expectedCause, result.Cause);
        Assert.Equal(CanonicalPath, result.CanonicalPath);
        Assert.Null(result.Source);
        Assert.Empty(result.Candidates);
    }

    private static PhysicalPathResolution PhysicalResult(PhysicalPathState state, string logicalPath)
        => state switch
        {
            PhysicalPathState.Contained => PhysicalPathResolution.Contained(
                logicalPath: logicalPath,
                physicalPath: SourceInventoryTestData.Physical(CanonicalPath)),
            PhysicalPathState.Missing => PhysicalPathResolution.Classified(state: state, logicalPath: logicalPath),
            PhysicalPathState.Dangling or PhysicalPathState.External or PhysicalPathState.Cycle => PhysicalPathResolution.Classified(
                state: state,
                logicalPath: logicalPath,
                resolvedPhysicalPath: SourceInventoryTestData.Physical("../physical-target.md")),
            PhysicalPathState.Inaccessible => PhysicalPathResolution.Failed(
                state: state,
                logicalPath: logicalPath,
                failure: new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Fixture access failure.")),
            PhysicalPathState.Invalid => PhysicalPathResolution.Failed(
                state: state,
                logicalPath: logicalPath,
                failure: new FilesystemFailure(FilesystemFailureKind.InvalidPath, "Fixture invalid path.")),
            PhysicalPathState.Unsupported => PhysicalPathResolution.Failed(
                state: state,
                logicalPath: logicalPath,
                failure: new FilesystemFailure(FilesystemFailureKind.Unsupported, "Fixture unsupported path.")),
            PhysicalPathState.InputOutputFailure => PhysicalPathResolution.Failed(
                state: state,
                logicalPath: logicalPath,
                failure: new FilesystemFailure(FilesystemFailureKind.InputOutput, "Fixture input/output failure.")),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The fixture physical state is not supported."),
        };
}
