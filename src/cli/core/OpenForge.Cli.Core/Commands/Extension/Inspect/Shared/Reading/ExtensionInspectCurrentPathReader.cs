using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using System.Security.Cryptography;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Reading;

/// <summary>
/// Captures current managed workspace paths once for an Inspect operation.
/// Result formation consumes this snapshot and never performs filesystem I/O.
/// </summary>
internal sealed class ExtensionInspectCurrentPathReader(PhysicalPathResolver physicalPathResolver)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<IReadOnlyList<ExtensionInspectCurrentPath>> ReadAsync(
        CliWorkspace workspace,
        IReadOnlyList<LifecycleInstalledPackage> packages,
        string subjectId,
        CancellationToken cancellationToken)
    {
        var selected = packages.FirstOrDefault(package => package.Id == subjectId);
        if (selected is null)
        {
            return [];
        }

        var packageById = packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var closure = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        pending.Push(selected.Id);
        while (pending.TryPop(out var id))
        {
            if (!closure.Add(id) || !packageById.TryGetValue(id, out var package))
            {
                continue;
            }

            foreach (var dependency in package.Dependencies.Order(StringComparer.Ordinal).Reverse())
            {
                pending.Push(dependency);
            }
        }

        var paths = packages
            .Where(package => closure.Contains(package.Id))
            .SelectMany(package => package.Paths)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var values = new List<ExtensionInspectCurrentPath>();
        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var lexical = Path.Combine(workspace.LexicalRoot, path.Replace('/', Path.DirectorySeparatorChar));
            var resolution = _physicalPathResolver.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                lexical);
            if (resolution.State is PhysicalPathState.Missing or PhysicalPathState.Dangling)
            {
                values.Add(Missing(path));
                continue;
            }

            if (resolution.State != PhysicalPathState.Contained)
            {
                values.Add(new ExtensionInspectCurrentPath
                {
                    Path = path,
                    State = ReadBoundaryState(resolution.State),
                    PhysicalIdentity = null,
                    ByteLength = null,
                    ExactSha256 = null,
                    Bytes = null,
                });
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var physical = resolution.GetContainedPhysicalPath();
                var bytes = await File.ReadAllBytesAsync(physical, cancellationToken).ConfigureAwait(false);
                values.Add(new ExtensionInspectCurrentPath
                {
                    Path = path,
                    State = ExtensionInspectCurrentPathState.Present,
                    PhysicalIdentity = physical,
                    ByteLength = bytes.LongLength,
                    ExactSha256 = Convert.ToHexStringLower(SHA256.HashData(bytes)),
                    Bytes = bytes,
                });
            }
            catch (FileNotFoundException)
            {
                values.Add(Missing(path));
            }
            catch (DirectoryNotFoundException)
            {
                values.Add(Missing(path));
            }
            catch (UnauthorizedAccessException)
            {
                values.Add(Unavailable(path));
            }
            catch (IOException)
            {
                values.Add(Unavailable(path));
            }
        }

        return values.OrderBy(path => path.Path, StringComparer.Ordinal).ToArray();
    }

    private static ExtensionInspectCurrentPath Missing(string path)
        => new()
        {
            Path = path,
            State = ExtensionInspectCurrentPathState.Missing,
            PhysicalIdentity = null,
            ByteLength = null,
            ExactSha256 = null,
            Bytes = null,
        };

    private static ExtensionInspectCurrentPath Unavailable(string path)
        => new()
        {
            Path = path,
            State = ExtensionInspectCurrentPathState.Unavailable,
            PhysicalIdentity = null,
            ByteLength = null,
            ExactSha256 = null,
            Bytes = null,
        };

    private static ExtensionInspectCurrentPathState ReadBoundaryState(PhysicalPathState state)
        => state is PhysicalPathState.External or PhysicalPathState.Cycle
            ? ExtensionInspectCurrentPathState.Blocked
            : ExtensionInspectCurrentPathState.Unavailable;
}
