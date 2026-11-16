using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.Stripping;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Operational;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Uninstall.Shared.Planning;

/// <summary>
/// Selects what uninstall removes before strip plans its edits. The uninstall
/// Interface and Behavior contracts define the targets, kept files and state.
/// </summary>
internal sealed class UninstallRemovalSource(PhysicalPathResolver resolver) : IStripRemovalSource
{
    private const string CurrentDirectoryPrefix = "./";
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public async ValueTask<StripRemovalSet> ReadAsync(StripRemovalInput input, CancellationToken cancellationToken)
    {
        var read = input.LayoutRead;
        var ownership = read.Ownership;
        if (ownership.State is WorkspaceOwnershipReadState.Invalid or WorkspaceOwnershipReadState.Unavailable)
        {
            return Stop(StripFindingCode.OwnershipUnavailable, ownership.Cause ?? "The ownership lock cannot be read, so uninstall cannot tell what Open Forge installed.");
        }

        if (ownership.Document.Libraries.FirstOrDefault() is { } library)
        {
            return Stop(StripFindingCode.LibraryAttached, $"Library '{library.Id}' is attached. Detach it before uninstalling.");
        }

        IReadOnlyList<FrameworkManagedTargetObservation> framework = ownership.Document.Framework is null ? [] :
            [.. (await new FrameworkLifecycleOperationalContributor(resolver, new FrameworkLifecycleTargetReader(resolver))
                .ReadDoctorAsync(read, cancellationToken).ConfigureAwait(false)).Targets.Select(target => target.Target)
                .Where(target => target.Kind != FrameworkManagedTargetKind.GeneratedRegion)
                .DistinctBy(target => target.Path, StringComparer.Ordinal)];
        IReadOnlyList<ExtensionTarget> extensions = ownership.Document.Extensions.IsEmpty ? [] :
            [.. (await new ExtensionLifecycleOperationalContributor(resolver, new ExtensionSourceReader(resolver), new ExtensionLifecycleTargetReader(resolver))
                .ReadDoctorAsync(read, cancellationToken).ConfigureAwait(false)).Targets
                .Select(target => new ExtensionTarget(target.Target.Path, target.Target.State, target.Target.Cause))];

        var managed = framework.Select(target => target.Path).Concat(extensions.Select(target => target.Path)).ToHashSet(StringComparer.Ordinal);
        var named = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in input.Request.RemoveEdited)
        {
            var path = Normalize(value);
            if (!managed.Contains(path))
            {
                return Stop(StripFindingCode.InvalidInput, $"'{value}' is not a file or section Open Forge manages here.");
            }

            named.Add(path);
        }

        var removals = new SortedDictionary<string, StripRemoval>(StringComparer.Ordinal);
        var kept = new List<StripKeptFile>();
        var findings = new List<StripFinding>();
        foreach (var target in framework.OrderBy(target => target.Path, StringComparer.Ordinal))
        {
            var stop = await AddAsync(input, target.Path, target.State, target.Cause, named, removals, kept, findings,
                target.Kind == FrameworkManagedTargetKind.ManagedRegion ? StripPlanReason.RootHost : StripPlanReason.Framework, cancellationToken).ConfigureAwait(false);
            if (stop is not null) return stop;
        }

        foreach (var target in extensions.OrderBy(target => target.Path, StringComparer.Ordinal))
        {
            if (removals.ContainsKey(target.Path) || kept.Any(file => file.Path == target.Path)) continue;
            var stop = await AddAsync(input, target.Path, target.State, target.Cause, named, removals, kept, findings,
                StripPlanReason.Extension, cancellationToken).ConfigureAwait(false);
            if (stop is not null) return stop;
        }

        foreach (var document in input.Documents)
        {
            if (!removals.ContainsKey(document.Path) && !ownership.Document.HasManagedPath(document.Path)
                && StripDocumentShapes.IsRouteScaffold(input.Layout, document))
            {
                removals.Add(document.Path, new StripRemoval { Path = document.Path, Reason = StripPlanReason.Scaffold, Before = document.Snapshot });
            }
        }

