using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Help;

internal static class HelpDefinitions
{
    internal const string CommandIdentity = "help";
    internal const string ConceptsTopic = "concepts";

    /// <summary>
    /// The heading of the shipped guide's file-format section, which the concepts topic prints last.
    /// </summary>
    internal const string FileFormatsHeading = "Writing workspace files";

    internal static IReadOnlyList<string> Topics { get; } = [ConceptsTopic];

    internal static CliSyntaxDefinition HelpCommand { get; } = new(
        CommandIdentity, "List help topics or read about Open Forge concepts.");

    internal static CliSyntaxDefinition TopicArgument { get; } = new(
        "topic", "The help topic to read. Available: concepts.");
}
