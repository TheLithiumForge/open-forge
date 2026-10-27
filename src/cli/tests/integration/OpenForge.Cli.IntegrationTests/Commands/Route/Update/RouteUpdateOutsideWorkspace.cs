using OpenForge.Cli.Core.Framework.Sources.Loading;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

internal sealed class RouteUpdateOutsideWorkspace : IDisposable
{
    internal const string CustomRoot = "knowledge/main";
    internal const string DefaultDirectory = "docs/team";
    internal const string Tag = "Fixture";
    internal const string Before = "Before outside";
    internal const string After = "After outside";
    internal const string AuthoredDocument = "\uFEFF---\r\nforeign: {retain: true}\r\nopen-forge:\r\n  description: Before outside\r\n  responsibility: Keep the authoring\r\n  tags: [Fixture, Before]\r\n  applyTo: [\"src/**\"]\r\n  unknown: keep-this # authored comment\r\n---\r\n\r\n# Independent authoring\r\n\r\nKeep every body byte, including π and {tokens}.\r\n";
    internal readonly TemporaryWorkspace Files = TemporaryWorkspace.Create("route-update-outside");
    private readonly WorkspaceLockTestStore _locks = WorkspaceLockTestStore.Create("route-update-outside-locks");
    private readonly List<string> _recoveryPaths = [];
    internal CliWorkspace Workspace { get; }
    internal WorkspaceLayout Layout { get; }
    internal string DirectoryPath { get; }
    internal string Anchor { get; }
    internal string ParentDirectory => DirectoryPath + "/design";
    internal string Parent => WorkspaceRoutePath.CanonicalEntrypointPath(ParentDirectory);
    internal string Target => ParentDirectory + "/note" + SourceFormClassifier.MarkdownExtension;
    internal string Overwrite => SourceOverwritePath.ReadAdjacentPath(Target);
    internal string Template => ParentDirectory + "/template" + SourceFormClassifier.MarkdownExtension;
    internal string PrimarySource => Layout.SourcePath("ordinary/note" + SourceFormClassifier.MarkdownExtension);

    internal RouteUpdateOutsideWorkspace(string root, bool listed = true, string directory = DefaultDirectory)
    {
        Workspace = new(Files.Path, Files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Layout = WorkspaceLayout.Create(Workspace, root).WithOutsideRoutes([directory]);
        DirectoryPath = directory;
        Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(directory);
        _locks.Track(Workspace);
        Files.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings([directory]));
        Files.WriteText(Anchor, Entrypoint());
        Files.WriteText(Parent, Entrypoint());
        Files.WriteText(Target, AuthoredDocument);
        var primaryAnchor = WorkspaceRoutePath.CanonicalEntrypointPath(Layout.SourcePath("ordinary"));
        Files.WriteText(primaryAnchor, Entrypoint());
        Files.WriteText(PrimarySource, AuthoredDocument);
        var destination = Path.GetRelativePath(root, Anchor).Replace(Path.DirectorySeparatorChar, '/');
        var rows = listed ? $"- [Independent outside policy]({destination}) - #{SourceLoadingClosureResolver.LoadNowTag} #{Tag} - applies to `policy/**`\r\n" : string.Empty;
        rows += $"- [Primary policy](ordinary/_ordinary.md) - #{SourceLoadingClosureResolver.LoadNowTag} #Fixture";
        Files.WriteText(Layout.LoaderPath, "\uFEFF" + OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Prefix = "# Authored Loader\r\n\r\nPreserve row order and policy exactly.",
            Entries = rows,
            LineEnding = "\r\n",
        }));
        Files.WriteBytes("unrelated.bin", [0, 239, 187, 191, 255, 13, 10]);
    }

    internal string Settings(IEnumerable<string> routes, string? extra = null)
        => $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{Layout.MainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":[{{string.Join(",", routes.Select(path => $"\"{path}\""))}}]{{extra}}}""";

    internal static string Entrypoint(string entries = MarkdownEntriesSectionReader.EmptyEntry)
        => OpenForgeDocumentSeed.Metadata("Independent route metadata", [Tag], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Prefix = "# Authored route\r\n\r\nKeep this introduction byte-exact.",
            Entries = entries,
            LineEnding = "\r\n",
        }));

    internal RouteUpdateRequest Request(string? path = null, RouteUpdatePatchRequest? patch = null,
        RouteUpdateMode mode = RouteUpdateMode.Apply, string? template = null)
        => new(Workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + (path ?? Target),
            patch ?? RouteUpdateIntegrationWorkspace.DescriptionPatch(After), template, mode);

    internal ValueTask<RouteUpdateResult> Execute(RouteUpdateRequest request)
        => RouteUpdateOperationFactory.Create(_locks.StoreRoot).ExecuteAsync(request, TestContext.Current.CancellationToken);

    internal async Task<RouteUpdatePlan> Plan(RouteUpdateRequest? request = null)
    {
        var build = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(request ?? Request());
        Assert.True(build.Plan is not null, string.Join(Environment.NewLine, build.Formation.Findings.Select(finding => finding.Cause)));
        return Assert.IsType<RouteUpdatePlan>(build.Plan);
    }

    internal ValueTask<RouteUpdateResult> Apply(RouteUpdatePlan plan)
        => RouteUpdateIntegrationWorkspace.ExecutePlanAsync(plan, _locks.StoreRoot);

    internal async ValueTask<WorkspaceLockLease> Lease(Guid operationId)
    {
        var result = await _locks.AcquireAsync(new(Workspace, RouteUpdateDefinitions.CommandIdentity, operationId), TestContext.Current.CancellationToken);
        return result.Lease ?? throw new InvalidOperationException(result.Cause);
    }

    internal byte[] Bytes(string path) => File.ReadAllBytes(Files.Combine(path));
    internal string Text(string path) => File.ReadAllText(Files.Combine(path));
    internal void TrackRecovery(string path) => _recoveryPaths.Add(path);

    internal void AssertPreserved(IReadOnlyDictionary<string, string> before)
    {
        var after = Files.SnapshotHashes();
        foreach (var row in before.Where(row => !row.Key.StartsWith(DirectoryPath + "/", StringComparison.Ordinal)))
            Assert.Equal(row.Value, after[row.Key]);
        Assert.All(after.Keys.Except(before.Keys, StringComparer.Ordinal), path => Assert.StartsWith(DirectoryPath + "/", path, StringComparison.Ordinal));
    }

    internal static byte[] ChangedDocument()
        => Encoding.UTF8.GetBytes(AuthoredDocument.Replace("  description: Before outside", "  description: After outside", StringComparison.Ordinal));

    public void Dispose()
    {
        foreach (var path in _recoveryPaths) if (File.Exists(path)) File.Delete(path);
        _locks.Dispose();
        Files.Dispose();
    }
}
