using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Shared.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveAppliedVerifier
{
    private async ValueTask<RouteRemoveAppliedVerification?> VerifyPersistenceAsync(
        RouteRemoveAppliedVerificationInput input,
        CancellationToken cancellationToken)
    {
        var settings = await VerifySettingsAsync(input.Plan, input.Progress, cancellationToken)
            .ConfigureAwait(false);
        if (settings is not null)
        {
            return Failed(settings);
        }

        var ownership = await WorkspaceOwnershipReader.ReadAsync(
            _physicalPathResolver,
            input.Plan.Request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (!RouteRemovePersistencePlanner.IsOwnershipTrusted(ownership))
        {
            return Failed("The final Route Remove ownership record is unavailable or unsupported.");
        }

        var change = input.Plan.Projection.OwnershipChange;
        if (change is null)
        {
            if (input.Progress.OwnershipReceipt is not null
                || !MatchesOwnershipObservation(input.Plan.Projection.Ownership, ownership))
            {
                return Failed("The workspace ownership record changed even though no ownership update was planned.");
            }

            return null;
        }

        if (input.Progress.OwnershipReceipt is not { } applied
            || !Matches(change, applied.Receipt)
            || applied.Receipt.After is not { } after
            || ownership.State != WorkspaceOwnershipReadState.Complete
            || ownership.Snapshot is not { } snapshot
            || !snapshot.Bytes.AsSpan().SequenceEqual(change.IntendedBytes.AsSpan())
            || after.Expectation != snapshot.Expectation)
        {
            return Failed("The planned Route Remove ownership release has no exact verified final lock state.");
        }

        var released = input.Plan.Projection.ContentPathsToRelease
            .ToHashSet(StringComparer.Ordinal);
        if (RouteRemoveOwnershipProjector.HasSubjectClaims(ownership, input.Plan.Projection.Subject)
            || RouteOwnershipEvidence.Claims(ownership).Any(claim => released.Contains(claim.Path)))
        {
            return Failed("A selected Route Remove content ownership claim remains after publication.");
        }

        return null;
    }

    private async ValueTask<string?> VerifySettingsAsync(
        RouteRemovePlan plan,
        RouteRemoveApplicationProgress progress,
        CancellationToken cancellationToken)
    {
        var change = plan.Projection.SettingsChange;
        if (change is null)
        {
            if (progress.SettingsReceipt is not null)
            {
                return "Route Remove produced a settings receipt without a planned settings change.";
            }
        }
        FileStateSnapshot? settingsAfter = null;
        if (change is not null)
        {
            if (progress.SettingsReceipt is not { } applied
                || !Matches(change, applied.Receipt)
                || applied.Receipt.After is not { } after)
            {
                return "The planned Route Remove settings change has no exact verified receipt.";
            }

            settingsAfter = after;
        }

        var settings = await WorkspaceSettingsReader.ReadAsync(
            _physicalPathResolver,
            plan.Request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || settings.Snapshot is null)
        {
            return settings.Cause ?? "The final Route Remove settings are unavailable or invalid.";
        }

        if (change is null)
        {
            if (!plan.Projection.Settings.MatchesObservation(settings))
            {
                return "The workspace settings changed after the Route Remove plan was accepted.";
            }
        }
        else if (settings.State != WorkspaceSettingsReadState.Complete
            || !settings.Snapshot.Bytes.AsSpan().SequenceEqual(change.IntendedBytes.AsSpan())
            || settings.Snapshot.Expectation != settingsAfter?.Expectation)
        {
            return "The final settings bytes do not match the exact planned Route Remove exclusions.";
        }

        var selection = plan.Projection.RemovalSelection;
        var paths = selection.Categories.Select(category => $".agents/{category}")
            .Concat(selection.Files)
            .Concat(selection.Directories);
        return paths.All(path => WorkspaceRemovals.IsPathRemoved(path, settings.Document))
            ? null
            : "The final settings do not retain every selected Route Remove exclusion.";
    }

    private static bool MatchesOwnershipObservation(
        WorkspaceOwnershipRead expected,
        WorkspaceOwnershipRead actual)
    {
        if (expected.State != actual.State)
        {
            return false;
        }

        if (expected.Snapshot is not { } before)
        {
            return actual.Snapshot is null;
        }

        return actual.Snapshot is { } after
            && before.Expectation == after.Expectation
            && before.Bytes.AsSpan().SequenceEqual(after.Bytes.AsSpan());
    }
}
