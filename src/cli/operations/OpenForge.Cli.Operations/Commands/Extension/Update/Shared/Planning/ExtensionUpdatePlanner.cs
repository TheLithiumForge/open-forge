using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning.Reconciliation;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning.Topology;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;

internal sealed class ExtensionUpdatePlanner
{
    private readonly ExtensionSourceReader _sourceReader;
    private readonly FileExpectationValidator _validator;
    private readonly PhysicalPathResolver _physicalPathResolver;
    private readonly ExtensionUpdateTopologyBuilder _topologyBuilder = new();
    private readonly ExtensionUpdateReconciler _reconciler;
    private readonly WorkspaceOwnershipStore _ownershipStore = new();
    private readonly CliPrompt<CliMultiSelectQuestion<string>, CliMultiSelection<string>> _selectionPrompt;
    private readonly string _selectionQuestion;

    internal ExtensionUpdatePlanner(
        ExtensionSourceReader sourceReader,
        FileExpectationValidator validator,
        PhysicalPathResolver physicalPathResolver,
        CliPrompt<CliMultiSelectQuestion<string>, CliMultiSelection<string>> selectionPrompt,
        string selectionQuestion)
    {
        ArgumentNullException.ThrowIfNull(sourceReader);
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(selectionPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionQuestion);
        _sourceReader = sourceReader;
        _validator = validator;
        _physicalPathResolver = physicalPathResolver;
        _reconciler = new ExtensionUpdateReconciler(validator);
        _selectionPrompt = selectionPrompt;
        _selectionQuestion = selectionQuestion;
    }

    internal async ValueTask<ExtensionUpdatePlanBuild> BuildAsync(
        ExtensionUpdateRequest request,
        CancellationToken cancellationToken)
        => await BuildAsync(request, frozenSelection: null, allowedRecovery: null, cancellationToken)
            .ConfigureAwait(false);

    internal ValueTask<ExtensionUpdatePlanBuild> BuildAsync(
        ExtensionUpdateRequest request,
        ExtensionUpdateSelection frozenSelection,
        CancellationToken cancellationToken)
        => BuildAsync(request, frozenSelection, allowedRecovery: null, cancellationToken);

    internal async ValueTask<ExtensionUpdatePlanBuild> BuildForVerificationAsync(
        ExtensionUpdateRequest request,
        RecoveryBundlePreparation? allowedRecovery,
        CancellationToken cancellationToken)
        => await BuildAsync(request, frozenSelection: null, allowedRecovery, cancellationToken).ConfigureAwait(false);

    internal ValueTask<ExtensionUpdatePlanBuild> BuildForVerificationAsync(
        ExtensionUpdateRequest request,
        ExtensionUpdateSelection frozenSelection,
        RecoveryBundlePreparation? allowedRecovery,
        CancellationToken cancellationToken)
        => BuildAsync(request, frozenSelection, allowedRecovery, cancellationToken);

