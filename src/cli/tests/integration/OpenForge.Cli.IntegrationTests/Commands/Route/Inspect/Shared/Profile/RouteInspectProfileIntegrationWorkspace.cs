using System.Text;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;

internal sealed class RouteInspectProfileIntegrationWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _temporary;

    private RouteInspectProfileIntegrationWorkspace(TemporaryWorkspace temporary)
    {
        _temporary = temporary;
        _temporary.CreateDirectory(".agents");
    }

    internal string Path => _temporary.Path;

    internal CliWorkspace Workspace => new(
        Path,
        Path,
        CliWorkspaceSelectionMethod.ExplicitWorkspace);

    internal static RouteInspectProfileIntegrationWorkspace Create()
    {
        return new RouteInspectProfileIntegrationWorkspace(
            TemporaryWorkspace.Create("route-inspect-profile-integration"));
    }

    internal void Write(string relativePath, string contents)
    {
        _temporary.WriteText(relativePath, contents);
    }

    internal void Write(string relativePath, byte[] contents)
    {
        _temporary.WriteBytes(relativePath, contents);
    }

    internal string Absolute(string relativePath)
    {
        return _temporary.Combine(relativePath);
    }

    internal RouteInspectProfileIntegrationSnapshot Snapshot()
    {
        return new RouteInspectProfileIntegrationSnapshot(
            _temporary.SnapshotHashes(),
            SnapshotEntries());
    }

    internal ValueTask<RouteInspectResult> InspectAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        return RouteInspectOperationFactory.Create()(
            new RouteInspectRequest(
                Workspace,
                reference,
                allowInteractiveSourceSelection: false),
            cancellationToken);
    }

    internal void WriteLoader(
        IEnumerable<string> entries,
        string? axioms = null)
    {
        Write(".agents/loader.md", BuildLoader(entries, axioms));
    }

    internal void WriteEntrypoint(RouteInspectProfileIntegrationEntrypoint source)
    {
        Write(
            source.RelativePath,
            BuildSource(
                source.Description,
                source.Tags,
                source.Axioms,
                source.Entries));
    }

    internal void WriteRoutedMarkdown(
        string relativePath,
        string description,
        IEnumerable<string> tags,
        string body)
    {
        Write(
            relativePath,
            OpenForgeMetadata(description, tags.ToArray())
            + body);
    }

    internal static string OpenForgeMetadata(
        string description,
        params string[] tags)
    {
        return OpenForgeDocumentSeed.Metadata(
            description: description,
            tags: tags,
            body: string.Empty);
    }

    internal static string Entry(
        string label,
        string destination,
        params string[] tags)
    {
        var renderedTags = string.Join(
            " ",
            tags.Select(tag => tag.StartsWith('#') ? tag : $"#{tag}"));
        return $"- [{label}]({destination}) - {renderedTags}";
    }

    internal static string BuildSource(
        string description,
        IEnumerable<string> tags,
        string? axioms,
        IEnumerable<string> entries)
    {
        return OpenForgeMetadata(description, tags.ToArray())
            + BuildBody(description, axioms, entries);
    }

    internal static string BuildBody(
        string title,
        string? axioms,
        IEnumerable<string> entries)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {title}");
        builder.AppendLine();
        if (axioms is not null)
        {
            builder.AppendLine("## Axioms");
            builder.AppendLine();
            builder.AppendLine(axioms);
            builder.AppendLine();
        }

        builder.AppendLine("## Entries");
        builder.AppendLine();
        builder.AppendLine("<!-- open-forge:generated-index:start -->");
        builder.AppendLine();
        var materializedEntries = entries.ToArray();
        if (materializedEntries.Length == 0)
        {
            builder.AppendLine("- none - No entries - #Empty");
        }
        else
        {
            foreach (var entry in materializedEntries)
            {
                builder.AppendLine(entry);
            }
        }

        builder.AppendLine();
        builder.AppendLine("<!-- open-forge:generated-index:end -->");
        return builder.ToString();
    }

    internal static string BuildLoader(
        IEnumerable<string> entries,
        string? axioms)
    {
        return BuildLoaderBody("Open Forge Loader", axioms, entries);
    }

    internal static string BuildLoaderBody(
        string title,
        string? axioms,
        IEnumerable<string> entries)
    {
        return BuildBody(title, axioms, entries);
    }

    private IReadOnlySet<string> SnapshotEntries()
    {
        var entries = new SortedSet<string>(StringComparer.Ordinal);
        CollectEntries(new DirectoryInfo(Path), entries);
        return entries;
    }

    private void CollectEntries(
        DirectoryInfo directory,
        ISet<string> entries)
    {
        foreach (var entry in directory
                     .EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                     .OrderBy(entry => entry.Name, StringComparer.Ordinal))
        {
            if (directory.FullName == Path
                && entry.Name == ".open-forge-test-workspace-owner")
            {
                continue;
            }

            if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidOperationException(
                    "Profile integration snapshots do not support symbolic links.");
            }

            var relativePath = System.IO.Path.GetRelativePath(Path, entry.FullName)
                .Replace('\\', '/');
            var isDirectory = (entry.Attributes & FileAttributes.Directory) != 0;
            entries.Add($"{(isDirectory ? 'D' : 'F')}:{relativePath}");
            if (isDirectory)
            {
                CollectEntries((DirectoryInfo)entry, entries);
            }
        }
    }

    public void Dispose()
    {
        _temporary.Dispose();
    }
}

