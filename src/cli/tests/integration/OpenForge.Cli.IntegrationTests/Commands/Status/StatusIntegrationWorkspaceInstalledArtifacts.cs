namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal sealed partial class StatusIntegrationWorkspace
{
    private readonly HashSet<string> _postInstallAgentFiles = new(StringComparer.Ordinal);

    internal void WritePostInstallAgentText(string relativePath, string contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        var path = PreparePostInstallAgentFile(relativePath);
        File.WriteAllText(path, contents);
        _postInstallAgentFiles.Add(path);
    }

    internal void WritePostInstallAgentBytes(string relativePath, byte[] contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        var path = PreparePostInstallAgentFile(relativePath);
        File.WriteAllBytes(path, contents);
        _postInstallAgentFiles.Add(path);
    }

    internal void OverwriteInstalledText(string relativePath, string contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        File.WriteAllText(ReadInstalledOrdinaryFile(relativePath), contents);
    }

    internal void OverwriteInstalledBytes(string relativePath, byte[] contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        File.WriteAllBytes(ReadInstalledOrdinaryFile(relativePath), contents);
    }

    internal void DeleteInstalledTarget(string relativePath)
    {
        var path = ReadInstalledOrdinaryFile(relativePath);
        File.Delete(path);
        _transformedTargets.Add(path);
    }

    internal void ReplaceInstalledTargetWithDirectory(string relativePath)
    {
        var path = ReadInstalledOrdinaryFile(relativePath);
        File.Delete(path);
        Directory.CreateDirectory(path);
        _transformedTargets.Add(path);
    }

    internal void ReplaceInstalledTargetWithLink(string relativePath, string targetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        var path = ReadInstalledOrdinaryFile(relativePath);
        File.Delete(path);
        File.CreateSymbolicLink(path, targetPath);
        _transformedTargets.Add(path);
    }

    private string PreparePostInstallAgentFile(string relativePath)
    {
        EnsureInstalledArtifacts();
        var path = Combine(relativePath);
        var parent = System.IO.Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The Status post-install file requires a parent directory.");
        if (!PathEquals(parent, Combine(".agents")))
        {
            throw new InvalidOperationException("Status post-install files must be direct children of the installed .agents directory.");
        }

        EnsureOrdinaryDirectory(parent);
        if (File.Exists(path) || Directory.Exists(path))
        {
            throw new InvalidOperationException("The Status post-install file must not already exist.");
        }

        return path;
    }

    private string ReadInstalledOrdinaryFile(string relativePath)
    {
        EnsureInstalledArtifacts();
        if (!StatusInstalledWorkspaceArtifacts.Contains(relativePath))
        {
            throw new InvalidOperationException("The Status mutation target is not an exact installed artifact.");
        }

        var path = Combine(relativePath);
        if (!File.Exists(path))
        {
            throw new InvalidOperationException("The Status installed mutation target does not exist.");
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException("The Status installed mutation target is not an ordinary file.");
        }

        return path;
    }

    private void DeletePostInstallAgentFiles()
    {
        foreach (var path in _postInstallAgentFiles)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            var attributes = File.GetAttributes(path);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException("The Status post-install cleanup target is not an ordinary file.");
            }

            File.Delete(path);
        }
    }

    private void EnsureInstalledArtifacts()
    {
        if (!_hasInstallArtifacts)
        {
            throw new InvalidOperationException("The Status workspace has no installed artifacts.");
        }
    }

    private static void EnsureOrdinaryDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new InvalidOperationException("The Status installed .agents directory does not exist.");
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            != FileAttributes.Directory)
        {
            throw new InvalidOperationException("The Status installed .agents path is not an ordinary directory.");
        }
    }

    private static bool PathEquals(string first, string second)
        => string.Equals(
            first,
            second,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
}