    private async ValueTask<ExtensionUpdatePlanBuild> BuildAsync(
        ExtensionUpdateRequest request,
        ExtensionUpdateSelection? frozenSelection,
        RecoveryBundlePreparation? allowedRecovery,
        CancellationToken cancellationToken)
    {
        var ownership = await WorkspaceOwnershipReader.ReadAsync(
            _physicalPathResolver,
            request.Workspace,
            cancellationToken).ConfigureAwait(false);

        if (!HasInterpretableOwnership(ownership.Document.Extensions))
        {
            return Stop(request, ExtensionUpdateFindingCode.LifecycleObservation,
                "Recorded Extension identities, paths or dependencies cannot be interpreted; no ownership or file effects were inferred.");
        }

        RecoveryBundleCatalogueResult recovery;
        try
        {
            recovery = await RecoveryBundleCatalogue.ReadAsync(
                request.Workspace,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                request,
                ExtensionUpdateFindingCode.Interrupted,
                "Extension Update recovery inspection was interrupted.");
        }
        catch (Exception)
        {
            return Stop(
                request,
                ExtensionUpdateFindingCode.RecoveryUnavailable,
                "Extension Update recovery inspection failed unexpectedly.");
        }

        var recoveryIsExpected = recovery.State == RecoveryBundleCatalogueState.Available
            && (allowedRecovery is null
                ? recovery.Candidates.Length == 0
                : recovery.Candidates.Length == 1
                    && RecoveryBundleIdentity.Matches(recovery.Candidates[0], allowedRecovery));
        if (!recoveryIsExpected && (recovery.State != RecoveryBundleCatalogueState.Available || allowedRecovery is not null))
        {
            ExtensionUpdateFindingCode recoveryFinding;
            if (recovery.State == RecoveryBundleCatalogueState.Cancelled)
            {
                recoveryFinding = ExtensionUpdateFindingCode.Interrupted;
            }
            else if (recovery.State == RecoveryBundleCatalogueState.Available)
            {
                recoveryFinding = ExtensionUpdateFindingCode.RecoveryConflict;
            }
            else
            {
                recoveryFinding = ExtensionUpdateFindingCode.RecoveryUnavailable;
            }

            return Stop(
                request,
                recoveryFinding,
                recovery.Cause ?? "Recognized recovery residuals block Extension Update.");
        }

        var source = await _sourceReader.ReadAsync(
            request.Workspace,
            request.SourcePath,
            cancellationToken).ConfigureAwait(false);
        if (source.State != ExtensionSourceReadState.Complete)
        {
            return Stop(
                request,
                ReadSourceFinding(source),
                source.Cause ?? "The selected Extension source is unavailable.",
                selection: RequestSelection(request),
                source: SourceFact(source));
        }

        var inferred = !request.All
            && request.RequestedIds.Count == 0
            && source.Packages.Count == 1;
        string[] selectedIds;
        ExtensionUpdateSelectionKind selectionKind;
        if (frozenSelection is { } frozen)
        {
            selectionKind = frozen.SelectedBy;
            selectedIds = frozen.FrozenIds is { } frozenIds
                ? [.. frozenIds.Order(StringComparer.Ordinal)]
                : frozen.SelectedBy is ExtensionUpdateSelectionKind.ExplicitAll
                or ExtensionUpdateSelectionKind.InteractiveAll
                ? [.. source.Packages
                    .Where(package => ownership.Document.Extensions.Any(installed => installed.Id == package.Id))
                    .Select(package => package.Id)
                    .Order(StringComparer.Ordinal)]
                : [.. frozen.RootIds.Order(StringComparer.Ordinal)];
        }
        else if (request.All)
        {
            selectedIds = [.. ownership.Document.Extensions.Select(package => package.Id).Order(StringComparer.Ordinal)];
            selectionKind = ExtensionUpdateSelectionKind.ExplicitAll;
        }
        else if (inferred)
        {
            selectedIds = [source.Packages[0].Id];
            selectionKind = ExtensionUpdateSelectionKind.SinglePackageInference;
        }
        else if (request.RequestedIds.Count > 0)
        {
            selectedIds = [.. request.RequestedIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
            selectionKind = ExtensionUpdateSelectionKind.ExplicitIds;
        }
        else
        {
            var installedIds = ownership.Document.Extensions
                .Select(package => package.Id)
                .ToHashSet(StringComparer.Ordinal);
            var candidates = source.Packages
                .Where(package => installedIds.Contains(package.Id))
                .OrderBy(package => package.Id, StringComparer.Ordinal)
                .ToArray();
            if (candidates.Length == 0)
            {
                return Stop(
                    request,
                    ExtensionUpdateFindingCode.SelectionRequired,
                    "A multi-package Extension source requires an installed package selection.",
                    source: SourceFact(source));
            }
            if (request.Automatic || !request.AllowInteraction)
            {
                return Stop(
                    request,
                    ExtensionUpdateFindingCode.SelectionRequired,
                    "A multi-package Extension source requires explicit Extension IDs or --all.",
                    source: SourceFact(source));
            }
            // Update selection is scoped to packages that are already installed.
            // Source-only packages remain a dependency compatibility fact and
            // are evaluated by the frozen plan after selection; they are never
            // offered as update roots.
            var displayedIds = candidates
                .Select(package => package.Id)
                .ToHashSet(StringComparer.Ordinal);
            var question = new CliMultiSelectQuestion<string>(
                _selectionQuestion,
                [.. candidates
                    .OrderBy(package => package.Id, StringComparer.Ordinal)
                    .Select(package => new CliChoice<string>(package.Id, package.Id, package.Description))],
                [.. candidates.SelectMany(package => package.Dependencies
                    .Where(displayedIds.Contains)
                    .Select(dependency => new CliDependency<string>(package.Id, dependency)))],
                new HashSet<string>(StringComparer.Ordinal),
                CliDependencyDirection.Requires);
            var reply = await _selectionPrompt(
                question,
                new CliPromptPolicy(Allowed: true),
                cancellationToken).ConfigureAwait(false);
            if (reply.State == CliPromptState.Unavailable)
            {
                return Stop(request, ExtensionUpdateFindingCode.SelectionRequired,
                    "A multi-package Extension source requires explicit Extension IDs or --all.",
                    source: SourceFact(source));
            }
            if (reply.State == CliPromptState.Cancelled)
            {
                return Stop(request, ExtensionUpdateFindingCode.Interrupted,
                    "Extension update was cancelled. Nothing was changed.",
                    source: SourceFact(source));
            }
            if (reply.State != CliPromptState.Answered || reply.Value.Chosen.Count == 0)
            {
                return Stop(request, ExtensionUpdateFindingCode.SelectionRequired,
                    "A multi-package Extension source requires at least one selected package.",
                    source: SourceFact(source));
            }
            selectedIds = [.. reply.Value.Chosen.Order(StringComparer.Ordinal)];
            selectionKind = reply.Value.Chosen.Count == candidates.Length
                ? ExtensionUpdateSelectionKind.InteractiveAll
                : ExtensionUpdateSelectionKind.InteractiveIds;
        }

        var selection = frozenSelection ?? new ExtensionUpdateSelection(
            selectionKind,
            request.All || selectionKind == ExtensionUpdateSelectionKind.InteractiveAll ? [] : selectedIds,
            selectionKind is ExtensionUpdateSelectionKind.ExplicitAll
                or ExtensionUpdateSelectionKind.InteractiveAll
                ? selectedIds
                : null);
        var installed = ownership.Document.Extensions.Select(package => package.Id)
            .ToHashSet(StringComparer.Ordinal);
        var absentInstalled = selectedIds.FirstOrDefault(id => !installed.Contains(id));
        if (absentInstalled is not null)
        {
            return Stop(
                request,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.LifecycleObservation,
                    "The selected Extension has no recorded ownership.",
                    absentInstalled),
                selection,
                SourceFact(source));
        }

        var closure = ResolveClosure(source.Packages, selectedIds);
        if (closure.Cause is not null)
        {
            return Stop(
                request,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.SourceUnavailable,
                    closure.Cause,
                    closure.Target),
                selection,
                SourceFact(source));
        }

