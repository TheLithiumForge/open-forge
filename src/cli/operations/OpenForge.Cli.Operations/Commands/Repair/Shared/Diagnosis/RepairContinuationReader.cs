using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Sharing;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairContinuationReader
{
    internal static bool NeedsIndex(DoctorObservation observation, CancellationToken token)
    {
        var routes = observation.Routes;
        if (routes.Catalogue is null || observation.LayoutRead?.Layout is not { } layout) return false;
        var workspace = layout.Workspace;
        var paths = new PhysicalPathResolver();
        var ownership = observation.LayoutRead.Ownership;
        var settings = observation.LayoutRead.Settings;
        if (ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete)
            || settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)) return false;
        var roots = routes.Routes.Topology.LoaderRootPaths.ToImmutableArray();
        var policy = new RoutedMetadataCompletionPolicy();
        var context = new RoutedMetadataPolicyContext
        {
            Layout = layout,
            LayoutRead = observation.LayoutRead,
            Settings = settings.Document,
            Ownership = ownership.Document,
        };
        var eligible = ImmutableArray.CreateBuilder<OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceLogicalSource>();
        var metadataMissing = false;
        foreach (var source in routes.Sources.Where(source => source.IsReadable && source.Document is not null))
        {
            var path = source.Source.Identity.CanonicalBasePath;
            if (!RepairOutsideAdmission.Allows(path, context, token)) continue;
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
            || routes.GeneratedNavigation.Any(target => target.State == OpenForge.Cli.Core.Framework.OperationalContributors.Models.OperationalGeneratedNavigationState.Changed
                && (!SourceLogicalPath.IsOutsideSource(layout, target.Path)
                    || roots.Any(root => target.Path.StartsWith(SourceLogicalPath.ReadParent(root) + "/", StringComparison.Ordinal))
                    && RepairOutsideAdmission.Allows(target.Path, context, token)));
    }
}
