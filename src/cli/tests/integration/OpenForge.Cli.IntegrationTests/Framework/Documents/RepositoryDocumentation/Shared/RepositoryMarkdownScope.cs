namespace OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.Shared;

internal static class RepositoryMarkdownScope
{
    private const string SolutionFileName = "OpenForge.Cli.slnx";
    private const string MarkdownSuffix = ".md";
    private const string ExtensionsRoot = "src/extensions";

    private static readonly IReadOnlyList<string> RequiredFiles =
    [
        "README.md",
        "src/extensions/README.md",
        ".agents/memory/crystallized/decisions/framework/workspace-state-files.md",
        ".agents/memory/crystallized/documents/cli/_cli.md",
        ".agents/memory/crystallized/documents/cli/architecture.md",
        ".agents/memory/crystallized/documents/cli/command-contract-set.md",
        ".agents/memory/crystallized/documents/cli/distribution.md",
        ".agents/memory/crystallized/documents/cli/shared-operation-contract.md",
    ];

    private static readonly IReadOnlyList<string> RecursiveRoots =
    [
        "docs",
        "src/docusaurus/docs",
        ".agents/maps",
        ".agents/memory/crystallized/documents/cli/contracts",
        ".agents/memory/crystallized/documents/cli/layers",
        ".agents/memory/crystallized/documents/cli/technical-designs",
    ];

    private static readonly IReadOnlySet<string> ExcludedSubtrees = new HashSet<string>(StringComparer.Ordinal)
    {
        "docs/cli-experience-fixtures",
        "docs/extension-candidates",
    };

    internal static IReadOnlyList<string> Enumerate(string repositoryRoot)
    {
        var root = ValidateRepositoryRoot(repositoryRoot);
        var paths = new HashSet<string>(StringComparer.Ordinal);

        foreach (var relativePath in RequiredFiles)
        {
            AddRequiredFile(root, relativePath, paths);
        }

        foreach (var relativeRoot in RecursiveRoots)
        {
            AddRecursiveFiles(root, relativeRoot, paths);
        }

        AddExtensionReadmes(root, paths);

        var result = paths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
        if (result.Length == 0)
        {
            throw new InvalidOperationException("The repository Markdown scope is empty.");
        }

        return result;
    }

    internal static bool IsPublishedSiteSource(string sourcePath)
    {
        ArgumentNullException.ThrowIfNull(sourcePath);
        var normalized = NormalizeRelativePath(sourcePath);
        return (normalized.StartsWith("src/docusaurus/docs/", StringComparison.Ordinal)
                && normalized.EndsWith(MarkdownSuffix, StringComparison.Ordinal))
            || normalized is "docs/cli.md" or "docs/extensions.md" or "docs/development.md";
    }

    private static string ValidateRepositoryRoot(string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        if (!Path.IsPathFullyQualified(repositoryRoot))
        {
            throw new ArgumentException(
                "The repository root must be an explicit absolute path.",
                nameof(repositoryRoot));
        }

        string root;
        try
        {
            root = Path.GetFullPath(repositoryRoot);
        }
        catch (Exception exception) when (IsPathFailure(exception))
        {
            throw new ArgumentException(
                "The repository root is not a valid absolute path.",
                nameof(repositoryRoot),
                exception);
        }

        EnsureOrdinaryDirectory(root, ".");
        AddRequiredFile(root, SolutionFileName, new HashSet<string>(StringComparer.Ordinal));
        return root;
    }

    private static void AddRequiredFile(
        string repositoryRoot,
        string relativePath,
        ISet<string> paths)
    {
        var normalized = NormalizeRelativePath(relativePath);
        var absolutePath = ToAbsolutePath(repositoryRoot, normalized);
        EnsureOrdinaryFile(absolutePath, normalized);
        if (normalized.EndsWith(MarkdownSuffix, StringComparison.Ordinal))
        {
            paths.Add(normalized);
        }
    }

    private static void AddRecursiveFiles(
        string repositoryRoot,
        string relativeRoot,
        ISet<string> paths)
    {
        var normalizedRoot = NormalizeRelativePath(relativeRoot);
        var absoluteRoot = ToAbsolutePath(repositoryRoot, normalizedRoot);
        EnsureOrdinaryDirectory(absoluteRoot, normalizedRoot);
        EnumerateDirectory(normalizedRoot, absoluteRoot, paths);
    }

    private static void AddExtensionReadmes(
        string repositoryRoot,
        ISet<string> paths)
    {
        var extensionsRoot = ToAbsolutePath(repositoryRoot, ExtensionsRoot);
        EnsureOrdinaryDirectory(extensionsRoot, ExtensionsRoot);

        FileSystemInfo[] children;
        try
        {
            children = new DirectoryInfo(extensionsRoot)
                .EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                .OrderBy(entry => entry.Name, StringComparer.Ordinal)
                .ToArray();
        }
        catch (Exception exception) when (IsEnumerationFailure(exception))
        {
            throw EnumerationFailure(ExtensionsRoot, exception);
        }

        foreach (var child in children)
        {
            if ((ReadAttributes(child, CombineRelative(ExtensionsRoot, child.Name)) & FileAttributes.Directory) == 0)
            {
                continue;
            }

            var childRelativePath = CombineRelative(ExtensionsRoot, child.Name);
            EnsureOrdinaryDirectory(child.FullName, childRelativePath);
            var readme = CombineRelative(childRelativePath, "README.md");
            AddRequiredFile(repositoryRoot, readme, paths);
        }
    }

