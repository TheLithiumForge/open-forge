namespace OpenForge.Cli.Core.Framework.Ownership;

/// <summary>
/// The generated workspace lock file. The tool writes and owns this one; its
/// authored counterpart is <c>open-forge.json</c>, and the npm-shaped pair of
/// names is what tells a reader which is which.
///
/// The types here are named for what the file records — ownership — rather than
/// for the file name, because <c>WorkspaceLock</c> already means the mutation
/// lease in <c>Framework/Mutation/Locking</c>.
/// </summary>
internal static class WorkspaceOwnershipDefinitions
{
    internal const string EntriesRegion = "entries";

    internal const string ManagedBlockRegion = "open-forge";

    internal const string DirectoryName = ".agents";
    internal const string FileName = "open-forge.lock.json";
    internal const string RelativePath = DirectoryName + "/" + FileName;

    /// <summary>
    /// The shape this release writes. A file carrying a different number is still
    /// read for the keys this release understands: the version is a fact to report,
    /// never a reason to refuse, because the lock is rebuilt best-effort when it
    /// cannot be trusted.
    /// </summary>
    internal const int SchemaVersion = 1;

    /// <summary>
    /// The schema an editor uses when somebody opens the generated file to see
    /// what the tool claims it owns. The CLI never fetches it.
    /// </summary>
    internal const string SchemaUrl =
        "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v1/open-forge.lock.schema.json";

    internal const string SchemaProperty = "$schema";
    internal const string SchemaVersionProperty = "schemaVersion";
    internal const string FrameworkProperty = "framework";
    internal const string ExtensionsProperty = "extensions";
    internal const string LibrariesProperty = "libraries";
}
