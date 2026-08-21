namespace OpenForge.Cli.Core.Shell.Parsing;

internal static class CliGlobalInputReader
{
    internal static CliGlobalInput Read(CliParseOutcome parse)
    {
        ArgumentNullException.ThrowIfNull(parse);
        return CliGlobalInput.Read(parse);
    }

    internal static CliGlobalInput ReadAvailable(CliParseOutcome parse)
    {
        ArgumentNullException.ThrowIfNull(parse);
        return CliGlobalInput.ReadAvailable(parse);
    }
}
