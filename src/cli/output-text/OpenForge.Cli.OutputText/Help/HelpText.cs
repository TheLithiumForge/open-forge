namespace OpenForge.Cli.OutputText.Help;

internal static class HelpText
{
    private static readonly Lazy<string> Concepts = new(ReadConcepts);

    internal static string ConceptsMarkdown() => Concepts.Value;

    private static string ReadConcepts()
    {
        using var stream = typeof(HelpText).Assembly.GetManifestResourceStream("OpenForge.Cli.OutputText.Help/concepts.md")
            ?? throw new InvalidDataException("The embedded concepts topic is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // @OpenForgeText help.topics-heading
    internal static string TopicsHeading() => "Help topics:";

    // @OpenForgeText help.topic-summary
    internal static string TopicSummary(string topic) => topic switch
    {
        "concepts" => "How Open Forge works: what loads, rules, tags, reuse and the file formats",
        _ => string.Empty,
    };

    // @OpenForgeText help.command-options
    internal static string CommandOptions()
        => "Run open-forge help <topic> to read one. open-forge <command> --help shows a command's options.\n";

    // @OpenForgeText help.completed
    internal static string Completed() => "Help is ready.";

    // @OpenForgeText help.invalid
    internal static string Invalid(string cause) => $"Cannot show help: {cause}";

    // @OpenForgeText help.invalid-title
    internal static string InvalidTitle() => "Help input is invalid";

    // @OpenForgeText help.failed-title
    internal static string FailedTitle() => "Built-in help could not be read";

    // @OpenForgeText help.cancelled
    internal static string Cancelled() => "Help was cancelled.";

    // @OpenForgeText help.syntax
    internal static string HelpSyntax() => "open-forge help [<topic>] [options]";

    // @OpenForgeText help.examples
    internal static string HelpExamples() => "open-forge help\nopen-forge help concepts";

    // @OpenForgeText help.behavior
    internal static string HelpBehavior() => """
        With no topic, lists available topics. The only topic is concepts.
        Reads built-in help without a workspace. Changes no files.
        Use open-forge <command> --help for a command's options.
        """;

    // @OpenForgeText help.related-commands
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelatedCommands()
        => [("context", "Read workspace instructions and context."), ("route list", "List workspace routes.")];
}
