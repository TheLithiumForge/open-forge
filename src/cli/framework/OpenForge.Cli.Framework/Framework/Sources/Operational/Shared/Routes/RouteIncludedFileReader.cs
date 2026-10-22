using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

// This reader stays local to operational reporting until the included-source reader is shared.
internal sealed class RouteIncludedFileReader(CliWorkspace workspace)
{
    private readonly PhysicalPathResolver _resolver = new();
    private readonly Dictionary<string, RouteIncludedFileObservation> _files = new(StringComparer.Ordinal);
    private readonly Dictionary<string, RouteContextSource> _total = new(PhysicalIdentityTracker.PathComparer);
    private readonly Dictionary<string, RouteContextSource> _startup = new(PhysicalIdentityTracker.PathComparer);
    private readonly Dictionary<string, RouteContextSource> _continuity = new(PhysicalIdentityTracker.PathComparer);
    private readonly HashSet<string> _pending = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _physicalPaths = new(StringComparer.Ordinal);
    private IReadOnlySet<string> _registered = new HashSet<string>();
    private IReadOnlySet<string> _loaderPaths = new HashSet<string>();
    private bool _startupComplete = true;
    private bool _totalComplete = true;

    internal async ValueTask<RouteIncludedFilesObservation?> ReadAsync(
        RouteSourceInspection inspection,
        CancellationToken cancellationToken)
    {
        var settings = await WorkspaceSettingsReader.ReadAsync(_resolver, workspace, cancellationToken).ConfigureAwait(false);
        _registered = settings.Document.Roots.ToHashSet(StringComparer.Ordinal);
        var loader = inspection.Sources.SingleOrDefault(source => source.Source.Base.Form == SourceDocumentForm.Loader);
        var entries = loader?.GeneratedEntries.Entries ?? [];
        var rows = entries.Select(entry => (Entry: entry, Path: SourceGeneratedDestinationResolver.ResolveLinked(
                SourceLogicalPath.LoaderPath, isLoader: true, entry.Destination)))
            .Where(row => row.Path is not null).ToArray();
        if (_registered.Count == 0 && rows.Length == 0)
            return null;
        _loaderPaths = rows.Select(row => row.Path ?? string.Empty).ToHashSet(StringComparer.Ordinal);

        foreach (var path in _registered)
            await ReadFileAsync(path, cancellationToken).ConfigureAwait(false);
        foreach (var row in rows)
            await VisitAsync(SourceLogicalPath.LoaderPath, row.Entry, requiredParent: true, continuityParent: false,
                conditions: [], ancestors: [], cancellationToken).ConfigureAwait(false);

        return new RouteIncludedFilesObservation
        {
            Files = _files.Values.OrderBy(file => file.Path, StringComparer.Ordinal).ToArray(),
            HandWrittenRows = rows.Count(row => !_registered.Contains(row.Path ?? string.Empty)),
            Startup = RouteContextMeasurementReader.Measure(_startup.Values, _startupComplete),
            Total = RouteContextMeasurementReader.Measure(_total.Values, _totalComplete),
            Continuity = RouteContextMeasurementReader.Measure(_continuity.Values, _startupComplete),
            ContinuitySources = RouteContextMeasurementReader.Contributions(new RouteContextClosure
            {
                Startup = [],
                Continuity = _continuity.Values.GroupBy(source => source.Path, StringComparer.Ordinal)
                    .Select(group => group.First() with { Layers = group.SelectMany(source => source.Layers).ToArray() }).ToArray(),
                IsComplete = _startupComplete,
            }),
            Pending = _pending.Order(StringComparer.Ordinal).ToArray(),
        };
    }