    private static void EnumerateDirectory(
        string relativeDirectory,
        string absoluteDirectory,
        ISet<string> paths)
    {
        FileSystemInfo[] entries;
        try
        {
            entries = new DirectoryInfo(absoluteDirectory)
                .EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                .OrderBy(entry => entry.Name, StringComparer.Ordinal)
                .ToArray();
        }
        catch (Exception exception) when (IsEnumerationFailure(exception))
        {
            throw EnumerationFailure(relativeDirectory, exception);
        }

        foreach (var entry in entries)
        {
            var relativePath = CombineRelative(relativeDirectory, entry.Name);
            var attributes = ReadAttributes(entry, relativePath);
            if ((attributes & FileAttributes.Directory) != 0)
            {
                if (IsExcludedSubtree(relativePath))
                {
                    continue;
                }

                if ((attributes & FileAttributes.ReparsePoint) != 0)
                {
                    throw new InvalidOperationException(
                        $"The Markdown scope cannot traverse the reparse-point directory {relativePath}.");
                }

                EnumerateDirectory(relativePath, entry.FullName, paths);
                continue;
            }

            if (!entry.Name.EndsWith(MarkdownSuffix, StringComparison.Ordinal))
            {
                continue;
            }

            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidOperationException(
                    $"The Markdown scope cannot read the reparse-point file {relativePath}.");
            }

            paths.Add(NormalizeRelativePath(relativePath));
        }
    }

    private static FileAttributes ReadAttributes(FileSystemInfo entry, string relativePath)
    {
        try
        {
            return entry.Attributes;
        }
        catch (Exception exception) when (IsEnumerationFailure(exception))
        {
            throw EnumerationFailure(relativePath, exception);
        }
    }

    private static void EnsureOrdinaryDirectory(string absolutePath, string relativePath)
    {
        var attributes = ReadAttributes(absolutePath, relativePath);
        if ((attributes & FileAttributes.Directory) == 0)
        {
            throw new InvalidOperationException($"The required Markdown root {relativePath} is not a directory.");
        }

        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidOperationException(
                $"The Markdown scope cannot traverse the reparse-point directory {relativePath}.");
        }
    }

    private static void EnsureOrdinaryFile(string absolutePath, string relativePath)
    {
        FileAttributes attributes;
        try
        {
            attributes = File.GetAttributes(absolutePath);
        }
        catch (FileNotFoundException exception)
        {
            throw new FileNotFoundException($"The required Markdown file {relativePath} is missing.", relativePath, exception);
        }
        catch (DirectoryNotFoundException exception)
        {
            throw new FileNotFoundException($"The required Markdown file {relativePath} is missing.", relativePath, exception);
        }
        catch (Exception exception) when (IsEnumerationFailure(exception))
        {
            throw EnumerationFailure(relativePath, exception);
        }

        if ((attributes & FileAttributes.Directory) != 0)
        {
            throw new InvalidOperationException($"The required Markdown file {relativePath} is a directory.");
        }

        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidOperationException(
                $"The Markdown scope cannot read the reparse-point file {relativePath}.");
        }
    }

    private static FileAttributes ReadAttributes(string absolutePath, string relativePath)
    {
        try
        {
            return File.GetAttributes(absolutePath);
        }
        catch (Exception exception) when (IsEnumerationFailure(exception))
        {
            throw EnumerationFailure(relativePath, exception);
        }
    }

    private static bool IsExcludedSubtree(string relativePath)
        => ExcludedSubtrees.Contains(relativePath);

    private static string ToAbsolutePath(string repositoryRoot, string relativePath)
        => Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static string CombineRelative(string parent, string child)
        => string.IsNullOrEmpty(parent) ? child : $"{parent}/{child}";

    private static string NormalizeRelativePath(string path)
        => path.Replace('\\', '/');

    private static InvalidOperationException EnumerationFailure(string relativePath, Exception exception)
        => new($"The Markdown scope could not enumerate {relativePath}.", exception);

    private static bool IsEnumerationFailure(Exception exception)
        => exception is ArgumentException
            or DirectoryNotFoundException
            or FileNotFoundException
            or IOException
            or NotSupportedException
            or PlatformNotSupportedException
            or UnauthorizedAccessException
            or System.Security.SecurityException;

    private static bool IsPathFailure(Exception exception)
        => exception is ArgumentException
            or NotSupportedException
            or PathTooLongException
            or PlatformNotSupportedException;
}
