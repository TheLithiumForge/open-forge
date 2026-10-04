using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallEstablishmentPlanner
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly PhysicalPathResolver _physicalPathResolver;
    private readonly WorkspaceOwnershipStore _ownershipStore = new();
    private readonly InstallContentIdentity _contentIdentity = new();
    private readonly MarkdownDocumentParser _markdownDocumentParser = new();

    internal InstallEstablishmentPlanner(
        PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal InstallPlanningDecision Build(InstallEstablishmentPlanInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var context = input.Context;
        var request = context.Request;
        var intendedState = context.IntendedState;
        var findings = new List<InstallFinding>();
        var effects = new List<InstallFileEffect>();
        var hasEligibleOccupant = false;
        var userOwnedPaths = intendedState.UserOwnedPaths
            .Select(PortableWorkspacePath.CreatePortableKey)
            .ToHashSet(StringComparer.Ordinal);
        var migrationPaths = intendedState.Migrations
            .Select(migration => migration.Path)
            .ToHashSet(StringComparer.Ordinal);
        try
        {
            var competingPaths = input.Ownership.Document.Extensions
                .SelectMany(extension => extension.Paths.Concat(extension.Regions.Select(region => region.Path)))
                .Concat(LibraryRegistrationReader.ReadRegistrations(input.Ownership.Document).Libraries
                    .SelectMany(library => LibraryPathIdentity.Mappings(library).Select(mapping => mapping.DestinationPath.Value)))
                .Select(PortableWorkspacePath.CreatePortableKey)
                .ToHashSet(StringComparer.Ordinal);
            var conflict = intendedState.TargetBytes.Keys.Concat(intendedState.ManagedBlockBytes.Keys)
                .FirstOrDefault(path => competingPaths.Contains(PortableWorkspacePath.CreatePortableKey(path)));
            if (conflict is not null)
            {
                return new InstallPlanningWithFindings(context, InstallManagementState.Blocked,
                    [new InstallFinding(InstallFindingCode.OwnershipConflict,
                        "Another manager owns a selected Framework destination.", conflict)]);
            }
        }
        catch (ArgumentException exception)
        {
            return new InstallPlanningWithFindings(context, InstallManagementState.Blocked,
                [new InstallFinding(InstallFindingCode.LifecycleBlocked,
                    $"Recorded competing ownership could not be safely interpreted: {exception.Message}", WorkspaceOwnershipDefinitions.RelativePath)]);
        }

        var completedTargetBytes = intendedState.TargetBytes
            .ToImmutableDictionary(StringComparer.Ordinal)
            .ToBuilder();
        var projectionTargetBytes = intendedState.ProjectionTargetBytes;

        foreach (var target in intendedState.TargetBytes.OrderBy(
                     value => value.Key,
                     StringComparer.Ordinal))
        {
            var read = input.CurrentTargets[target.Key];
            var isGeneratedRegion = intendedState.GeneratedRegionPaths.Contains(target.Key);
            var isUserOwned = userOwnedPaths.Contains(
                PortableWorkspacePath.CreatePortableKey(target.Key));
            var isVerifiedManagedTarget = input.VerifiedManagedTargetPaths.Contains(target.Key);
            var isPreservedEntrypoint = intendedState.PreservedEntrypointPaths.Contains(target.Key);
            var canRebaseGeneratedRegion = isGeneratedRegion
                && read.State == InstallTargetReadState.File
                && (!isUserOwned || request.Configuration is not null)
                && isVerifiedManagedTarget;
            var isNewAdoptedEntrypoint = isUserOwned
                && intendedState.Migrations.Any(migration =>
                    string.Equals(migration.Path, target.Key, StringComparison.Ordinal)
                    && migration.Actions.Contains(WorkspaceAdoptionAction.EntrypointCreated));
            if (read.State == InstallTargetReadState.File)
            {
                if (isNewAdoptedEntrypoint)
                {
                    findings.Add(new InstallFinding(
                        code: InstallFindingCode.TargetUnsafe,
                        cause: "A planned local entrypoint destination became occupied before Install planning completed.",
                        subject: target.Key));
                    continue;
                }

                if (isGeneratedRegion
                    && !isUserOwned
                    && !isPreservedEntrypoint
                    && !HasSafeGeneratedRegion(read, out var generatedCause))
                {
                    findings.Add(new InstallFinding(
                        code: InstallFindingCode.GeneratedRegionUnsafe,
                        cause: generatedCause,
                        subject: target.Key));
                    continue;
                }

                if (!isUserOwned && !isVerifiedManagedTarget && !isPreservedEntrypoint)
                {
                    hasEligibleOccupant = true;
                    if (!request.Force)
                    {
                        findings.Add(new InstallFinding(
                            code: InstallFindingCode.TargetOccupied,
                            cause: "An exact current Framework destination is occupied before management establishment.",
                            subject: target.Key));
                        continue;
                    }
                }
            }

            var intendedEffectBytes = target.Value;
            if (canRebaseGeneratedRegion
                && !TryRebaseGeneratedRegion(
                    read,
                    target.Value,
                    out intendedEffectBytes,
                    out var rebaseCause))
            {
                findings.Add(new InstallFinding(
                    code: InstallFindingCode.GeneratedRegionUnsafe,
                    cause: rebaseCause,
                    subject: target.Key));
                continue;
            }

            if (canRebaseGeneratedRegion)
            {
                completedTargetBytes[target.Key] = intendedEffectBytes;
                if (!intendedEffectBytes.AsSpan().SequenceEqual(target.Value))
                {
                    projectionTargetBytes ??= intendedState.TargetBytes
                        .ToImmutableDictionary(StringComparer.Ordinal);
                }
            }

            if (CreateEffect(new InstallFileEffectInput
            {
                Read = read,
                IntendedBytes = intendedEffectBytes,
                Kind = canRebaseGeneratedRegion
                        ? InstallEffectKind.GeneratedRegion
                        : InstallEffectKind.File,
                Action = read.State == InstallTargetReadState.Missing
                        ? InstallEffectAction.Create
                        : InstallEffectAction.Replace,
                SourceAssetPath = isUserOwned
                        || (isGeneratedRegion && read.State == InstallTargetReadState.File)
                            ? null
                            : target.Key,
            }) is { } effect)
            {
                effects.Add(effect);
            }
        }

        foreach (var block in intendedState.ManagedBlockBytes.OrderBy(
                     value => value.Key,
                     StringComparer.Ordinal))
        {
            var read = input.CurrentTargets[block.Key];
            if (read.Snapshot is not { } snapshot)
            {
                throw new InvalidOperationException(
                    "A readable managed host requires its exact snapshot.");
            }

            if (read.State == InstallTargetReadState.Missing)
            {
                effects.Add(CreateEffect(new InstallFileEffectInput
                {
                    Read = read,
                    IntendedBytes = block.Value,
                    Kind = InstallEffectKind.File,
                    Action = InstallEffectAction.Create,
                    SourceAssetPath = block.Key,
                })
                    ?? throw new InvalidOperationException(
                        "A missing managed host requires one create effect."));
                continue;
            }

            var blockResolution = _contentIdentity.ResolveManagedBlock(
                snapshot.Bytes.AsSpan(),
                block.Value);
            if (blockResolution.State == ManagedBlockState.Blocked)
            {
                findings.Add(new InstallFinding(
                    code: InstallFindingCode.TargetUnsafe,
                    cause: blockResolution.Cause
                        ?? "The managed host marker boundary is unsafe.",
                    subject: block.Key));
                continue;
            }

            if (blockResolution.State == ManagedBlockState.Present)
            {
                if (input.VerifiedManagedTargetPaths.Contains(block.Key))
                {
                    continue;
                }

                hasEligibleOccupant = true;
                if (!request.Force)
                {
                    findings.Add(new InstallFinding(
                        code: InstallFindingCode.TargetOccupied,
                        cause: "An Open Forge managed block is occupied before management establishment.",
                        subject: block.Key));
                    continue;
                }
            }

            var intendedDocument = blockResolution.IntendedDocumentBytes
                ?? throw new InvalidOperationException(
                    "A safe managed host resolution requires intended document bytes.");
            if (CreateEffect(new InstallFileEffectInput
            {
                Read = read,
                IntendedBytes = intendedDocument,
                Kind = InstallEffectKind.ManagedRegion,
                Action = blockResolution.State == ManagedBlockState.Absent
                        ? InstallEffectAction.Append
                        : InstallEffectAction.Replace,
                SourceAssetPath = block.Key,
            }) is { } effect)
            {
                effects.Add(effect);
            }
        }

        if (findings.Count > 0)
        {
            var state = findings.Any(finding =>
                    finding.Code is InstallFindingCode.TargetUnsafe
                        or InstallFindingCode.GeneratedRegionUnsafe)
                ? InstallManagementState.Blocked
                : InstallManagementState.EligibleInitialOccupant;
            return new InstallPlanningWithFindings(context, state, findings);
        }

        AddConfigurationEffects(intendedState, effects);

        var ownershipPlan = _ownershipStore.PlanFrameworkOwnership(
            input.Ownership,
            request.Configuration is not null
                ? InstallConfigurationOwnership.Build(input, effects)
                : new FrameworkOwnership(
                new OwnedSource("embedded-framework", null),
                effects
                    .Where(effect => effect.Identity.Kind == InstallEffectKind.File)
                    .Select(effect => effect.RelativePath)
                    .Concat(intendedState.PreservedEntrypointPaths)
                    .Concat(input.Ownership.Document.Framework?.Paths ?? [])
                    .Where(path => !userOwnedPaths.Contains(
                        PortableWorkspacePath.CreatePortableKey(path)))
                    .Where(path => path is not (FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath))
                    .Distinct(StringComparer.Ordinal)
                    .ToImmutableArray(),
                intendedState.GeneratedRegionPaths
                    .Select(path => new OwnedRegion(path, "entries"))
                    .Concat(intendedState.ManagedBlockBytes.Keys.Select(path =>
                        new OwnedRegion(path, WorkspaceOwnershipDefinitions.ManagedBlockRegion)))
                    .Concat(input.Ownership.Document.Framework?.Regions ?? [])
                    .Distinct()
                    .ToImmutableArray()));
        var ownershipEffect = ownershipPlan.Change is { } ownershipChange
            ? new InstallFileEffect
            {
                Identity = new InstallEffectIdentity
                {
                    Path = WorkspaceOwnershipDefinitions.RelativePath,
                    Kind = InstallEffectKind.File,
                    Action = ownershipChange.Kind == PlannedFileChangeKind.Create
                        ? InstallEffectAction.Create
                        : InstallEffectAction.Replace,
                    SourceAssetPath = null,
                },
                Change = ownershipChange,
                RecoveryTarget = RecoveryBundleTarget.Create(
                    ownershipChange,
                    input.Ownership.Snapshot
                        ?? throw new InvalidOperationException(
                            "A workspace ownership write plan requires exact prior file facts.")),
            }
            : null;
        IReadOnlyList<PlannedDirectoryCreation> directories;
        try
        {
            directories = CreateDirectoryPlan(
                request,
                context.AgentsDirectoryExpectation,
                effects.Select(effect => effect.Change)

                    .Concat(ownershipEffect is null
                        ? []
                        : [ownershipEffect.Change]));
        }
        catch (InvalidDataException exception)
        {
            return new InstallPlanningWithFindings(
                context,
                InstallManagementState.Blocked,
                [new InstallFinding(
                    InstallFindingCode.TargetUnsafe,
                    exception.Message)]);
        }

        var hasPhysicalEffects = effects.Count > 0
            || ownershipEffect is not null
            || directories.Count > 0;
        var managementState = hasEligibleOccupant
            ? InstallManagementState.EligibleInitialOccupant
            : InstallManagementState.SafelyAbsent;
        if (input.VerifiedManagedTargetPaths.Count > 0 && !hasPhysicalEffects)
        {
            managementState = InstallManagementState.TrustedExact;
        }
        else if (input.VerifiedManagedTargetPaths.Count > 0
                 && effects.Any(effect => migrationPaths.Contains(effect.RelativePath)))
        {
            managementState = InstallManagementState.ManagedAdoption;
        }

        var completedContext = context with
        {
            IntendedState = intendedState with
            {
                TargetBytes = completedTargetBytes.ToImmutable(),
                ProjectionTargetBytes = projectionTargetBytes,
            },
        };

        return new InstallPlanningCompleted(
            completedContext,
            managementState,
            new InstallPlanEffects
            {
                DirectoryCreations = directories,
                TargetEffects = effects,
                OwnershipEffect = ownershipEffect,
            });
    }

    private static void AddConfigurationEffects(InstallIntendedState intended, List<InstallFileEffect> effects)
    {
        if (intended.Configuration is not { } configuration) return;
        if (configuration.SettingsChange is { } settingsChange)
            effects.Add(ConfigurationEffect(".agents/open-forge.json", settingsChange,
                configuration.Settings.Snapshot ?? throw new InvalidOperationException("Settings require original bytes.")));
        if (configuration.IgnoreChange is { } ignoreChange)
            effects.Add(ConfigurationEffect(InstallIgnoreSection.Path, ignoreChange,
                configuration.Ignore.Snapshot ?? throw new InvalidOperationException("Ignore changes require original bytes.")));
    }

    private static InstallFileEffect ConfigurationEffect(string path, PlannedFileChange change, FileStateSnapshot before)
        => new()
        {
            Identity = new()
            {
                Path = path,
                Kind = InstallEffectKind.File,
                Action = change.Kind == PlannedFileChangeKind.Create ? InstallEffectAction.Create : InstallEffectAction.Replace,
                SourceAssetPath = null
            },
            Change = change,
            RecoveryTarget = RecoveryBundleTarget.Create(change, before),
        };

    private IReadOnlyList<PlannedDirectoryCreation> CreateDirectoryPlan(
        Commands.Install.Models.Request.InstallRequest request,
        FileExpectation agentsDirectoryExpectation,
        IEnumerable<PlannedFileChange> changes)
    {
        var agentsPath = Path.Combine(
            request.Workspace.LexicalRoot,
            SourceLogicalPath.AgentsRoot);
        var missing = new HashSet<string>(PhysicalIdentityTracker.PathComparer);
        if (agentsDirectoryExpectation.Kind == FileExpectationKind.Missing)
        {
            missing.Add(agentsPath);
        }

        foreach (var change in changes.Where(change =>
                     change.LogicalPath.StartsWith(
                         agentsPath + Path.DirectorySeparatorChar,
                         PathComparison())))
        {
            var parent = Path.GetDirectoryName(change.LogicalPath);
            while (parent is not null
                && !PhysicalIdentityTracker.PathComparer.Equals(
                    parent,
                    request.Workspace.LexicalRoot))
            {
                if (PhysicalIdentityTracker.PathComparer.Equals(parent, agentsPath))
                {
                    if (agentsDirectoryExpectation.Kind == FileExpectationKind.Missing)
                    {
                        missing.Add(parent);
                    }

                    break;
                }

                var resolution = _physicalPathResolver.ResolveCandidate(
                    request.Workspace.LexicalRoot,
                    request.Workspace.PhysicalRoot,
                    parent);
                if (resolution.State == PhysicalPathState.Missing)
                {
                    missing.Add(parent);
                    parent = Path.GetDirectoryName(parent);
                    continue;
                }

                if (resolution.State != PhysicalPathState.Contained
                    || !IsOrdinaryDirectory(resolution.GetContainedPhysicalPath()))
                {
                    throw new InvalidDataException(
                        "A planned Install directory parent is unsafe or unavailable.");
                }

                break;
            }
        }

        return missing
            .OrderBy(path => path.Count(character =>
                character == Path.DirectorySeparatorChar))
            .ThenBy(path => path, PhysicalIdentityTracker.PathComparer)
            .Select(path => PlannedDirectoryCreation.Create(
                FileExpectation.Missing(path)))
            .ToArray();
    }

    private static InstallFileEffect? CreateEffect(InstallFileEffectInput input)
    {
        var read = input.Read;
        var before = read.Snapshot
            ?? throw new InvalidOperationException(
                "A readable Install target requires one exact prior snapshot.");
        PlannedFileChange? change = read.State switch
        {
            InstallTargetReadState.Missing => PlannedFileChange.Create(
                before.Expectation,
                input.IntendedBytes),
            InstallTargetReadState.File when before.Bytes.AsSpan().SequenceEqual(input.IntendedBytes) => null,
            InstallTargetReadState.File when input.Kind == InstallEffectKind.GeneratedRegion
                => PlannedFileChange.ReplaceGeneratedRegion(
                    before.Expectation,
                    input.IntendedBytes),
            InstallTargetReadState.File => PlannedFileChange.Replace(
                before.Expectation,
                input.IntendedBytes),
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.State,
                "Only readable Install target states can form an effect."),
        };
        return change is null
            ? null
            : new InstallFileEffect
            {
                Identity = new InstallEffectIdentity
                {
                    Path = read.RelativePath,
                    Kind = input.Kind,
                    Action = input.Action,
                    SourceAssetPath = input.SourceAssetPath,
                },
                Change = change,
                RecoveryTarget = RecoveryBundleTarget.Create(change, before),
            };
    }

    private bool TryRebaseGeneratedRegion(
        InstallTargetRead read,
        ReadOnlySpan<byte> intendedDocumentBytes,
        out byte[] expectedDocumentBytes,
        out string cause)
    {
        expectedDocumentBytes = [];
        try
        {
            if (read.State != InstallTargetReadState.File
                || read.Snapshot is not { HasBytes: true } snapshot)
            {
                throw new InvalidDataException(
                    "A verified managed Entries target requires its captured current bytes.");
            }

            var currentDocument = StrictUtf8.GetString(snapshot.Bytes.AsSpan());
            var intendedDocument = StrictUtf8.GetString(intendedDocumentBytes);
            var currentFacts = _markdownDocumentParser.Parse(currentDocument);
            var intendedFacts = _markdownDocumentParser.Parse(intendedDocument);
            if (currentFacts.GeneratedRegion.State != MarkdownGeneratedRegionState.Complete
                || intendedFacts.GeneratedRegion.State != MarkdownGeneratedRegionState.Complete)
            {
                throw new InvalidDataException(
                    "Both current and intended managed Entries regions must be complete and unique.");
            }

            var currentEntries = currentFacts.GeneratedRegion.EntriesBlock
                ?? throw new InvalidDataException(
                    "The current managed Entries region has no established list span.");
            var intendedEntries = intendedFacts.GeneratedRegion.EntriesBlock
                ?? throw new InvalidDataException(
                    "The intended managed Entries region has no established list span.");
            if (!IsSupportedGeneratedLineEnding(currentEntries.LineEnding)
                || !IsSupportedGeneratedLineEnding(intendedEntries.LineEnding))
            {
                throw new InvalidDataException(
                    "Managed Entries rebasing requires an LF or CRLF list line ending.");
            }

            var currentSpan = currentEntries.Span;
            var intendedSpan = intendedEntries.Span;
            var beforeBody = currentDocument.Substring(
                currentSpan.Start,
                currentSpan.Length);
            var intendedBody = intendedDocument.Substring(
                intendedSpan.Start,
                intendedSpan.Length);
            var expectedBody = NormalizeGeneratedListLineEndings(
                intendedBody,
                currentEntries.LineEnding);
            var change = new GeneratedNavigationBoundedChange(
                new GeneratedNavigationBoundedChangeInput
                {
                    ContentLocation = new Utf8SourceMap(currentDocument).Map(
                        currentSpan.Start,
                        currentSpan.Length),
                    BeforeBody = beforeBody,
                    ExpectedBody = expectedBody,
                    Prefix = currentDocument[..currentSpan.Start],
                    Suffix = currentDocument[(currentSpan.Start + currentSpan.Length)..],
                });
            expectedDocumentBytes = change.ExpectedDocumentBytes.AsSpan().ToArray();
            cause = string.Empty;
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or InvalidOperationException)
        {
            cause = $"The verified managed Entries target could not be safely rebased: {exception.Message}";
            return false;
        }
    }

    private static string NormalizeGeneratedListLineEndings(
        string generatedList,
        string lineEnding)
    {
        var normalized = generatedList
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        return lineEnding == "\r\n"
            ? normalized.Replace("\n", "\r\n", StringComparison.Ordinal)
            : normalized;
    }

    private static bool IsSupportedGeneratedLineEnding(string lineEnding)
        => lineEnding is "\n" or "\r\n";

    private bool HasSafeGeneratedRegion(
        InstallTargetRead read,
        out string cause)
    {
        try
        {
            var snapshot = read.Snapshot
                ?? throw new InvalidDataException(
                    "An occupied generated target requires exact current bytes.");
            _ = _contentIdentity.ReadGeneratedFingerprint(snapshot.Bytes.AsSpan());
            cause = string.Empty;
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or InvalidOperationException)
        {
            cause = $"The occupied generated target has an unsafe marker boundary: {exception.Message}";
            return false;
        }
    }

    private static bool IsOrdinaryDirectory(string physicalPath)
    {
        try
        {
            var attributes = File.GetAttributes(physicalPath);
            return (attributes & FileAttributes.Directory) != 0
                && (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) == 0;
        }
        catch (Exception exception) when (exception is FileNotFoundException
            or DirectoryNotFoundException
            or UnauthorizedAccessException
            or IOException)
        {
            return false;
        }
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
}
