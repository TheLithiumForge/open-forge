using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemoveReferenceDetachmentTests
{
    [Fact(DisplayName = "Route Remove detachment facts preserve the visible label and surrounding authored bytes"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void DetachmentFactsPreserveVisibleLabelAndAuthoredBytes()
    {
        var before = "Prefix [Readable guide](.agents/guidance/old%20guide.md#part) suffix.";
        var expected = "Prefix Readable guide suffix.";
        var detachment = RouteRemoveTestData.Detachment(
            before: before,
            expected: expected) with
        {
            OriginalDestination = ".agents/guidance/old%20guide.md#part",
            VisibleLabel = "Readable guide",
        };

        Assert.Equal("README.md", detachment.SourcePath);
        Assert.Equal(before, detachment.Before);
        Assert.Equal(expected, detachment.Expected);
        Assert.Equal("Readable guide", detachment.VisibleLabel);
        Assert.Equal(".agents/guidance/old%20guide.md#part", detachment.OriginalDestination);
        Assert.Equal(3, detachment.Location.Line);
        Assert.Equal(5, detachment.Location.Column);
        Assert.True(detachment.Location.ByteLength > 0);
    }

    [Fact(DisplayName = "Route Remove reference plans retain complete scan and document edit boundaries"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void ReferencePlansRetainCompleteScanAndDocumentBoundaries()
    {
        var scan = new RouteRemoveReferenceScan
        {
            Documents =
            [
                new RouteRemoveReferenceDocumentPlan
                {
                    SourcePath = "README.md",
                    Snapshot = null!,
                    IntendedText = "Prefix Readable guide suffix.",
                    Edits =
                    [
                        new RouteRemoveReferenceDocumentEdit
                        {
                            Location = RouteRemoveTestData.Detachment().Location,
                            Before = RouteRemoveTestData.Detachment().Before,
                            Expected = RouteRemoveTestData.Detachment().Expected,
                        },
                    ],
                },
            ],
            Detachments = [RouteRemoveTestData.Detachment()],
            OccurrenceCount = 1,
        };

        var result = new RouteRemoveReferenceScanResult(scan, null);

        Assert.Same(scan, result.Scan);
        Assert.Null(result.Boundary);
        Assert.Equal(1, result.Scan?.OccurrenceCount);
        Assert.Single(result.Scan?.Detachments ?? []);
        Assert.Throws<ArgumentException>(
            () => new RouteRemoveReferenceScanResult(null, null));
        Assert.Throws<ArgumentException>(
            () => new RouteRemoveReferenceScanResult(scan, RouteRemoveTestData.Formation()));
    }

    [Fact(DisplayName = "Route Remove reference postcondition retains interruption state without inventing success"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void ReferencePostconditionRetainsInterruption()
    {
        var result = new RouteRemoveReferencePostRemoveResult(
            RouteRemoveReferencePostRemoveState.Interrupted,
            "The complete reference verification was cancelled.");

        Assert.Equal(RouteRemoveReferencePostRemoveState.Interrupted, result.State);
        Assert.Equal("The complete reference verification was cancelled.", result.Cause);
    }
}
