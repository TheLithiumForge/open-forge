using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairContinuationReader
{
    internal static async ValueTask<bool> NeedsIndexAsync(CliWorkspace workspace, RouteDoctorView routes, CancellationToken token)
    {
        var paths = new PhysicalPathResolver();
        var ownership = await WorkspaceOwnershipReader.ReadAsync(paths, workspace, token).ConfigureAwait(false);
        var settings = await WorkspaceSettingsReader.ReadAsync(paths, workspace, token).ConfigureAwait(false);
        if (ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete)
            || settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)) return false;
        var roots = routes.Routes.Topology.LoaderRootPaths.ToImmutableArray();
        var policy = new RoutedMetadataCompletionPolicy();
        var context = new RoutedMetadataPolicyContext
        {
            Settings = settings.Document,
            Ownership = ownership.Document,
            NativeSkillPaths = [.. routes.Sources.Where(source => source.Source.Base.Form == OpenForge.Cli.Core.Framework.Sources.Models.Identity.SourceDocumentForm.Skill)
                .Select(source => source.Source.Identity.CanonicalBasePath)],
        };
        var eligible = ImmutableArray.CreateBuilder<OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceLogicalSource>();
        var metadataMissing = false;
        foreach (var source in routes.Sources.Where(source => source.IsReadable && source.Document is not null))
        {
            var path = source.Source.Identity.CanonicalBasePath;
            var file = new RoutedMetadataFileFacts
            {
                CanonicalPath = path,
                IsSelected = true,
                IsReadable = true,
                IsSafeOrdinaryLeaf = NoFollowLeafObserver.Observe(paths, workspace, SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path), token).State == NoFollowLeafState.OrdinaryFile,
                LoaderRootEntrypointPath = roots.FirstOrDefault(root => path.StartsWith(SourceLogicalPath.ReadParent(root) + "/", StringComparison.Ordinal)),
                RouteEntrypointPaths = [.. routes.Sources.Where(parent => SourceFormClassifier.IsEntrypoint(parent.Source.Base.Form)
                    && path.StartsWith(SourceLogicalPath.ReadParent(parent.Source.Identity.CanonicalBasePath) + "/", StringComparison.Ordinal))
                    .Select(parent => parent.Source.Identity.CanonicalBasePath)],
                IsGeneratedFile = false,
                HasManagedAuthoredRegions = false,
            };
            if (policy.Evaluate(file, context) != RoutedMetadataEligibility.Eligible) continue;
            var completion = policy.Complete(new() { File = file, Bytes = [.. Encoding.UTF8.GetBytes(source.Document?.Source ?? throw new InvalidOperationException())] }, context).Completion;
            if (completion.State is FrameworkMetadataCompletionState.Malformed or FrameworkMetadataCompletionState.Unsafe) continue;
            eligible.Add(source.Source);
            metadataMissing |= completion.State == FrameworkMetadataCompletionState.Completed;
        }
        var chain = new RouteScaffoldChainDiscovery().Discover(new()
        {
            Catalogue = routes.Catalogue,
            RecognizedRootPaths = roots,
            EligibleSources = eligible.ToImmutable(),
            Settings = settings.Document,
            Sharing = new SourceSharing(ownership.Document.Framework?.GitIgnoredRoutes ?? []),
        });
        return metadataMissing || !chain.IsBlocked && !chain.Gaps.IsEmpty
            || routes.GeneratedNavigation.Any(target => target.State == OpenForge.Cli.Core.Framework.OperationalContributors.Models.OperationalGeneratedNavigationState.Changed);
    }
}
