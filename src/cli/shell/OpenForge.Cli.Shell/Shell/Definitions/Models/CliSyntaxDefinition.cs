namespace OpenForge.Cli.Core.Shell.Definitions.Models;

internal sealed record CliSyntaxDefinition
{
    internal CliSyntaxDefinition(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        Name = name;
        Description = description;
    }

    internal string Name { get; }

    internal string Description { get; }
}
