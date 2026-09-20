using System.Globalization;

namespace OpenForge.Cli.OutputText.References;

internal static class ReferencesWording
{
    internal static string UnknownSourceId(string reference) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.UnknownSource(reference);

    // @OpenForgeText references.wording.is-not-under-agents
    internal static string SourceOutsideAgents(string path) => $"{path} is not under .agents.";

    internal static string FilterNotASource(string value)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FilterNotASource(value);

    // @OpenForgeText references.wording.could-not-be-scanned-safely-for-incoming-links-so-none-of-its-links-were-counted
    internal static string CandidateUnsafe(string path)
        => $"{path} could not be scanned safely for incoming links, so none of its links were counted.";

    // @OpenForgeText references.wording.overwrite-md-has-no-base-file-and-was-not-scanned
    internal static string LayerUnresolved(string name)
        => $"{name}.overwrite.md has no base file and was not scanned.";

    // @OpenForgeText references.wording.could-not-be-read-and-was-not-scanned
    internal static string InspectionUnavailable(string path)
        => $"{path} could not be read and was not scanned.";

    // @OpenForgeText references.wording.is-not-valid-utf-8-so-it-was-not-scanned
    internal static string InvalidEncoding(string path)
        => $"{path} is not valid UTF-8, so it was not scanned.";

    // @OpenForgeText references.wording.the-link-at-has-an-encoding-that-cannot-be-resolved-so-it-was-not-counted
    internal static string LinkEncodingInvalid(string location)
        => $"The link at {location} has an encoding that cannot be resolved, so it was not counted.";

    // @OpenForgeText references.wording.the-entries-section-of-could-not-be-identified-so-its-links-could-not-be-told-apart-from-authored-links
    internal static string GeneratedRegionUnavailable(string path)
        => $"The Entries section of {path} could not be identified, so its links could not be told apart from authored links.";

    // @OpenForgeText references.wording.the-link-at-points-outside-the-workspace
    internal static string TargetUnsafe(string location)
        => $"The link at {location} points outside the workspace.";

    // @OpenForgeText references.wording.the-link-at-could-point-to-more-than-one-file
    internal static string TargetAmbiguous(string location)
        => $"The link at {location} could point to more than one file.";

    // @OpenForgeText references.wording.in-out
    internal static string Counts(int incoming, int outgoing)
        => string.Create(CultureInfo.InvariantCulture, $"{incoming} in, {outgoing} out");
}
