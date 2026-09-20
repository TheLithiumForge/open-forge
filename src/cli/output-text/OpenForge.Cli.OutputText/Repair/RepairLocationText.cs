using System.Globalization;

namespace OpenForge.Cli.OutputText.Repair;

internal static class RepairLocationText
{
    // @OpenForgeText repair.proposal.not-repairable
    internal static string ProposalUnavailable(string location)
        => $"The link at {location} is not one doctor reports as repairable.";

    // @OpenForgeText repair.facts.conflicting
    internal static string FactsConflicting(string location)
        => $"The current Repair facts disagree about {location}.";
}