internal sealed class RouteInspectProfileIntegrationEntrypoint
{
    internal required string RelativePath { get; init; }

    internal required string Description { get; init; }

    internal required IReadOnlyList<string> Tags { get; init; }

    internal string? Axioms { get; init; }

    internal IReadOnlyList<string> Entries { get; init; } = [];
}

internal sealed record RouteInspectProfileIntegrationSnapshot(
    IReadOnlyDictionary<string, string> FileHashes,
    IReadOnlySet<string> Entries);

internal static class RouteInspectProfileIntegrationAssertions
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal static void AssertMeasurement(
        RouteInspectFact<RouteInspectMeasurement> fact,
        RouteInspectProfileIntegrationWorkspace workspace,
        params string[] relativePaths)
    {
        Assert.Equal(RouteInspectFactState.Value, fact.State);
        var actual = Assert.IsType<RouteInspectMeasurement>(fact.Value);
        var physicalPaths = relativePaths
            .Select(workspace.Absolute)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var byteArrays = physicalPaths.Select(File.ReadAllBytes).ToArray();
        var unicodeScalarCount = byteArrays
            .Select(bytes => StrictUtf8.GetString(bytes))
            .Sum(text => text.EnumerateRunes().LongCount());
        var utf8ByteCount = byteArrays.Sum(bytes => (long)bytes.Length);
        var expectedTokens = unicodeScalarCount / 4
            + (unicodeScalarCount % 4 == 0 ? 0 : 1);

        Assert.Equal(physicalPaths.Length, actual.PhysicalFileCount);
        Assert.Equal(unicodeScalarCount, actual.UnicodeScalarCount);
        Assert.Equal(utf8ByteCount, actual.Utf8ByteCount);
        Assert.Equal(expectedTokens, actual.EstimatedTokens);
    }

    internal static void AssertUnavailable<T>(RouteInspectFact<T> fact)
    {
        Assert.Equal(RouteInspectFactState.Unavailable, fact.State);
        if (default(T) is null)
        {
            Assert.Null(fact.Value);
        }

        Assert.False(string.IsNullOrWhiteSpace(fact.Reason));
    }

    internal static void AssertNotApplicable<T>(RouteInspectFact<T> fact)
    {
        Assert.Equal(RouteInspectFactState.NotApplicable, fact.State);
        if (default(T) is null)
        {
            Assert.Null(fact.Value);
        }

        Assert.False(string.IsNullOrWhiteSpace(fact.Reason));
    }

    internal static void AssertSemanticConditionAndNext(
        RouteInspectResult result,
        RouteInspectConditionCode code)
    {
        var condition = Assert.IsType<RouteInspectCondition>(
            result.Conditions.FirstOrDefault(candidate => candidate.Code == code));
        Assert.False(string.IsNullOrWhiteSpace(condition.Subject));
        Assert.False(string.IsNullOrWhiteSpace(condition.Message));
        var next = Assert.IsType<CliNextAction>(result.Next);
        Assert.False(string.IsNullOrWhiteSpace(next.Command));
        Assert.False(string.IsNullOrWhiteSpace(next.Reason));
    }

    internal static void AssertNoWriteOrInspectionState(
        RouteInspectProfileIntegrationSnapshot before,
        RouteInspectProfileIntegrationSnapshot after)
    {
        Assert.Equal(before.FileHashes, after.FileHashes);
        Assert.Equal(
            before.Entries.OrderBy(entry => entry, StringComparer.Ordinal),
            after.Entries.OrderBy(entry => entry, StringComparer.Ordinal));
        var addedPaths = after.Entries
            .Except(before.Entries, StringComparer.Ordinal)
            .ToArray();
        Assert.DoesNotContain(
            addedPaths,
            path => path.Contains("cache", StringComparison.OrdinalIgnoreCase)
                || path.Contains("receipt", StringComparison.OrdinalIgnoreCase)
                || path.Contains("recovery", StringComparison.OrdinalIgnoreCase)
                || path.Contains("inspection", StringComparison.OrdinalIgnoreCase)
                || path.Contains("index", StringComparison.OrdinalIgnoreCase));
    }
}
