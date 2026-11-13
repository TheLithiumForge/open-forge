using System.Reflection;

namespace OpenForge.Cli.TestSupport.Tools;

// This is the single test oracle for the embedded catalogue's external resource layout.
public static class ToolDraftSource
{
    public const string CodeGraph = "codegraph-1.6.2";
    public const string CodebaseMemory = "codebase-memory-0.12.1";
    public const string DefinitionExtension = ".json";
    public const string Pending = "pending";
    public const string Blocked = "blocked";
    public const string SourceCommitPattern = "^[0-9a-f]{40}$";
    private const string ResourcePrefix = "OpenForge.Tools.Catalogue/";
    private const string DraftFolder = "drafts/";
    private const string ChecksumsSuffix = "-checksums.txt";

    public static byte[] ReadDraft(Assembly assembly, string pin) => Read(assembly, DraftFolder + pin + DefinitionExtension);

    public static byte[] ReadChecksums(Assembly assembly, string id) => Read(assembly, DraftFolder + id + ChecksumsSuffix);

    public static byte[] Read(Assembly assembly, string relativePath)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrEmpty(relativePath);
        var resourceName = assembly.GetManifestResourceNames().Single(name => name.Replace('\\', '/') == ResourcePrefix + relativePath);
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidDataException("The packaged tool draft resource is missing.");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
