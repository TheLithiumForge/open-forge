using System.Globalization;

namespace OpenForge.Cli.OutputText.Shared;

internal static class CliFindingWording
{
    // @OpenForgeText shared.wording.before
    internal static string BeforeHash(string hash) => $"Before: {hash}";

    // @OpenForgeText shared.wording.after
    internal static string AfterHash(string hash) => $"After: {hash}";

    // @OpenForgeText shared.wording.cannot
    internal static string InvalidInput(string command, string problem) => $"Cannot {command}: {problem}.";

    // @OpenForgeText shared.wording.cannot-use-as-the-workspace-it-does-not-exist-or-cannot-be-read
    internal static string WorkspaceUnavailable(string path) => $"Cannot use {path} as the workspace: it does not exist or cannot be read.";

    // @OpenForgeText shared.wording.cannot-use-as-the-workspace-its-location-could-not-be-verified
    internal static string WorkspaceUnsafe(string path, string reason) => $"Cannot use {path} as the workspace: its location could not be verified ({reason}).";

    // @OpenForgeText shared.wording.changed-after-the-plan-was-made-nothing-was-changed
    internal static string TargetChanged(string path) => $"{path} changed after the plan was made. Nothing was changed.";

    // @OpenForgeText shared.wording.changed-while-changes-were-being-written-stopped-after-of-changes
    internal static string TargetChangedDuringApply(string path, long completed, long total) => FormattableString.Invariant($"{path} changed while changes were being written. Stopped after {completed} of {total} changes.");

    // @OpenForgeText shared.wording.cannot-be-written-safely
    internal static string TargetUnsafe(string path, string reason) => $"{path} cannot be written safely: {reason}.";

    // @OpenForgeText shared.wording.the-entries-section-of-could-not-be-identified
    internal static string GeneratedRegionUnsafe(string path, string reason) => $"The Entries section of {path} could not be identified: {reason}.";

    // @OpenForgeText shared.wording.the-entries-content-for-could-not-be-computed-because
    internal static string ProjectionUnavailable(string path, string reason) => $"The Entries content for {path} could not be computed because {reason}.";

    // @OpenForgeText shared.wording.the-frontmatter-of-could-not-be-read-completely
    internal static string MetadataIncomplete(string path) => $"The frontmatter of {path} could not be read completely.";

    // @OpenForgeText shared.wording.the-frontmatter-of-cannot-be-used
    internal static string MetadataUnsafe(string path, string reason) => $"The frontmatter of {path} cannot be used: {reason}.";

    // @OpenForgeText shared.wording.recovery-data-could-not-be-prepared-at-nothing-was-changed
    internal static string RecoveryUnavailable(string store) => $"Recovery data could not be prepared at {store}. Nothing was changed.";

    // @OpenForgeText shared.wording.recovery-data-from-an-earlier-run-exists-at-and-blocks-this-change-nothing-was-changed
    internal static string RecoveryConflict(string path) => $"Recovery data from an earlier run exists at {path} and blocks this change. Nothing was changed.";

    // @OpenForgeText shared.wording.the-changes-were-applied-but-the-recovery-bundle-at-could-not-be-removed
    internal static string RecoveryRetained(string path) => $"The changes were applied, but the recovery bundle at {path} could not be removed.";

    // @OpenForgeText shared.wording.writing-failed-stopped-after-of-changes-recovery-data
    internal static string WriteFailed(string path, long completed, long total, string recovery) => FormattableString.Invariant($"Writing {path} failed. Stopped after {completed} of {total} changes. Recovery data: {recovery}.");

    // @OpenForgeText shared.wording.did-not-verify-after-it-was-written-recovery-data
    internal static string VerificationFailed(string path, string recovery) => $"{path} did not verify after it was written. Recovery data: {recovery}.";

    // @OpenForgeText shared.wording.matches-more-than-one-source-use-the-exact-path
    internal static string SourceAmbiguous(string reference) => $"{reference} matches more than one source. Use the exact path.";

