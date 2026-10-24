using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Observation;

internal sealed class WorkspaceLayoutReader(PhysicalPathResolver paths)
{
    internal ValueTask<WorkspaceLayoutRead> ReadAsync(CliWorkspace workspace, CancellationToken cancellationToken)
        => new Observation(paths, workspace).ReadAsync(cancellationToken);

    private sealed class Observation(PhysicalPathResolver paths, CliWorkspace workspace)
    {
        private static readonly UTF8Encoding StrictUtf8 = new(false, true);
        private readonly List<WorkspaceLayoutIssue> _issues = [];
        private readonly Dictionary<string, FileExpectation> _expectations = new(StringComparer.Ordinal);
        private readonly HashSet<string> _directories = new(StringComparer.Ordinal);
        private readonly FileExpectationValidator _validator = new(paths);

        internal async ValueTask<WorkspaceLayoutRead> ReadAsync(CancellationToken cancellationToken)
        {
            var settings = await WorkspaceSettingsReader.ReadAsync(paths, workspace, cancellationToken).ConfigureAwait(false);
            var ownership = await WorkspaceOwnershipReader.ReadAsync(paths, workspace, cancellationToken).ConfigureAwait(false);
            Capture(settings.Snapshot);
            Capture(ownership.Snapshot);
            ObserveDirectory(WorkspaceLayout.DefaultMainRoot, cancellationToken);

            if (settings.State == WorkspaceSettingsReadState.Invalid)
                Issue(WorkspaceLayoutIssueCode.InvalidSetting, WorkspaceSettingsDefinitions.RelativePath, settings.Cause ?? "Settings are invalid.");
            if (settings.State == WorkspaceSettingsReadState.Unavailable)
                Issue(WorkspaceLayoutIssueCode.ObservationUnavailable, WorkspaceSettingsDefinitions.RelativePath, settings.Cause ?? "Settings are unavailable.");
            if (ownership.State is WorkspaceOwnershipReadState.Invalid or WorkspaceOwnershipReadState.Unavailable)
                Issue(WorkspaceLayoutIssueCode.ObservationUnavailable, WorkspaceOwnershipDefinitions.RelativePath, ownership.Cause ?? "Ownership is unavailable.");

            if (settings.State is WorkspaceSettingsReadState.Invalid or WorkspaceSettingsReadState.Unavailable)
                return Result(settings.State == WorkspaceSettingsReadState.Invalid ? WorkspaceLayoutReadState.Invalid : WorkspaceLayoutReadState.Unavailable, null);

            var layout = WorkspaceLayout.Create(workspace, settings.Document.MainRoot);
            ObserveDirectory(layout.MainRoot, cancellationToken);
            ObserveDirectory(WorkspaceLayout.NativeSkillsPath, cancellationToken);
            if (!layout.IsDefault && ObserveDirectory(layout.MainRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName, cancellationToken) is { Exists: true })
                Issue(WorkspaceLayoutIssueCode.UnsafeRoot, layout.MainRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName, "A custom root cannot contain a competing Skills category.", WorkspaceLayout.NativeSkillsPath);

            var loader = await ReadFileAsync(layout.LoaderPath, cancellationToken).ConfigureAwait(false);
            var defaultLoader = layout.IsDefault ? loader : await ReadFileAsync(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, cancellationToken).ConfigureAwait(false);
            var positive = loader?.Kind == FileExpectationKind.File || defaultLoader?.Kind == FileExpectationKind.File
                || ownership.Document.Framework is not null || !ownership.Document.Extensions.IsEmpty || !ownership.Document.Libraries.IsEmpty;

            foreach (var host in new[] { WorkspaceLayoutNames.AgentHostFileName, WorkspaceLayoutNames.ClaudeHostFileName })
            {
                var snapshot = await ReadFileAsync(host, cancellationToken).ConfigureAwait(false);
                if (snapshot?.Kind == FileExpectationKind.File) positive |= InspectHost(host, snapshot, layout);
            }

            if (!layout.IsDefault && defaultLoader?.Kind == FileExpectationKind.File)
                Issue(WorkspaceLayoutIssueCode.ConflictingLoader, WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, "The default loader conflicts with the selected main root.", layout.LoaderPath);
            if (ownership.Document.Framework is { } framework)
            {
                var recordedLoaders = framework.Paths.Concat(framework.Regions.Select(region => region.Path))
                    .Where(path => path.EndsWith("/" + WorkspaceLayoutNames.LoaderFileName, StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.Ordinal);
                foreach (var recorded in recordedLoaders.Where(path => path != layout.LoaderPath))
                    Issue(WorkspaceLayoutIssueCode.OwnershipMismatch, recorded, "Recorded Framework ownership names another loader.", layout.LoaderPath);
            }
            if (positive && loader?.Kind == FileExpectationKind.Missing)
                Issue(WorkspaceLayoutIssueCode.MissingLoader, layout.LoaderPath, "The selected loader is missing despite installation evidence.");

            foreach (var expectation in _expectations.Values)
            {
                var check = await _validator.ValidateAsync(workspace, expectation, cancellationToken).ConfigureAwait(false);
                if (check.State == FileExpectationValidationState.Cancelled) cancellationToken.ThrowIfCancellationRequested();
                if (check.State != FileExpectationValidationState.Matched)
                    Issue(WorkspaceLayoutIssueCode.ObservationUnavailable, Relative(expectation.LogicalPath), "The layout changed while it was being observed.");
            }

            if (_issues.Any(issue => issue.Code == WorkspaceLayoutIssueCode.ObservationUnavailable))
                return Result(WorkspaceLayoutReadState.Unavailable, null);
            if (_issues.Any(issue => issue.Code == WorkspaceLayoutIssueCode.UnsafeRoot))
                return Result(WorkspaceLayoutReadState.Invalid, null);
            if (_issues.Count != 0) return Result(WorkspaceLayoutReadState.Mismatch, layout);
            return Result(positive ? WorkspaceLayoutReadState.Ready : WorkspaceLayoutReadState.Uninstalled, layout);

            WorkspaceLayoutRead Result(WorkspaceLayoutReadState state, WorkspaceLayout? selected)
                => new(state, settings, ownership, selected, [.. _issues], [.. _expectations.Values]);
        }

        private FileExpectation? ObserveDirectory(string relative, CancellationToken cancellationToken)
        {
            if (!_directories.Add(relative)) return _expectations.GetValueOrDefault(Logical(relative));
            var slash = relative.LastIndexOf('/');
            if (slash >= 0 && ObserveDirectory(relative[..slash], cancellationToken) is null) return null;
            var logical = Logical(relative);
            var leaf = NoFollowLeafObserver.Observe(paths, workspace, logical, cancellationToken);
            FileExpectation expectation;
            if (leaf.State == NoFollowLeafState.Missing) expectation = FileExpectation.Missing(logical);
            else if (leaf.State == NoFollowLeafState.Directory)
            {
                var resolved = paths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, logical);
                if (resolved.State != PhysicalPathState.Contained)
                {
                    Issue(WorkspaceLayoutIssueCode.UnsafeRoot, relative, "The content ancestry cannot be resolved safely.");
                    return null;
                }
                var physical = resolved.GetContainedPhysicalPath();
                try { _ = Directory.EnumerateFileSystemEntries(physical).Take(1).Any(); }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    Issue(WorkspaceLayoutIssueCode.ObservationUnavailable, relative, "The content directory is inaccessible.");
                    return null;
                }
                expectation = FileExpectation.Directory(logical, physical);
            }
            else
            {
                var code = leaf.State is NoFollowLeafState.Inaccessible or NoFollowLeafState.Unknown
                    ? WorkspaceLayoutIssueCode.ObservationUnavailable : WorkspaceLayoutIssueCode.UnsafeRoot;
                Issue(code, relative, "Content ancestry must be missing or an ordinary directory.");
                return null;
            }
            _expectations[logical] = expectation;
            return expectation;
        }

