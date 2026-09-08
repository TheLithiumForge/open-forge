using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation;

internal static class RelativeFileLinkValidator
{
    internal static RelativeFileLinkValidationResult Validate(
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation expected,
        NoFollowLeafObservation actual)
    {
        ArgumentNullException.ThrowIfNull(effect);
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);
        if (!PhysicalIdentityTracker.PathComparer.Equals(expected.LogicalPath, actual.LogicalPath))
        {
            return RelativeFileLinkValidationResult.Blocked(
                effect,
                actual,
                "The expected and observed link leaves do not identify the same logical path.");
        }

        if (!Matches(effect.Expected, expected))
        {
            return RelativeFileLinkValidationResult.Blocked(
                effect,
                actual,
                "The supplied link expectation does not match the typed effect.");
        }

        if (actual.Failure is { } failure)
        {
            return RelativeFileLinkValidationResult.Failed(effect, failure);
        }

        if (actual.State is NoFollowLeafState.Link
            or NoFollowLeafState.ReparsePoint
            or NoFollowLeafState.Special
            or NoFollowLeafState.Inaccessible
            or NoFollowLeafState.Unknown)
        {
            return RelativeFileLinkValidationResult.Blocked(
                effect,
                actual,
                "The destination leaf is not an exact managed relative file link or a proven missing leaf.");
        }

        return actual == expected
            ? RelativeFileLinkValidationResult.Matched(effect, expected, actual)
            : RelativeFileLinkValidationResult.Mismatched(
                effect,
                expected,
                actual,
                "The destination leaf no longer matches the exact planned link state.");
    }

    private static bool Matches(
        RelativeFileLinkState expected,
        NoFollowLeafObservation observation)
        => expected.State == observation.State
            && (expected.Link is null || expected.Link == observation.RelativeFileLink);
}
