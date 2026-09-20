using System.Globalization;

namespace OpenForge.Cli.OutputText.Repair;

internal static class RepairWording
{
    // @OpenForgeText repair.wording.repair-stopped-after-of-links-were-rewritten
    internal static string Failed(int completed, int total)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"Repair stopped after {completed} of {total} links were rewritten.");

    // @OpenForgeText repair.wording.remaining-library-diagnosis
    internal static string RemainingLibraryDiagnosis()
        => "Library diagnosis remains";

    // @OpenForgeText repair.wording.inspect-remaining-library-problems
    internal static string InspectRemainingLibraryProblems()
        => "Inspect remaining Library problems with open-forge doctor.";

    // @OpenForgeText repair.wording.review-remaining-links-and-inspect-library-problems
    internal static string ReviewRemainingLinksAndInspectLibraryProblems()
        => "Review remaining broken links in Repair, then inspect remaining Library problems with open-forge doctor.";

    // @OpenForgeText repair.wording.broken-destination-detail
    internal static string BrokenDestination(string destination, string detail)
        => $"{destination}: {detail}";

    // @OpenForgeText repair.wording.could-not-be-checked-so-its-links-were-not-repaired
    internal static string DiagnosisIncomplete(string path)
        => $"{path} could not be checked, so its links were not repaired.";
}