        var uninstalledDependency = closure.Packages.FirstOrDefault(
            package => !installed.Contains(package.Id));
        if (uninstalledDependency is not null)
        {
            return Stop(
                request,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.LifecycleObservation,
                    "Extension Update cannot introduce an uninstalled dependency.",
                    uninstalledDependency.Id),
                selection,
                SourceFact(source));
        }

        var selectedSet = closure.Packages.Select(package => package.Id)
            .ToHashSet(StringComparer.Ordinal);
        var libraryTargets = closure.Packages.SelectMany(package => package.Payload)
            .Select(file => file.TargetPath)
            .OfType<string>()
            .Concat(ownership.Document.Extensions
                .Where(package => selectedSet.Contains(package.Id))
                .SelectMany(package => package.Paths));
        var unsafeTarget = libraryTargets.FirstOrDefault(path => !ExtensionDestinationPolicy.IsAllowed(path));
        if (unsafeTarget is not null)
        {
            return Stop(request, new ExtensionUpdateFinding(ExtensionUpdateFindingCode.TargetUnsafe,
                "The Extension destination is not an eligible workspace file.", unsafeTarget), selection, SourceFact(source));
        }
        var managedPaths = ownership.Document.Extensions.SelectMany(extension => extension.Paths).ToArray();
        var alias = closure.Packages.SelectMany(package => package.Payload).Select(file => file.TargetPath).OfType<string>()
            .FirstOrDefault(target => managedPaths.Any(path => path != target
                && PortableWorkspacePath.CreatePortableKey(path) == PortableWorkspacePath.CreatePortableKey(target)));
        if (alias is not null)
        {
            return Stop(request, new ExtensionUpdateFinding(ExtensionUpdateFindingCode.OwnershipConflict,
                "The Extension destination aliases a managed path.", alias), selection, SourceFact(source));
        }
        var libraryBoundary = await ReadLibraryBoundaryAsync(
            request,
            libraryTargets,
            cancellationToken).ConfigureAwait(false);
        if (libraryBoundary is not null)
        {
            return Stop(request, libraryBoundary, selection, SourceFact(source));
        }

        var topologyBuild = await BuildTopologyAsync(
            new TopologyBuildInput
            {
                Request = request,
                Packages = closure.Packages,
                Ownership = ownership.Document,
                Selection = selection,
                Source = source,
                Admission = new ExtensionUpdateTopologyAdmission(
                    new Dictionary<string, byte[]>(StringComparer.Ordinal),
                    new HashSet<string>(StringComparer.Ordinal)),
            },
            cancellationToken).ConfigureAwait(false);
        if (topologyBuild.Boundary is not null)
        {
            return topologyBuild.Boundary;
        }

        var topology = topologyBuild.Topology
            ?? throw new InvalidOperationException("A complete topology build requires topology facts.");
        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        if (payloadRead.Payload is not { } frameworkPayload)
        {
            return Stop(request, ExtensionUpdateFindingCode.FrameworkUnavailable,
                payloadRead.Cause ?? "The running Framework inventory is unavailable.", selection, SourceFact(source));
        }
        var container = Path.Combine(request.Workspace.LexicalRoot, ".agents");
        var containerBoundary = _physicalPathResolver.ResolveCandidate(request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot, container);
        if (containerBoundary.State != PhysicalPathState.Contained
            || !Directory.Exists(containerBoundary.GetContainedPhysicalPath())
            || (File.GetAttributes(container) & FileAttributes.ReparsePoint) != 0)
        {
            return Stop(request, ExtensionUpdateFindingCode.FrameworkUnsafe,
                "The .agents Framework container is not a contained ordinary directory.", selection, SourceFact(source));
        }

        var reconciliation = await _reconciler.BuildAsync(
            new ExtensionUpdateReconciliationInput
            {
                Request = request,
                Packages = closure.Packages,
                Ownership = ownership.Document,
                Topology = topology,
                SourceIdentity = source.Identity,
            },
            cancellationToken).ConfigureAwait(false);
        if (reconciliation.Finding is null
            && (reconciliation.AdmittedOverrides.Count > 0
                || reconciliation.AdmittedExclusions.Count > 0))
        {
            var admittedTopology = await BuildTopologyAsync(
                new TopologyBuildInput
                {
                    Request = request,
                    Packages = closure.Packages,
                    Ownership = ownership.Document,
                    Selection = selection,
                    Source = source,
                    Admission = new ExtensionUpdateTopologyAdmission(
                        reconciliation.AdmittedOverrides,
                        reconciliation.AdmittedExclusions),
                },
                cancellationToken).ConfigureAwait(false);
            if (admittedTopology.Boundary is not null)
            {
                return admittedTopology.Boundary;
            }

            topology = admittedTopology.Topology
                ?? throw new InvalidOperationException(
                    "A complete admitted topology build requires topology facts.");
        }

        if (reconciliation.Finding is null)
        {
            reconciliation = await _reconciler.ReprojectGeneratedAsync(
                request,
                topology,
                reconciliation,
                cancellationToken).ConfigureAwait(false);
        }
        var installedById = ownership.Document.Extensions
            .ToDictionary(package => package.Id, StringComparer.Ordinal);
        var packageFacts = closure.Packages.Select(package => new ExtensionUpdatePackage(
            package.Id,
            selectedIds.Contains(package.Id, StringComparer.Ordinal),
            package.Dependencies.Order(StringComparer.Ordinal),
            installedById.GetValueOrDefault(package.Id)?.Version,
            package.Version)).ToArray();
        if (reconciliation.Finding is not null)
        {
            return Boundary(
                request,
                ExtensionUpdateResultFormationFactory.Facts(new ExtensionUpdateResultFactsInput
                {
                    Selection = selection,
                    Source = SourceFact(source),
                    Packages = packageFacts,
                    Comparisons = reconciliation.Comparisons,
                    Topology = topology,
                    Effects = [],
                    LifecycleAction = ExtensionUpdateLifecycleAction.None,
                    LifecycleOutcome = ExtensionUpdateLifecycleOutcome.NotRequested,
                    RecoveryState = ExtensionUpdateRecoveryState.NotRequired,
                    Verification = ExtensionUpdateVerificationState.NotRequested,
                }),
                reconciliation.Finding);
        }

        var frameworkPaths = (ownership.Document.Framework?.Paths ?? [])
            .Concat(ownership.Document.Framework?.Regions.Select(region => region.Path) ?? [])
            .Select(PortableWorkspacePath.CreatePortableKey).ToHashSet(StringComparer.Ordinal);
        var frameworkConflict = libraryTargets.FirstOrDefault(path => frameworkPaths.Contains(PortableWorkspacePath.CreatePortableKey(path)));
        if (frameworkConflict is not null)
        {
            return Stop(request, new ExtensionUpdateFinding(ExtensionUpdateFindingCode.OwnershipConflict,
                "The Extension target is owned by the installed Framework.", frameworkConflict), selection, SourceFact(source));
        }
        var ownershipPlan = _ownershipStore.PlanExtensionOwnership(
            ownership,
            reconciliation.IntendedOwnership);
        RecoveryBundleTarget? ownershipRecovery = null;
        if (ownershipPlan.Change is { } ownershipChange)
        {
            ownershipRecovery = RecoveryBundleTarget.Create(
                ownershipChange,
                ownership.Snapshot
                    ?? throw new InvalidOperationException(
                        "An Extension ownership write plan requires exact prior file facts."));
        }

        var hasChanges = reconciliation.Effects.Count > 0
            || ownershipPlan.Change is not null;
        if (hasChanges && !recoveryIsExpected)
        {
            return Stop(request, ExtensionUpdateFindingCode.RecoveryConflict,
                "Recognized recovery residuals block new Extension Update effects.");
        }
        var facts = ExtensionUpdateResultFormationFactory.Facts(new ExtensionUpdateResultFactsInput
        {
            Selection = selection,
            Source = SourceFact(source),
            Packages = packageFacts,
            Comparisons = reconciliation.Comparisons,
            Topology = topology,
            Effects = [.. reconciliation.Effects.Select(effect => effect.Result)],
            LifecycleAction = ownershipPlan.State switch
            {
                OwnershipWritePlanState.Planned => ExtensionUpdateLifecycleAction.Publish,
                OwnershipWritePlanState.Unchanged => ExtensionUpdateLifecycleAction.Preserve,
                _ => ExtensionUpdateLifecycleAction.None,
            },
            LifecycleOutcome = ownershipPlan.State switch
            {
                OwnershipWritePlanState.Planned => ExtensionUpdateLifecycleOutcome.Planned,
                OwnershipWritePlanState.Unchanged => ExtensionUpdateLifecycleOutcome.AlreadyCurrent,
                _ => ExtensionUpdateLifecycleOutcome.NotRequested,
            },
            RecoveryState = hasChanges
                ? ExtensionUpdateRecoveryState.NotCreated
                : ExtensionUpdateRecoveryState.NotRequired,
            Verification = hasChanges
                ? ExtensionUpdateVerificationState.Planned
                : ExtensionUpdateVerificationState.Verified,
        });
        var plan = ExtensionUpdatePlan.Create(new ExtensionUpdatePlanInput
        {
            Request = request,
            SourceRead = source,
            SourceSignature = ReadSourceSignature(source),
            Selection = selection,
            Packages = closure.Packages,
            FrameworkPayload = frameworkPayload,
            Ownership = ownership,
            IntendedOwnership = reconciliation.IntendedOwnership,
            Topology = topology,
            Facts = facts,
            Findings = reconciliation.Findings,
            Effects = reconciliation.Effects,
            DirectoryCreations = reconciliation.DirectoryCreations,
            OwnershipChange = ownershipPlan.Change,
            OwnershipRecoveryTarget = ownershipRecovery,
        });
        return new ExtensionUpdatePlanBuild { Plan = plan, Result = plan.Result };
    }

    private async ValueTask<ExtensionUpdateFinding?> ReadLibraryBoundaryAsync(
        ExtensionUpdateRequest request,
        IEnumerable<string> targetPaths,
        CancellationToken cancellationToken)
    {
        var unsafeTarget = targetPaths.FirstOrDefault(path => !ExtensionDestinationPolicy.IsAllowed(request.Workspace, path)
            || !ExtensionDestinationInspector.HasOrdinaryAncestors(_physicalPathResolver, request.Workspace, path, cancellationToken));
        if (unsafeTarget is not null)
        {
            return new ExtensionUpdateFinding(ExtensionUpdateFindingCode.TargetUnsafe,
                "The Extension destination is a protected workspace or recovery path.", unsafeTarget);
        }
        var record = await LibraryRegistrationReader.ReadAsync(
            _physicalPathResolver,
            request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (record.State == LibraryRegistrationReadState.Complete)
        {
            var document = record.Record
                ?? throw new InvalidOperationException("A complete Library record observation requires its document.");
            var claimedPaths = document.Libraries
                .SelectMany(library => LibraryPathIdentity.Mappings(library).Select(mapping => mapping.DestinationPath))
                .Select(path => PortableWorkspacePath.CreatePortableKey(path.Value))
                .ToHashSet(StringComparer.Ordinal);
            var conflict = targetPaths.FirstOrDefault(path => claimedPaths.Contains(PortableWorkspacePath.CreatePortableKey(path)));
            return conflict is null
                ? null
                : new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.OwnershipConflict,
                    "The Extension target is owned by a registered workspace Library.",
                    conflict);
        }

        return record.State switch
        {
            LibraryRegistrationReadState.Missing => null,
            LibraryRegistrationReadState.Unavailable => new ExtensionUpdateFinding(
                ExtensionUpdateFindingCode.ProjectionUnavailable,
                record.Cause ?? "Library ownership could not be observed for Extension Update."),
            LibraryRegistrationReadState.Malformed or LibraryRegistrationReadState.Blocked => new ExtensionUpdateFinding(
                ExtensionUpdateFindingCode.OwnershipConflict,
                record.Cause ?? "Library ownership is unsafe or ambiguous for Extension Update."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(request),
                record.State,
                "The Library record state is not defined."),
        };
    }

    private async ValueTask<TopologyBuild> BuildTopologyAsync(
        TopologyBuildInput input,
        CancellationToken cancellationToken)
    {
        var request = input.Request;
        try
        {
            var selectedIds = input.Packages.Select(package => package.Id)
                .ToHashSet(StringComparer.Ordinal);
            var intendedPaths = input.Packages.SelectMany(package => package.Payload)
                .Select(file => file.TargetPath)
                .OfType<string>()
                .ToHashSet(StringComparer.Ordinal);
            var retiredPaths = request.Prune
                ? input.Ownership.Extensions
                .Where(package => selectedIds.Contains(package.Id))
                .SelectMany(package => package.Paths)
                .Where(path => !intendedPaths.Contains(path))
                .ToHashSet(StringComparer.Ordinal)
                : new HashSet<string>(StringComparer.Ordinal);
            var topology = await _topologyBuilder.BuildAsync(
                new ExtensionUpdateTopologyInput(
                    request,
                    input.Packages,
                    retiredPaths,
                    input.Admission),
                cancellationToken)
                .ConfigureAwait(false);
            return new TopologyBuild(topology, Boundary: null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new TopologyBuild(null, Stop(
                request,
                ExtensionUpdateFindingCode.Interrupted,
                "Extension Update topology planning was interrupted.",
                input.Selection,
                SourceFact(input.Source)));
        }
        catch (ExtensionUpdateTopologyAliasException exception)
        {
            return new TopologyBuild(null, Stop(
                request,
                ExtensionUpdateFindingCode.SourceIdentityConflict,
                exception.Message,
                input.Selection,
                SourceFact(input.Source)));
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException
            or IOException)
        {
            return new TopologyBuild(null, Stop(
                request,
                ExtensionUpdateFindingCode.ProjectionUnavailable,
                exception.Message,
                input.Selection,
                SourceFact(input.Source)));
        }
    }

    private static bool HasInterpretableOwnership(ImmutableArray<ExtensionOwnership> packages)
    {
        if (packages.GroupBy(package => package.Id, StringComparer.Ordinal).Any(group => group.Count() != 1)
            || packages.Any(package => !ExtensionIdentity.IsValidStableId(package.Id)
                || package.Paths.Any(path => !ExtensionDestinationPolicy.IsAllowed(path)))
            || packages.SelectMany(package => package.Paths).Distinct(StringComparer.Ordinal)
                .GroupBy(PortableWorkspacePath.CreatePortableKey, StringComparer.Ordinal)
                .Any(group => group.Count() != 1)) return false;
        var byId = packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var complete = new HashSet<string>(StringComparer.Ordinal);
        bool Visit(string id)
        {
            if (complete.Contains(id)) return true;
            if (!byId.TryGetValue(id, out var package) || !visiting.Add(id)) return false;
            if (!package.Dependencies.All(Visit)) return false;
            visiting.Remove(id);
            complete.Add(id);
            return true;
        }
        return packages.All(package => Visit(package.Id));
    }

    private static Closure ResolveClosure(
        IReadOnlyList<ExtensionPackageFact> universe,
        IReadOnlyList<string> roots)
    {
        var byId = universe.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<ExtensionPackageFact>();
        foreach (var root in roots.Order(StringComparer.Ordinal))
        {
            if (!Visit(root, out var cause, out var target))
            {
                return new Closure([], cause, target);
            }
        }

        return new Closure(ordered, Cause: null, Target: null);

        bool Visit(string id, out string? cause, out string? target)
        {
            cause = null;
            target = null;
            if (!byId.TryGetValue(id, out var package))
            {
                cause = "The selected installed Extension has no exact package in the selected source.";
                target = id;
                return false;
            }

            if (visited.Contains(id))
            {
                return true;
            }

            if (!visiting.Add(id))
            {
                cause = "The selected Extension dependency closure contains a cycle.";
                target = id;
                return false;
            }

            foreach (var dependency in package.Dependencies.Order(StringComparer.Ordinal))
            {
                if (!Visit(dependency, out cause, out target))
                {
                    return false;
                }
            }

            _ = visiting.Remove(id);
            _ = visited.Add(id);
            ordered.Add(package);
            return true;
        }
    }

    private static ExtensionUpdateFindingCode ReadSourceFinding(ExtensionSourceReadResult source)
        => source.State == ExtensionSourceReadState.Cancelled
            ? ExtensionUpdateFindingCode.Interrupted
            : source.FailureKind == ExtensionSourceFailureKind.Overlap
                ? ExtensionUpdateFindingCode.SourceOverlap
                : source.State == ExtensionSourceReadState.Invalid
                    ? ExtensionUpdateFindingCode.SourceInvalid
                    : ExtensionUpdateFindingCode.SourceUnavailable;

    private static ExtensionUpdateSelection? RequestSelection(ExtensionUpdateRequest request)
        => request.RequestedIds.Count > 0
            ? new ExtensionUpdateSelection(
                ExtensionUpdateSelectionKind.ExplicitIds,
                request.RequestedIds)
            : request.All
                ? new ExtensionUpdateSelection(
                    ExtensionUpdateSelectionKind.ExplicitAll,
                    [],
                    frozenIds: [])
                : null;

    private static ExtensionUpdateSource SourceFact(ExtensionSourceReadResult source)
        => new(
            source.Kind switch
            {
                ExtensionSourceKind.EmbeddedCatalogue => ExtensionUpdateSourceKind.Embedded,
                ExtensionSourceKind.Package => ExtensionUpdateSourceKind.Package,
                ExtensionSourceKind.Catalogue => ExtensionUpdateSourceKind.Catalogue,
                null => ExtensionUpdateSourceKind.Catalogue,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source.Kind, "The source kind is not defined."),
            },
            source.Kind == ExtensionSourceKind.EmbeddedCatalogue ? null : source.Identity,
            source.Identity,
            source.Packages.Count);

    internal static string ReadSourceSignature(ExtensionSourceReadResult source)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes(source.Identity));
        foreach (var package in source.Packages.OrderBy(package => package.Id, StringComparer.Ordinal))
        {
            hash.AppendData(Encoding.UTF8.GetBytes(
                $"\n{package.Id}\n{package.Name}\n{package.Description}\n{package.Version}\n{package.ManifestPath}"));
            foreach (var dependency in package.Dependencies.Order(StringComparer.Ordinal))
            {
                hash.AppendData(Encoding.UTF8.GetBytes($"\ndependency:{dependency}"));
            }

            foreach (var file in package.Payload.OrderBy(file => file.TargetPath, StringComparer.Ordinal))
            {
                hash.AppendData(Encoding.UTF8.GetBytes(
                    $"\nfile:{file.Path}\n{file.TargetPath}\n{file.State}\n{file.Sha256}"));
            }
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private static ExtensionUpdatePlanBuild Stop(
        ExtensionUpdateRequest request,
        ExtensionUpdateFindingCode code,
        string cause,
        ExtensionUpdateSelection? selection = null,
        ExtensionUpdateSource? source = null)
        => Stop(request, new ExtensionUpdateFinding(code, cause), selection, source);

    private static ExtensionUpdatePlanBuild Stop(
        ExtensionUpdateRequest request,
        ExtensionUpdateFinding finding,
        ExtensionUpdateSelection? selection = null,
        ExtensionUpdateSource? source = null)
        => Boundary(
            request,
            ExtensionUpdateResultFormationFactory.EmptyFacts(selection, source),
            finding);

    private static ExtensionUpdatePlanBuild Boundary(
        ExtensionUpdateRequest request,
        ExtensionUpdateResultFacts facts,
        ExtensionUpdateFinding finding)
        => new()
        {
            Plan = null,
            Result = ExtensionUpdateResultFormationFactory.Create(request, facts, [finding]),
        };

    private sealed record Closure(
        IReadOnlyList<ExtensionPackageFact> Packages,
        string? Cause,
        string? Target);

    private sealed record TopologyBuild(
        ExtensionUpdateTopology? Topology,
        ExtensionUpdatePlanBuild? Boundary);

    private sealed class TopologyBuildInput
    {
        internal required ExtensionUpdateRequest Request { get; init; }

        internal required IReadOnlyList<ExtensionPackageFact> Packages { get; init; }

        internal required WorkspaceOwnershipDocument Ownership { get; init; }

        internal required ExtensionUpdateSelection Selection { get; init; }

        internal required ExtensionSourceReadResult Source { get; init; }

        internal required ExtensionUpdateTopologyAdmission Admission { get; init; }
    }

}
