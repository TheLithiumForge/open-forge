namespace OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

internal readonly record struct PublishedTerminalSize(short Width, short Height);
internal sealed record PublishedTerminalStep(string? WaitFor, string Input, PublishedTerminalSize? Resize = null);
internal sealed record PublishedTerminalScenario
{
    public PublishedTerminalSize Size { get; init; } = new(120, 30);
    public string TerminalName { get; init; } = "dumb";
    public required IReadOnlyList<PublishedTerminalStep> Steps { get; init; }
}