    // @OpenForgeText shared.wording.could-not-be-verified-to-be-inside-the-workspace
    internal static string SourceUnsafe(string path) => $"{path} could not be verified to be inside the workspace.";

    // @OpenForgeText shared.wording.stopped-because-of-an-unexpected-error
    internal static string OperationFailed(string command, string reason) => $"{command} stopped because of an unexpected error: {reason}.";

    // @OpenForgeText shared.wording.needs-confirmation-and-this-session-cannot-ask
    internal static string ConfirmationRequired(string command) => $"{command} needs confirmation, and this session cannot ask.";

    // @OpenForgeText shared.wording.cannot-use-as-the-workspace-it-is-not-a-directory
    internal static string WorkspaceNotDirectory(string path) => $"Cannot use {path} as the workspace: it is not a directory.";

    // @OpenForgeText shared.wording.already-exists-and-is-not-managed-by-open-forge
    internal static string TargetOccupied(string path) => $"{path} already exists and is not managed by Open Forge.";

    // @OpenForgeText shared.wording.could-not-be-read-completely
    internal static string InspectionIncomplete(string path) => $"{path} could not be read completely.";

    // @OpenForgeText shared.wording.agents-open-forge-lock-json-is-invalid
    internal static string LifecycleBlocked(string reason) => $".agents/open-forge.lock.json is invalid: {reason}.";

    // @OpenForgeText shared.wording.no-ownership-record-exists-so-cannot-be-read-from-it
    internal static string OwnershipObservation(string what) => $"No ownership record exists, so {what} cannot be read from it.";

    // @OpenForgeText shared.wording.is-owned-by-so-cannot-change-it
    internal static string OwnershipConflict(string path, string owner, string command) => $"{path} is owned by {owner}, so {command} cannot change it.";

    // @OpenForgeText shared.wording.is-managed-by-so-cannot-it
    internal static string OwnershipClaimed(string path, string owner, string command, string verb) => $"{path} is managed by {owner}, so {command} cannot {verb} it.";

    // @OpenForgeText shared.wording.has-changed-since-it-was-installed
    internal static string ManagedDivergence(string path) => $"{path} has changed since it was installed.";

    // @OpenForgeText shared.wording.cannot-it-writes-outside-agents-and-no-grant-allows-that
    internal static string PermissionRequired(string command) => $"Cannot {command}: it writes outside .agents and no grant allows that.";

    // @OpenForgeText shared.wording.agents-open-forge-json-cannot-be-used
    internal static string PermissionsInvalid(string reason) => $".agents/open-forge.json cannot be used: {reason}.";

    // @OpenForgeText shared.wording.the-id-matches-more-than-one-file-use-the-exact-path
    internal static string IdentityCollision(string id) => $"The ID {id} matches more than one file. Use the exact path.";

    // @OpenForgeText shared.wording.could-match-more-than-one-route
    internal static string RouteAmbiguous(string id) => $"{id} could match more than one route.";

    // @OpenForgeText shared.wording.include-or-exclude-matches-more-than-one-source-use-the-exact-path
    internal static string SelectorAmbiguous(string value) => $"--include or --exclude {value} matches more than one source. Use the exact path.";

    // @OpenForgeText shared.wording.include-or-exclude-points-outside-the-workspace
    internal static string SelectorUnsafe(string value) => $"--include or --exclude {value} points outside the workspace.";

    // @OpenForgeText shared.wording.was-cancelled-nothing-was-changed
    internal static string Interrupted(string command) => $"{command} was cancelled. Nothing was changed.";

    // @OpenForgeText shared.wording.was-cancelled-stopped-after-of-changes
    internal static string Interrupted(string command, long completed, long total)
        => FormattableString.Invariant($"{command} was cancelled. Stopped after {completed} of {total} changes.");

    // @OpenForgeText shared.wording.no-has-the-id
    internal static string UnknownId(string subject, string id) => $"No {subject} has the ID {id}.";

    // @OpenForgeText shared.wording.no-source-has-the-id
    internal static string UnknownSource(string id) => $"No source has the ID {id}.";
}