        private async ValueTask<FileStateSnapshot?> ReadFileAsync(string relative, CancellationToken cancellationToken)
        {
            var slash = relative.LastIndexOf('/');
            if (slash >= 0 && ObserveDirectory(relative[..slash], cancellationToken) is null) return null;
            var logical = Logical(relative);
            var leaf = NoFollowLeafObserver.Observe(paths, workspace, logical, cancellationToken);
            if (leaf.State == NoFollowLeafState.Missing)
            {
                var missing = FileStateSnapshot.Missing(logical);
                Capture(missing);
                return missing;
            }
            if (leaf.State != NoFollowLeafState.OrdinaryFile)
            {
                Issue(WorkspaceLayoutIssueCode.ObservationUnavailable, relative, "The layout file is not an accessible ordinary file.");
                return null;
            }
            var resolution = paths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, logical);
            if (resolution.State != PhysicalPathState.Contained)
            {
                Issue(WorkspaceLayoutIssueCode.ObservationUnavailable, relative, "The layout file cannot be resolved safely.");
                return null;
            }
            try
            {
                var physical = resolution.GetContainedPhysicalPath();
                var bytes = await File.ReadAllBytesAsync(physical, cancellationToken).ConfigureAwait(false);
                var snapshot = FileStateSnapshot.File(logical, physical, bytes);
                Capture(snapshot);
                return snapshot;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Issue(WorkspaceLayoutIssueCode.ObservationUnavailable, relative, "The layout file could not be read.");
                return null;
            }
        }

        private bool InspectHost(string host, FileStateSnapshot snapshot, WorkspaceLayout layout)
        {
            string text;
            try { text = StrictUtf8.GetString(snapshot.Bytes.AsSpan()); }
            catch (DecoderFallbackException)
            {
                Issue(WorkspaceLayoutIssueCode.ObservationUnavailable, host, "The host file is not valid UTF-8.");
                return false;
            }
            var facts = new MarkdownDocumentParser().Parse(text);
            if (facts.ManagedHost.State == MarkdownManagedHostState.Absent) return false;
            if (facts.ManagedHost.Span is not { } region)
            {
                Issue(WorkspaceLayoutIssueCode.PointerMismatch, host, "The managed host boundary is invalid.");
                return true;
            }
            var candidates = facts.OpaqueSpans.Where(value => value.IsCode && value.Span.Start >= region.Start && value.Span.End <= region.End)
                .Select(value => text[value.Span.Start..value.Span.End].Trim('`'))
                .Concat(facts.Links.Where(value => value.Span.Start >= region.Start && value.Span.End <= region.End).Select(value => value.RawDestination))
                .Concat(facts.VisibleText.Where(value => value.Span.Start >= region.Start && value.Span.End <= region.End)
                    .SelectMany(value => text[value.Span.Start..value.Span.End].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
                    .Select(value => value.Trim())
                    .Where(value => value.StartsWith('@')).Select(value => value[1..]));
            foreach (var pointer in candidates.Where(value => value.EndsWith("/" + WorkspaceLayoutNames.LoaderFileName, StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.Ordinal))
                if (pointer != layout.LoaderPath)
                    Issue(WorkspaceLayoutIssueCode.PointerMismatch, host, "The managed host points to another loader.", pointer, layout.LoaderPath);
            return true;
        }

        private void Capture(FileStateSnapshot? snapshot)
        {
            if (snapshot is not null) _expectations[snapshot.LogicalPath] = snapshot.Expectation;
        }

        private void Issue(WorkspaceLayoutIssueCode code, string path, string cause, params string[] related)
            => _issues.Add(new(code, path, cause, [.. related]));

        private string Logical(string relative) => Path.Combine(workspace.LexicalRoot, relative.Replace('/', Path.DirectorySeparatorChar));
        private string Relative(string logical) => Path.GetRelativePath(workspace.LexicalRoot, logical).Replace(Path.DirectorySeparatorChar, '/');
    }
}
