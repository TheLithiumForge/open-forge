namespace OpenForge.Cli.Core.Shell.Parsing;

internal sealed class CliParser
{
    private readonly CliCommandTree _tree;

    internal CliParser(CliCommandTree tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
        _tree = tree;
    }

    internal CliParseOutcome Parse(string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return _tree.Parse(arguments);
    }
}
