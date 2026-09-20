namespace OpenForge.Cli.Core.Shell.Presentation.Models;

internal sealed record CliHelpSection
{
    internal CliHelpSection(string heading, string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        Heading = heading;
        Body = body;
    }

    internal string Heading { get; }

    internal string Body { get; }
}
