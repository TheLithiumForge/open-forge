using System.Globalization;

namespace OpenForge.Cli.OutputText.Doctor;

internal static class DoctorWording
{
    internal static string OutsideDirectoryMissingTitle() => "Registered outside directory is missing";
    internal static string OutsideUnlistedTitle() => "Outside route has no direct Loader row";
    internal static string OutsideUnsafeTitle() => "Outside route boundary is unsafe";
    internal static string OutsideUnavailableTitle() => "Outside route observation is unavailable";
    internal static string OutsideLoaderTargetMissingTitle() => "Outside Loader target is missing";
    internal static string OutsideDirectoryMissing(string path)
        => $"The registered outside directory {path} is missing. Review its settings registration.";
    internal static string OutsideAnchorMissing(string path)
        => $"The registered outside directory has no recognized root entrypoint. Its canonical entrypoint is {path}.";
    internal static string OutsideUnlisted(string path)
        => $"The registered outside directory {path} has no direct root Loader row. Other entries or explicit selection can still load its sources.";
    internal static string OutsideUnsafe(string path, string? cause)
        => $"The registered outside boundary {path} is unsafe: {cause ?? "safe ordinary containment could not be established"}.";
    internal static string OutsideUnavailable(string path, string? cause)
        => $"The registered outside observation {path} is unavailable: {cause ?? "the required evidence could not be established"}.";
    internal static string OutsideAmbiguous(string path)
        => $"The registered outside root has competing entrypoints, including {path}. Keep one recognized root entrypoint.";
    internal static string OutsideLoaderTargetMissing(string path)
        => $"The direct Loader target {path} is missing or unreachable. Review the authored row independently from the registered root entrypoint.";
    internal static string ReviewOutsideRegistration(string path)
        => $"Review {path} and its outside-route settings registration by hand.";

    internal static string OwnershipObservation(string what)
        => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.OwnershipObservation(what);
}
