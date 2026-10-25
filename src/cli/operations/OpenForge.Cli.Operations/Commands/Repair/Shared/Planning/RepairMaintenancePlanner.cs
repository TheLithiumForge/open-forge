using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal sealed class RepairMaintenancePlanner(PhysicalPathResolver paths, IRoutedMaintenanceRenameEvidence? renameEvidence = null)
{
    private readonly RoutedMaintenancePlanner _maintenance = new(paths, renameEvidence);

    internal async ValueTask<RepairPlan> CompleteAsync(RepairPlan plan, RepairCatalogueRead repairCatalogue, CancellationToken token)
    {
        plan = plan with { RenameEvidence = repairCatalogue.RenameEvidence, LayoutRead = repairCatalogue.LayoutRead };
        if (plan.IsBlocked || plan.Effects.Count == 0) return plan;
        if (plan.LayoutRead?.Layout is not { } layout) return Block(plan, "Repair requires the observed workspace layout.");
        var workspace = plan.Request.Workspace;
        var catalogue = repairCatalogue.Catalogue
            ?? (await new SourceReadSessionReader(paths).ReadIncludedAsync(layout, token).ConfigureAwait(false)).Catalogue;
        var ownership = plan.LayoutRead.Ownership;
        var settings = plan.LayoutRead.Settings;
        if (ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete)
            || settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete))
            return Block(plan, "Affected navigation settings or ownership could not be established.");
        var policy = new RoutedMetadataPolicyContext
        {
            Layout = layout,
            LayoutRead = plan.LayoutRead,
            Settings = settings.Document,
            Ownership = ownership.Document,
        };
        if (plan.Selection.Selected.Any(selected => !RepairProtectionReader.Allows(selected.Proposal.SourceCanonicalPath, policy)
            || !RepairOutsideAdmission.Allows(selected.Proposal.SourceCanonicalPath, policy, token)
            || !RepairOutsideAdmission.Allows(Uri.UnescapeDataString(selected.Resolution.Target.CanonicalTargetPath), policy, token, selected.Resolution.Target.RenameProof is not null)
            || !RepairProtectionReader.Allows(Uri.UnescapeDataString(selected.Resolution.Target.CanonicalTargetPath), policy)))
            return Block(plan, "Repair cannot change a protected source or select a protected target.");
        var sharing = new SourceSharing(ownership.Document.Framework?.GitIgnoredRoutes ?? []);
        catalogue = sharing.Project(catalogue);
        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);
        var affected = plan.Selection.Selected.SelectMany(selected => new[]
        {
            selected.Proposal.SourceCanonicalPath, Uri.UnescapeDataString(selected.Resolution.Target.CanonicalTargetPath),
            selected.Proposal.MissingPath,
            selected.Proposal.RenameProof?.OldPath, selected.Proposal.RenameProof?.NewPath,
            selected.Proposal.SourceMoveProof?.OldPath, selected.Proposal.SourceMoveProof?.NewPath,
        }).OfType<string>().Distinct(StringComparer.Ordinal).ToArray();
        var folders = affected.Select(SourceLogicalPath.ReadParent).Distinct(StringComparer.Ordinal).ToArray();
        var selectedMissingPaths = plan.Selection.Selected.SelectMany(selected => new[] { selected.Proposal.MissingPath, selected.Proposal.RenameProof?.OldPath })
            .OfType<string>().ToHashSet(StringComparer.Ordinal);
        var deferredFolders = repairCatalogue.MissingTargetReferences.Where(reference => selectedMissingPaths.Contains(reference.Facts.Target.Path!)
                && !plan.Selection.Selected.Any(selected =>
                selected.Proposal.SourceCanonicalPath == reference.SourcePath && selected.Proposal.Occurrence == reference.DestinationLocation))
            .Select(reference => SourceLogicalPath.ReadParent(reference.Facts.Target.Path!)).Distinct(StringComparer.Ordinal).ToArray();
        var roots = formation.Topology.LoaderRootPaths.Where(path => !SourceLogicalPath.IsOutsideSource(layout, path))
            .Concat(layout.OutsideRoutes.Where(route => affected.Any(path => path.StartsWith(route.DirectoryPath + "/", StringComparison.Ordinal)))
                .Select(route => route.CataloguePath)).Distinct(StringComparer.Ordinal).ToImmutableArray();
        var regionPaths = catalogue.Sources.Where(source => (SourceFormClassifier.IsEntrypoint(source.Base.Form) || source.Base.Form == SourceDocumentForm.Loader)
            && ExposesFolder(source.Identity.CanonicalBasePath, folders))
            .Select(source => source.Identity.CanonicalBasePath).ToHashSet(StringComparer.Ordinal);
        var selectedSources = catalogue.Sources.Where(source => folders.Contains(SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath), StringComparer.Ordinal)
            || regionPaths.Contains(source.Identity.CanonicalBasePath)
            || regionPaths.Any(parent => parent != layout.LoaderPath
                && formation.Topology.FindByPath(parent)?.ChildPaths.Contains(source.Identity.CanonicalBasePath) == true)).ToImmutableArray();
        var intended = plan.Effects.ToDictionary(effect => effect.SourceCanonicalPath, effect => effect.IntendedState.Bytes, StringComparer.Ordinal);
        var intent = await _maintenance.PrepareAsync(new()
        {
            Catalogue = catalogue,
            RootPaths = roots,
            SelectedSources = selectedSources,
            SelectedSourcePaths = [.. selectedSources.Select(source => source.Identity.CanonicalBasePath)],
            IntendedBytes = intended,
            ObservedDocuments = repairCatalogue.Documents,
            Policy = policy,
        }, token).ConfigureAwait(false);
        if (intent.Limitations.Any(item => !item.Malformed || affected.Contains(item.Path, StringComparer.Ordinal)
            || regionPaths.Contains(item.Path) && catalogue.FindByPath(item.Path) is { } invalid && SourceFormClassifier.IsEntrypoint(invalid.Base.Form)))
            return Block(plan, intent.Limitations.First().Cause);
        foreach (var entrypoint in intent.Entrypoints) regionPaths.Add(entrypoint.Source.Identity.CanonicalBasePath);
        // Entries can be the only identity record for another unresolved occurrence.
        // Revalidation rebuilds this deferral from fresh authored references under the lease.
        regionPaths.RemoveWhere(path => ExposesFolder(path, deferredFolders));
        var reader = new SourceDocumentReader(workspace);
        foreach (var document in repairCatalogue.Documents.Values) reader.Documents.Retain(document);
        var documents = intent.Documents.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        var regionSources = intent.Formation.Sources.Where(source => regionPaths.Contains(source.Identity.CanonicalBasePath)).ToArray();
        var metadataSources = regionSources.SelectMany(source => source.Base.Form == SourceDocumentForm.Loader
                ? intent.Formation.Topology.LoaderRootPaths.Where(path => !SourceLogicalPath.IsOutsideSource(layout, path)).ToArray()
                : intent.Formation.Topology.FindByPath(source.Identity.CanonicalBasePath)?.ChildPaths ?? [])
            .Select(path => intent.Formation.FindSource(path)).OfType<OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceLogicalSource>().Distinct().ToArray();
        var readExpectations = new List<FileExpectation>();
        foreach (var source in regionSources.Concat(metadataSources).Distinct())
        {
            var path = source.Identity.CanonicalBasePath;
            var outside = SourceLogicalPath.IsOutsideSource(layout, path);
            var proposed = intent.Entrypoints.Any(entrypoint => entrypoint.Source.Identity.CanonicalBasePath == path);
            if (outside && !proposed && !RepairOutsideAdmission.AllowsNavigationInput(path, policy, regionPaths.Contains(path), token))
                return Block(plan, $"The affected outside navigation input {path} was not admitted for maintenance.");
            if (outside && intent.Originals.TryGetValue(path, out var original)) readExpectations.Add(original.Expectation);
            if (documents.ContainsKey(path)) continue;
            try
            {
                if (outside)
                {
                    var verification = reader.Verify(source.Base, token);
                    token.ThrowIfCancellationRequested();
                    var snapshot = await new SourceDocumentSnapshotReader().ReadAsync(workspace, verification, token).ConfigureAwait(false);
                    documents[path] = reader.ParseDocument(Encoding.UTF8.GetString(snapshot.Bytes.AsSpan()));
                    readExpectations.Add(snapshot.Expectation);
                    continue;
                }
                var read = await reader.ReadAsync(source.Base, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (read.Read?.Value is not { } text) return Block(plan, $"The affected navigation input {path} could not be read.");
                documents[path] = reader.ParseDocument(text);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            { return Block(plan, $"The affected navigation input {path} could not be read."); }
        }
        var projection = new GeneratedNavigationProjector().Project(new(intent.Formation,
            regionSources.Select(source => new GeneratedNavigationRegionInput(source, documents[source.Identity.CanonicalBasePath])),
            metadataSources.Select(source => new GeneratedNavigationMetadata(source, new SourceAuthoredMetadataParser().Parse(documents[source.Identity.CanonicalBasePath], source.Base.Form))), sharing));
        if (!projection.IsComplete) return Block(plan, projection.Regions.First(region => region.State != GeneratedNavigationRegionState.Available).Cause
            ?? "Affected Entries could not be projected safely.");
        var assembled = RoutedMaintenanceEffectAssembler.Assemble(workspace, intent, projection);
        if (assembled.Changes.Any(change => plan.LibrarySteps.Any(step => step.Effect is not null
            && step.Selection.Proposal.Evidence.Entry.Input.Context.LogicalPath == change.LogicalPath)))
            return Block(plan, "Affected navigation and Library recovery address the same file.");
        var changes = assembled.Changes.ToList();
        var recovery = assembled.RecoveryTargets.ToList();
        foreach (var effect in plan.Effects.Where(effect => !changes.Any(change => change.LogicalPath == effect.FileChange.LogicalPath)))
        {
            changes.Add(effect.FileChange);
            recovery.Add(OpenForge.Cli.Core.Framework.Recovery.Models.Preparation.RecoveryBundleTarget.Create(effect.FileChange, effect.ExpectedState));
        }
        return plan with
        {
            Maintenance = new()
            {
                ReadExpectations = [.. readExpectations.Distinct()],
                Intent = intent,
                Projection = projection,
                Effects = new([.. changes], [.. recovery]),
                PolicyExpectations = [ownership.Snapshot?.Expectation ?? FileExpectation.Missing(ownership.LogicalPath),
                settings.Snapshot?.Expectation ?? FileExpectation.Missing(settings.LogicalPath)],
            }
        };
    }

    internal async ValueTask<bool> VerifyAsync(RepairPlan plan, CancellationToken token)
    {
        if (plan.Maintenance is not { } maintenance) return true;
        var expectations = new FileExpectationValidator(paths.CreateReadSession());
        foreach (var expectation in maintenance.ReadExpectations)
        {
            var validation = await expectations.ValidateAsync(plan.Request.Workspace, expectation, token).ConfigureAwait(false);
            if (validation.State != FileExpectationValidationState.Matched) return false;
        }
        return await _maintenance.VerifyProofsAsync(maintenance.Intent, token).ConfigureAwait(false);
    }

    private static bool ExposesFolder(string path, IReadOnlyList<string> folders)
    {
        var parent = SourceLogicalPath.ReadParent(path);
        return folders.Any(folder => folder == parent || folder.StartsWith(parent + "/", StringComparison.Ordinal));
    }

    private static RepairPlan Block(RepairPlan plan, string cause)
        => new(plan.Request, plan.Selection, plan.Steps, [.. plan.Conflicts, new(RepairConflictKind.UnsafeBoundary, null, null, cause)], plan.LibrarySteps)
        { RenameEvidence = plan.RenameEvidence, LayoutRead = plan.LayoutRead };
}