        foreach (var path in new[] { SettingsPath, OwnershipPath })
        {
            var snapshot = await input.Read(path, cancellationToken).ConfigureAwait(false);
            if (snapshot is null)
            {
                return Stop(StripFindingCode.TargetUnavailable, $"'{path}' cannot be read safely.");
            }

            if (snapshot.Kind == FileExpectationKind.File)
            {
                removals[path] = new StripRemoval { Path = path, Reason = StripPlanReason.State, Before = snapshot };
            }
        }

        return new StripRemovalSet { Removals = [.. removals.Values], Kept = [.. kept], Findings = [.. findings] };
    }

    private static string SettingsPath => WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.SettingsFileName;

    private static string OwnershipPath => WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.OwnershipFileName;

    private static async ValueTask<StripRemovalSet?> AddAsync(
        StripRemovalInput input,
        string path,
        OperationalTargetState state,
        string? cause,
        IReadOnlySet<string> named,
        SortedDictionary<string, StripRemoval> removals,
        List<StripKeptFile> kept,
        List<StripFinding> findings,
        StripPlanReason reason,
        CancellationToken token)
    {
        switch (state)
        {
            case OperationalTargetState.Missing:
                return null;
            case OperationalTargetState.Unavailable or OperationalTargetState.Blocked:
                return Stop(StripFindingCode.TargetUnavailable, $"'{path}' cannot be compared with what Open Forge installed. {cause}".TrimEnd(), path);
            case OperationalTargetState.Changed when !named.Contains(path):
                kept.Add(new StripKeptFile(path, StripKeptReason.Edited));
                findings.Add(new StripFinding
                {
                    Code = StripFindingCode.EditedKept,
                    Severity = StripFindingSeverity.Info,
                    Message = $"'{path}' was edited, so it stays. Name it with --remove-edited to remove it.",
                    Path = path,
                });
                return null;
        }

        var snapshot = await input.Read(path, token).ConfigureAwait(false);
        if (snapshot is not { Kind: FileExpectationKind.File } file)
        {
            return Stop(StripFindingCode.TargetUnavailable, $"'{path}' cannot be read safely.", path);
        }

        if (reason != StripPlanReason.RootHost)
        {
            removals[path] = new StripRemoval { Path = path, Reason = reason, Before = file };
            return null;
        }

        string source;
        try
        {
            source = StrictUtf8.GetString(file.Bytes.AsSpan());
        }
        catch (DecoderFallbackException)
        {
            return Stop(StripFindingCode.TargetUnavailable, $"'{path}' is not UTF-8 text.", path);
        }

        var section = StripDocumentShapes.RemoveManagedSection(source);
        switch (section.State)
        {
            case StripManagedSectionState.Absent:
                return null;
            case StripManagedSectionState.Invalid:
                return Stop(StripFindingCode.TargetUnavailable, $"'{path}' has an incomplete or ambiguous Open Forge section.", path);
            case StripManagedSectionState.Emptied:
                removals[path] = new StripRemoval { Path = path, Reason = reason, Before = file };
                return null;
            default:
                removals[path] = new StripRemoval { Path = path, Reason = reason, Before = file, After = section.Bytes };
                return null;
        }
    }

    private static string Normalize(string value)
    {
        var path = value.Replace('\\', '/');
        return path.StartsWith(CurrentDirectoryPrefix, StringComparison.Ordinal) ? path[CurrentDirectoryPrefix.Length..] : path;
    }

    private static StripRemovalSet Stop(StripFindingCode code, string message, string? path = null)
        => new()
        {
            Stop = new StripFinding { Code = code, Severity = StripFindingSeverity.Error, Message = message, Path = path },
        };

    private sealed record ExtensionTarget(string Path, OperationalTargetState State, string? Cause);
}
