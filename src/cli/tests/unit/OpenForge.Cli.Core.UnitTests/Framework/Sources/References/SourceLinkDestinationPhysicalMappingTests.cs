using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.References;

public sealed class SourceLinkDestinationPhysicalMappingTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Returned noncontained physical states retain exact source-link facts before target I/O"),
        InlineData((int)PhysicalPathState.Dangling, (int)SourceLinkTargetResolution.PhysicalEscape, (int)SourceLinkDestinationFindingCode.TargetUnsafe,
            "The local destination physical boundary is unsafe."),
        InlineData((int)PhysicalPathState.Inaccessible, (int)SourceLinkTargetResolution.Unreadable, (int)SourceLinkDestinationFindingCode.TargetUnreadable,
            "The local destination target could not be read."),
        InlineData((int)PhysicalPathState.External, (int)SourceLinkTargetResolution.PhysicalEscape, (int)SourceLinkDestinationFindingCode.TargetUnsafe,
            "The local destination physical boundary is unsafe."),
        InlineData((int)PhysicalPathState.Cycle, (int)SourceLinkTargetResolution.PhysicalEscape, (int)SourceLinkDestinationFindingCode.TargetUnsafe,
            "The local destination physical boundary is unsafe."),
        InlineData((int)PhysicalPathState.Invalid, (int)SourceLinkTargetResolution.Malformed, (int)SourceLinkDestinationFindingCode.DestinationMalformed,
            "The local destination path is malformed."),
        InlineData((int)PhysicalPathState.Unsupported, (int)SourceLinkTargetResolution.PhysicalEscape, (int)SourceLinkDestinationFindingCode.TargetUnsafe,
            "The local destination physical boundary is unsafe."),
        InlineData((int)PhysicalPathState.InputOutputFailure, (int)SourceLinkTargetResolution.Unreadable, (int)SourceLinkDestinationFindingCode.TargetUnreadable,
            "The local destination target could not be read."),
        Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public async Task NoncontainedStatesRetainExactFacts(int state, int expectedResolution, int expectedFinding, string expectedCause)
    {
        var workspace = SourceInventoryTestData.Workspace();
        var catalogue = new SourceCatalogue(workspace: workspace, candidates: [], sources: [], issues: [], isCancelled: false);
        var physicalCalls = 0;
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: (_, lexicalPath) =>
            {
                physicalCalls++;
                Assert.Equal(SourceInventoryTestData.Physical(".agents/target.md"), lexicalPath);
                return PhysicalResult((PhysicalPathState)state, lexicalPath);
            },
            strictUtf8Reader: (_, _, _) => throw new InvalidOperationException("A noncontained target must not be read."),
            markdownParser: _ => throw new InvalidOperationException("A noncontained target must not be parsed."));
        var input = new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = catalogue,
            SourceCanonicalPath = ".agents/source.md",
            RawDestination = "target.md#Heading",
        };

        var facts = await resolver.ResolveAsync(input, TestContext.Current.CancellationToken);

        Assert.Equal(1, physicalCalls);
        Assert.Equal(SourceLinkTargetKind.Local, facts.Target.Kind);
        Assert.Equal((SourceLinkTargetResolution)expectedResolution, facts.Target.Resolution);
        Assert.Equal(".agents/target.md", facts.Target.Path);
        Assert.Null(facts.Target.Id);
        Assert.Null(facts.Target.PhysicalPath);
        Assert.Null(facts.Target.Layer);
        Assert.Null(facts.Target.Network);
        Assert.Equal("Heading", facts.Fragment);
        Assert.Null(facts.CanonicalFragment);
        var finding = Assert.IsType<SourceLinkDestinationFinding>(facts.Finding);
        Assert.Equal((SourceLinkDestinationFindingCode)expectedFinding, finding.Code);
        Assert.Equal(expectedCause, finding.Cause);
        Assert.Empty(finding.Candidates);
    }

    private static PhysicalPathResolution PhysicalResult(PhysicalPathState state, string logicalPath)
        => state switch
        {
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
            PhysicalPathState.Contained or PhysicalPathState.Missing =>
                throw new ArgumentOutOfRangeException(nameof(state), state, "The fixture requires a noncontained failure state."),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The fixture requires a noncontained failure state."),
        };
}