    private async ValueTask VisitAsync(
        string parent,
        SourceGeneratedEntry entry,
        bool requiredParent,
        bool continuityParent,
        IReadOnlyList<SourceApplyToCondition> conditions,
        IReadOnlyList<string> ancestors,
        CancellationToken cancellationToken)
    {
        var path = SourceGeneratedDestinationResolver.ResolveLinked(parent, parent == SourceLogicalPath.LoaderPath, entry.Destination);
        if (path is null || ancestors.Contains(path, StringComparer.Ordinal))
            return;
        var chain = conditions.Append(new SourceApplyToCondition(path, ApplyToMetadataFacts.FromEntryLine(entry.ApplyTo))).ToArray();
        var applicability = SourceApplicabilityEvaluator.Evaluate(chain, []);
        var required = requiredParent && (entry.HasTag("LoadNow") || entry.HasTag("KeepInMind"));
        if (required && applicability.State == SourceApplicabilityState.Pending)
            _pending.Add(path);
        var active = required && applicability.State == SourceApplicabilityState.Unconditioned;
        var continuity = active && (continuityParent || entry.HasTag("KeepInMind"));
        var (source, physicalPath) = await ReadFileAsync(path, cancellationToken).ConfigureAwait(false);
        if (source is null || physicalPath is null)
        {
            _totalComplete = false;
            _startupComplete &= !active;
            return;
        }
        AddLayers(_total, source);
        if (source.GeneratedEntries.State == SourceGeneratedEntriesState.Unavailable)
        {
            _totalComplete = false;
            _startupComplete &= !active;
        }
        if (active)
            AddLayers(_startup, source);
        if (continuity)
            AddLayers(_continuity, source);
        foreach (var child in source.GeneratedEntries.Entries)
            await VisitAsync(path, child, requiredParent: active, continuityParent: continuity,
                conditions: chain, ancestors: [.. ancestors, path], cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<(RouteContextSource? Source, string? PhysicalPath)> ReadFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resolution = _resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot,
            Path.Combine(workspace.LexicalRoot, path.Replace('/', Path.DirectorySeparatorChar)));
        var physical = resolution.State == PhysicalPathState.Contained ? resolution.GetContainedPhysicalPath() : null;
        var exists = physical is not null && File.Exists(physical);
        string? rejection = null;
        if (resolution.State != PhysicalPathState.Missing && !exists)
            rejection = "The included path is not a contained file.";
        else if (physical is not null && IsInsideAgents(physical))
            rejection = "The included file is inside .agents/.";
        else if (physical is not null && !WorkspaceSettingsCodec.IsValidRootPath(
                     Path.GetRelativePath(workspace.PhysicalRoot, physical).Replace('\\', '/')))
            rejection = "The included file resolves to a path that cannot be registered.";
        string? text = null;
        if (exists && rejection is null && physical is not null)
        {
            var read = await StrictUtf8FileReader.ReadAsync(physical, path, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            text = read.Value;
            if (text is null || text.Contains('\0'))
                rejection = "The included file is not readable UTF-8 text.";
        }
        _files[path] = new RouteIncludedFileObservation(
            Path: path,
            Registered: _registered.Contains(path),
            Listed: _loaderPaths.Contains(path),
            FileExists: exists,
            Missing: resolution.State is PhysicalPathState.Missing or PhysicalPathState.Dangling,
            Rejection: rejection);
        if (text is null || rejection is not null)
            return (null, physical);
        _physicalPaths[path] = physical ?? throw new InvalidOperationException("A readable include requires a physical path.");
        var layers = new List<RouteContextLayer> { new(path, text) };
        if (SourceGeneratedDestinationResolver.IsMarkdown(path))
        {
            var overwrite = path[..^3] + ".overwrite.md";
            var companion = _resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot,
                Path.Combine(workspace.LexicalRoot, overwrite.Replace('/', Path.DirectorySeparatorChar)));
            if (companion.State == PhysicalPathState.Contained && File.Exists(companion.GetContainedPhysicalPath())
                && !IsInsideAgents(companion.GetContainedPhysicalPath()))
            {
                var companionPath = companion.GetContainedPhysicalPath();
                var read = await StrictUtf8FileReader.ReadAsync(companionPath, overwrite, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                _physicalPaths[overwrite] = companionPath;
                layers.Add(new RouteContextLayer(overwrite, read.Value));
            }
        }
        var entries = SourceGeneratedDestinationResolver.IsMarkdown(path)
            ? SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(text))
            : SourceGeneratedEntriesFacts.Absent;
        return (new RouteContextSource
        {
            Id = $"./{path}",
            Path = path,
            Form = SourceDocumentForm.Markdown,
            RouteState = SourceRouteState.Routed,
            ParentPath = null,
            Metadata = SourceAuthoredMetadataFacts.Complete("Included file", []),
            GeneratedEntries = entries,
            Layers = layers,
        }, physical);
    }

    private void AddLayers(Dictionary<string, RouteContextSource> sources, RouteContextSource source)
    {
        foreach (var layer in source.Layers)
            sources.TryAdd(_physicalPaths[layer.Path], source with { Layers = [layer] });
    }

    private bool IsInsideAgents(string physicalPath)
        => Path.GetRelativePath(workspace.PhysicalRoot, physicalPath).Replace('\\', '/')
            .StartsWith(SourceLogicalPath.AgentsRoot + "/", StringComparison.OrdinalIgnoreCase);
}
