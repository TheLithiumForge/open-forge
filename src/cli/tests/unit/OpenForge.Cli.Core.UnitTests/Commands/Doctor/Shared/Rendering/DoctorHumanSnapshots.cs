namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Rendering;

internal static class DoctorHumanSnapshots
{
    internal const string Compact = """
        Link: .agents/directives/review.md:12:4
            Destination: ../guidance/testing.md
          WARNING  Broken link [reference.target-missing]; choose a target after reviewing the evidence
            The linked file was not found.
          WARNING  One possible target remains unselected [reference.candidates-one]; choose a target after reviewing the evidence
            Possible targets: 1; none selected
              Target: .agents/guidance/testing.md
        """;

    internal const string Expanded = """
        Link: .agents/directives/review.md:12:4
            Destination: ../guidance/testing.md
          WARNING  Broken link [reference.target-missing]
            The linked file was not found.
            Resolution: choose a target after reviewing the evidence
          WARNING  One possible target remains unselected [reference.candidates-one]
            Resolution: choose a target after reviewing the evidence
            Possible targets: 1; none selected
              Target: .agents/guidance/testing.md
                Filename match: testing.md
                Read from: local links
            Observed: missing
            Read from: local links
        """;
}
