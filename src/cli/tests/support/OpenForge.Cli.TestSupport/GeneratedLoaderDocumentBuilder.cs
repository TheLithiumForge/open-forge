namespace OpenForge.Cli.TestSupport;

/// <summary>
/// Builds the simple generated Entries document used by Open Forge Loader fixtures.
/// </summary>
public static class GeneratedLoaderDocumentBuilder
{
    /// <summary>
    /// Builds an Open Forge Loader document with the supplied generated Entries
    /// content while preserving the fixture's exact whitespace and final newline.
    /// </summary>
    /// <param name="entries">The caller-supplied generated Entries content.</param>
    /// <returns>The simple generated Open Forge Loader document.</returns>
    public static string Build(string entries)
    {
        return OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Entries = entries,
            Prefix = "# Open Forge Loader",
            IncludeFinalLineEnding = false,
        });
    }
}
