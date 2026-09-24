using System.Runtime.CompilerServices;
using TheLithium.Imprint;

namespace OpenForge.Cli.TestSupport.Snapshots;

public static class CommandOutputSnapshot
{
    public const string UpdateVariable = "OPENFORGE_SNAPSHOT_UPDATE";

    // Snapshot placement deliberately uses Imprint's default. Leave RootDirectory unset so each
    // capture stays under a `__snapshots__` directory beside the owning test class, where its
    // ownership is visible to the next author. JSON content keeps the `.json.` name segment while
    // the persisted snapshot remains a `.txt` file, so both formats stay together in one corpus.
    public const string JsonContentNameSegment = ".json.";

    // One comparison policy for every command output snapshot. Line endings are excluded on
    // purpose: the repository stores these files through `.gitattributes` `eol=lf`, so a capture
    // taken on Windows would otherwise disagree with the checked-out bytes for no product reason.
    // Content, case and trailing whitespace stay exact, and the CLI's own line-ending rule is
    // asserted by the renderers that own it rather than by every captured file.
    private static readonly SnapshotComparison OutputComparison = new()
    {
        IgnoreStringCase = false,
        IgnoreLineEndings = true,
        IgnoreTrailingWhitespace = false,
    };

    public static void MatchSnapshot(
        string actual,
        string name,
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string testName = "")
    {
        ArgumentNullException.ThrowIfNull(actual);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(testName);
        var update = Environment.GetEnvironmentVariable(UpdateVariable) == "1";
        var imprintUpdate = Environment.GetEnvironmentVariable("IMPRINT_UPDATE");
        if (!update && imprintUpdate is not null && imprintUpdate != "verify")
        {
            throw new InvalidOperationException($"Command output snapshots require {UpdateVariable}=1 to update.");
        }

        sourceFile = ResolveSourceFile(sourceFile);
        var directory = Path.GetDirectoryName(sourceFile)
            ?? throw new ArgumentException("The snapshot source must have a parent directory.", nameof(sourceFile));
        using var scope = TheLithium.Imprint.Snapshots.Begin(new SnapshotTestOptions
        {
            Identity = new SnapshotTestIdentity(
                ProjectDirectory: directory,
                SourceFile: sourceFile,
                Suite: testName,
                Test: name),
            Update = SnapshotUpdate.Verify,
            ArtifactDirectory = Path.Combine(RepositoryRoot(directory), "artifacts", "command-output-snapshots"),
        });
        actual.AssertSnapshot(name, new SnapshotOptions
        {
            Format = SnapshotFormat.Text,
            Update = update ? SnapshotUpdate.All : SnapshotUpdate.Verify,
            Comparison = OutputComparison,
        });
        scope.Complete();
    }

    public static void MatchDetailSnapshot(
        IReadOnlyDictionary<string, string> captures,
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string testName = "",
        ISnapshotComparer? diagnosticComparer = null)
    {
        ArgumentNullException.ThrowIfNull(captures);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(testName);
        var update = Environment.GetEnvironmentVariable(UpdateVariable) == "1";
        var imprintUpdate = Environment.GetEnvironmentVariable("IMPRINT_UPDATE");
        if (!update && imprintUpdate is not null && imprintUpdate != "verify")
        {
            throw new InvalidOperationException($"Command output snapshots require {UpdateVariable}=1 to update.");
        }

        sourceFile = ResolveSourceFile(sourceFile);
        var directory = Path.GetDirectoryName(sourceFile)
            ?? throw new ArgumentException("The snapshot source must have a parent directory.", nameof(sourceFile));
        var repositoryRoot = RepositoryRoot(directory);
        using var scope = TheLithium.Imprint.Snapshots.Begin(new SnapshotTestOptions
        {
            Identity = new SnapshotTestIdentity(
                ProjectDirectory: directory,
                SourceFile: sourceFile,
                Suite: Path.GetFileNameWithoutExtension(sourceFile),
                Test: testName),
            Update = SnapshotUpdate.Verify,
            ArtifactDirectory = Path.Combine(repositoryRoot, "artifacts", "command-output-snapshots"),
        });
        foreach (var capture in captures)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(capture.Key);
            ArgumentNullException.ThrowIfNull(capture.Value);
            var hasDiagnosticTemplate = capture.Key.EndsWith(".diagnostics", StringComparison.Ordinal)
                && diagnosticComparer is not null;
            capture.Value.AssertSnapshot(capture.Key, new SnapshotOptions
            {
                Format = SnapshotFormat.Text,
                // These expectations are templates, not normalized captures. Updating them
                // from received bytes would persist a real temporary path and lose the template.
                Update = update && !hasDiagnosticTemplate ? SnapshotUpdate.All : SnapshotUpdate.Verify,
                Comparison = OutputComparison,
                Comparer = hasDiagnosticTemplate
                    ? diagnosticComparer : new PlatformSnapshotComparer(
                    capture.Key.StartsWith("lock-held.", StringComparison.Ordinal),
                    manifestSharing: Path.GetFileNameWithoutExtension(sourceFile) == "ExtensionListBeforeOutputSnapshotTests"
                        && testName == "SourceBoundary"
                        && capture.Key.StartsWith("source-unreadable.", StringComparison.Ordinal)),
            });
        }
        scope.Complete();
    }

    private static string ResolveSourceFile(string sourceFile)
    {
        const string mappedRoot = "/_/";
        var normalized = sourceFile.Replace('\\', '/');
        if (!normalized.StartsWith(mappedRoot, StringComparison.Ordinal))
        {
            return sourceFile;
        }

        // CI maps compiler source paths for reproducibility. Snapshot identities
        // still point to the existing files beside each test in this checkout.
        return Path.Combine(
            RepositoryRoot(AppContext.BaseDirectory),
            normalized[mappedRoot.Length..].Replace('/', Path.DirectorySeparatorChar));
    }

    private static string RepositoryRoot(string sourceDirectory)
    {
        for (var directory = new DirectoryInfo(sourceDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "OpenForge.Cli.slnx")))
            {
                return directory.FullName;
            }
        }

        return sourceDirectory;
    }
}
