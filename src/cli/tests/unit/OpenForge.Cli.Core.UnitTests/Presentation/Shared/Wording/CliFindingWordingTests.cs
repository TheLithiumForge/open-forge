using System.Globalization;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Wording;

public sealed class CliFindingWordingTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Shared finding wording preserves the accepted workspace target and inspection catalogue literals"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void WorkspaceTargetAndInspectionFamiliesRemainExact()
    {
        Assert.Equal("Cannot install: problem.", CliFindingWording.InvalidInput("install", "problem"));
        Assert.Equal("Cannot use workspace.md as the workspace: it is not a directory.", CliFindingWording.WorkspaceNotDirectory("workspace.md"));
        Assert.Equal("target.md already exists and is not managed by Open Forge.", CliFindingWording.TargetOccupied("target.md"));
        Assert.Equal("target.md could not be read completely.", CliFindingWording.InspectionIncomplete("target.md"));
        Assert.Equal("path\n<raw> already exists and is not managed by Open Forge.", CliFindingWording.TargetOccupied("path\n<raw>"));
        Assert.Equal("Install needs confirmation, and this session cannot ask.", CliFindingWording.ConfirmationRequired("Install"));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Shared finding wording preserves lifecycle ownership permission and source catalogue literals"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void LifecycleOwnershipPermissionAndSourceFamiliesRemainExact()
    {
        Assert.Equal("The changes were applied, but the ownership record .agents/open-forge.lock.json could not be written.", CliFindingWording.LifecyclePublicationFailed());
        Assert.Equal(".agents/open-forge.lock.json could not be read completely.", CliFindingWording.LifecycleUnavailable());
        Assert.Equal(".agents/open-forge.lock.json is invalid: malformed.\u0000.", CliFindingWording.LifecycleBlocked("malformed.\u0000"));
        Assert.Equal("No ownership record exists, so installed libraries cannot be read from it.", CliFindingWording.OwnershipObservation("installed libraries"));
        Assert.Equal("target.md is owned by the Framework, so install cannot change it.", CliFindingWording.OwnershipConflict("target.md", "the Framework", "install"));
        Assert.Equal("target.md is managed by the alpha Extension, so remove cannot delete it.", CliFindingWording.OwnershipClaimed("target.md", "the alpha Extension", "remove", "delete"));
        Assert.Equal("target.md has changed since it was installed.", CliFindingWording.ManagedDivergence("target.md"));
        Assert.Equal("Cannot install: it writes outside .agents and no grant allows that.", CliFindingWording.PermissionRequired("install"));
        Assert.Equal("You declined the destinations, so nothing was changed.", CliFindingWording.PermissionDeclined());
        Assert.Equal(".agents/open-forge.json cannot be used: malformed.", CliFindingWording.PermissionsInvalid("malformed"));
        Assert.Equal(".agents/open-forge.json could not be read.", CliFindingWording.PermissionsUnavailable());
        Assert.Equal(".agents/open-forge.json changed after the plan was made. Nothing was changed.", CliFindingWording.PermissionsChanged());
        Assert.Equal("The grant could not be saved to .agents/open-forge.json.", CliFindingWording.PermissionWriteFailed());
        Assert.Equal("The ID alpha matches more than one file. Use the exact path.", CliFindingWording.IdentityCollision("alpha"));
        Assert.Equal("route could match more than one route.", CliFindingWording.RouteAmbiguous("route"));
        Assert.Equal("--include or --exclude value matches more than one source. Use the exact path.", CliFindingWording.SelectorAmbiguous("value"));
        Assert.Equal("--include or --exclude value points outside the workspace.", CliFindingWording.SelectorUnsafe("value"));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Shared finding wording preserves payload framework selection interaction and unknown-id literals"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void PayloadFrameworkSelectionInteractionAndUnknownFamiliesRemainExact()
    {
        Assert.Equal("The Framework bundled in this CLI could not be read completely.", CliFindingWording.PayloadUnavailable());
        Assert.Equal("The Framework bundled in this CLI is invalid.", CliFindingWording.PayloadInvalid());
        Assert.Equal("The Framework files this command needs could not be read completely.", CliFindingWording.FrameworkUnavailable());
        Assert.Equal("The Framework files this command needs could not be verified.", CliFindingWording.FrameworkUnsafe());
        Assert.Equal("Install needs to know which packages. Pass their IDs or --all.", CliFindingWording.SelectionRequired("Install", false));
        Assert.Equal("Install needs to know which packages. Pass their IDs or --all. This session cannot ask.", CliFindingWording.SelectionRequired("Install", true));
        Assert.Equal("Input ended before a choice was made. Nothing was changed.", CliFindingWording.InteractionEnded());
        Assert.Equal("Install was cancelled. Nothing was changed.", CliFindingWording.Interrupted("Install"));
        Assert.Equal("No Library has the ID alpha.", CliFindingWording.UnknownId("Library", "alpha"));
        Assert.Equal("No source has the ID source.", CliFindingWording.UnknownSource("source"));
    }

    // Bounding a runtime cause removes the exception type and error code, never the subject. A
    // reader who is told only that "the filesystem operation failed" cannot act; the path is the
    // whole point of the message. Where the caller framed the failure, its prefix names the
    // subject; otherwise the path the operating system quoted is kept.
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Shared cause wording bounds runtime detail and keeps the subject")]
    [InlineData("The Extension manifest is invalid: 'm' is an invalid start of a property name. LineNumber: 0 | BytePositionInLine: 2.", "the content is not valid JSON")]
    [InlineData("The Extension manifest is invalid: Unable to translate bytes [FF] at index 0 from specified code page to Unicode.", "the content is not valid JSON")]
    [InlineData("The process cannot access the file 'extension.json' because it is being used by another process.", "extension.json: the file is in use by another process")]
    [InlineData("Access to the path 'extension.json' is denied.", "extension.json: filesystem access was denied")]
    [InlineData("Writing .agents/memory/_memory.md failed: UnauthorizedAccessException (0x80070005): Filesystem access was denied.", "Writing .agents/memory/_memory.md failed: filesystem access was denied")]
    [InlineData("IOException (0x80070020): The filesystem operation failed.", "the filesystem operation failed")]
    [Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void PlainCauseBoundsRuntimeDetailAndKeepsTheSubject(string raw, string expected)
        => Assert.Equal(expected, CliFindingWording.PlainCause(raw));

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Bounded causes never leak a runtime type name or error code")]
    [InlineData("The process cannot access the file 'extension.json' because it is being used by another process.")]
    [InlineData("Access to the path 'extension.json' is denied.")]
    [InlineData("Writing .agents/memory/_memory.md failed: UnauthorizedAccessException (0x80070005): Filesystem access was denied.")]
    [InlineData("IOException (0x80070020): The filesystem operation failed.")]
    [InlineData("The Extension manifest is invalid: 'm' is an invalid start of a property name. LineNumber: 0 | BytePositionInLine: 2.")]
    [Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void BoundedCausesCarryNoRuntimeDetail(string raw)
    {
        var bounded = CliFindingWording.PlainCause(raw);
        foreach (var forbidden in new[] { "Exception", "0x8007", "LineNumber:", "BytePositionInLine:", "Unable to translate bytes" })
        {
            Assert.DoesNotContain(forbidden, bounded, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Counted interruption wording remains invariant under a custom current culture"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void CountedInterruptedUsesInvariantNumbers()
    {
        var original = CultureInfo.CurrentCulture;
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NegativeSign = "NEG";

        try
        {
            CultureInfo.CurrentCulture = culture;
            Assert.Equal("Install was cancelled. Stopped after -1 of 2 changes.", CliFindingWording.Interrupted("Install", -1, 2));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
