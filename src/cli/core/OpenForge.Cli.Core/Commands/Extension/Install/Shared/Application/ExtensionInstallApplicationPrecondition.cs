using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;

internal sealed class ExtensionInstallApplicationPreconditionValidator(
    ExtensionInstallSourceResolver sourceResolver,
    ExtensionInstallFoundationReader foundationReader,
    MutationRevalidator revalidator,
    FileExpectationValidator validator)
{
    private readonly ExtensionInstallSourceResolver _sourceResolver = sourceResolver;
    private readonly ExtensionInstallFoundationReader _foundationReader = foundationReader;
    private readonly FileExpectationValidator _validator = validator;
    private readonly MutationRevalidator _revalidator = revalidator;

    internal async ValueTask<ExtensionInstallApplicationPrecondition> ValidateAsync(
        ExtensionInstallPlan plan,
        WorkspaceLockLease lease,
        CancellationToken cancellationToken)
    {
        if (!lease.IsHeldFor(plan.Request.Workspace))
        {
            return Stop(
                ExtensionInstallFindingCode.TargetChanged,
                "The Extension Install lease no longer matches the selected workspace.");
        }

        var sourceResolution = await _sourceResolver.ReadAsync(plan.Request, cancellationToken)
            .ConfigureAwait(false);
        var source = sourceResolution.Source;
        if (source.State != ExtensionSourceReadState.Complete)
        {
            return Stop(
                ReadSourceFindingCode(source.State),
                source.Cause ?? "The reviewed Extension source changed before application.",
                source.Identity);
        }

        if (!string.Equals(
            ExtensionInstallFoundationReader.SourceSignature(source),
            plan.SourceSignature,
            StringComparison.Ordinal))
        {
            return Stop(
                ExtensionInstallFindingCode.TargetChanged,
                "The reviewed Extension source changed after planning.",
                source.Identity);
        }

        if (!string.Equals(
            sourceResolution.InferredRootId,
            plan.InferredRootId,
            StringComparison.Ordinal))
        {
            return Stop(
                ExtensionInstallFindingCode.TargetChanged,
                "The inferred Extension package root changed after planning.",
                source.Identity);
        }

        var selected = ReadSelectedPackages(plan, source);
        if (selected is null)
        {
            return Stop(
                ExtensionInstallFindingCode.TargetChanged,
                "The selected dependency closure changed after planning.");
        }

        var observation = await _foundationReader.ReadAsync(
            plan.Request,
            selected,
            cancellationToken).ConfigureAwait(false);
        if (observation.Finding is { } foundationFinding)
        {
            return Stop(
                foundationFinding.Code == ExtensionInstallFindingCode.Interrupted
                    ? ExtensionInstallFindingCode.Interrupted
                    : ExtensionInstallFindingCode.TargetChanged,
                foundationFinding.Cause,
                foundationFinding.Target);
        }

        var current = observation.Foundation
            ?? throw new InvalidOperationException(
                "Complete Extension Install revalidation requires foundation facts.");
        if (!MatchesPlan(plan, current))
        {
            return Stop(
                ExtensionInstallFindingCode.TargetChanged,
                "Workspace ownership, topology, lifecycle, or Framework facts changed after planning.");
        }

        var changingPaths = plan.AllFileChanges.Select(change => change.LogicalPath).ToHashSet(StringComparer.Ordinal);
        foreach (var target in plan.Topology.IntendedTargetBytes)
        {
            var relative = target.Key.Replace('/', Path.DirectorySeparatorChar);
            var logical = Path.Combine(plan.Request.Workspace.LexicalRoot, relative);
            if (changingPaths.Contains(logical))
            {
                continue;
            }
            var expectation = FileExpectation.File(logical, Path.Combine(plan.Request.Workspace.PhysicalRoot, relative), FileExpectation.Hash(target.Value.AsSpan()));
            var check = await _validator.ValidateAsync(plan.Request.Workspace, expectation, cancellationToken).ConfigureAwait(false);
            if (check.State != FileExpectationValidationState.Matched)
            {
                return Stop(check.State == FileExpectationValidationState.Cancelled ? ExtensionInstallFindingCode.Interrupted : ExtensionInstallFindingCode.TargetChanged,
                    "An unchanged Extension target changed after planning.", target.Key);
            }
        }
        var validation = plan.IsNoOp ? MutationValidationResult.Valid() : await _revalidator.ValidateAsync(
            lease,
            plan.DirectoryCreations,
            plan.AllFileChanges,
            cancellationToken).ConfigureAwait(false);
        return validation.State switch
        {
            MutationValidationState.Valid => new(validation, Finding: null),
            MutationValidationState.Cancelled => new(
                validation,
                new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.Interrupted,
                    "Extension Install plan revalidation was interrupted.")),
            MutationValidationState.Mismatched
                or MutationValidationState.Blocked
                or MutationValidationState.Failed => new(
                validation,
                new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.TargetChanged,
                    validation.Cause
                        ?? "An Extension Install target changed or became unsafe before effects.")),
            _ => throw new ArgumentOutOfRangeException(
                nameof(plan),
                validation.State,
                "The mutation validation state is not defined."),
        };
    }

    private static ExtensionInstallFindingCode ReadSourceFindingCode(
        ExtensionSourceReadState state)
        => state switch
        {
            ExtensionSourceReadState.Cancelled => ExtensionInstallFindingCode.Interrupted,
            ExtensionSourceReadState.Missing
                or ExtensionSourceReadState.Invalid
                or ExtensionSourceReadState.Blocked
                or ExtensionSourceReadState.Unavailable => ExtensionInstallFindingCode.TargetChanged,
            ExtensionSourceReadState.Complete => throw new InvalidOperationException(
                "A complete Extension source read has no revalidation finding."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Extension source read state is not defined."),
        };

    private static List<ExtensionPackageFact>? ReadSelectedPackages(
        ExtensionInstallPlan plan,
        ExtensionSourceReadResult source)
    {
        var byId = source.Packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var selected = new List<ExtensionPackageFact>(plan.Packages.Count);
        foreach (var package in plan.Packages)
        {
            if (!byId.TryGetValue(package.Id, out var current))
            {
                return null;
            }

            selected.Add(current);
        }

        return selected;
    }

    private static bool MatchesPlan(
        ExtensionInstallPlan plan,
        ExtensionInstallFoundation current)
        => string.Equals(
                plan.FrameworkPayload.InventoryFingerprint,
                current.FrameworkPayload.InventoryFingerprint,
                StringComparison.Ordinal)
            && LifecycleEquals(plan.FrameworkLifecycle, current.FrameworkLifecycle)
            && LifecycleEquals(plan.CurrentLifecycle, current.CurrentExtensions)
            && TopologyEquals(plan.Topology, current.Topology);

    internal static bool LifecycleEquals(
        FrameworkLifecycleState expected,
        FrameworkLifecycleState actual)
        => JsonSerializer.SerializeToUtf8Bytes(
                expected,
                LifecycleJsonContext.Default.FrameworkLifecycleState)
            .AsSpan()
            .SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(
                actual,
                LifecycleJsonContext.Default.FrameworkLifecycleState));

    internal static bool LifecycleEquals(
        ExtensionLifecycleState expected,
        ExtensionLifecycleState actual)
        => JsonSerializer.SerializeToUtf8Bytes(
                expected,
                LifecycleJsonContext.Default.ExtensionLifecycleState)
            .AsSpan()
            .SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(
                actual,
                LifecycleJsonContext.Default.ExtensionLifecycleState));

    internal static bool TopologyEquals(
        ExtensionInstallTopology expected,
        ExtensionInstallTopology actual)
        => DictionaryEquals(expected.IntendedTargetBytes, actual.IntendedTargetBytes)
            && DictionaryEquals(expected.GeneratedTargetBytes, actual.GeneratedTargetBytes)
            && expected.Regions.SequenceEqual(actual.Regions)
            && expected.ProtectedPaths.SetEquals(actual.ProtectedPaths)
            && expected.InitialForceEligiblePaths.SetEquals(actual.InitialForceEligiblePaths);

    internal static bool AppliedTopologyEquals(
        ExtensionInstallTopology expected,
        ExtensionInstallTopology actual)
        => DictionaryEquals(expected.IntendedTargetBytes, actual.IntendedTargetBytes)
            && DictionaryEquals(expected.GeneratedTargetBytes, actual.GeneratedTargetBytes)
            && expected.Regions.Select(region => region.Path)
                .SequenceEqual(actual.Regions.Select(region => region.Path));

    private static bool DictionaryEquals(
        IReadOnlyDictionary<string, System.Collections.Immutable.ImmutableArray<byte>> expected,
        IReadOnlyDictionary<string, System.Collections.Immutable.ImmutableArray<byte>> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        return expected.All(pair => actual.TryGetValue(pair.Key, out var bytes)
            && pair.Value.AsSpan().SequenceEqual(bytes.AsSpan()));
    }

    private static ExtensionInstallApplicationPrecondition Stop(
        ExtensionInstallFindingCode code,
        string cause,
        string? target = null)
        => new(
            Validation: null,
            new ExtensionInstallFinding(code, cause, target));
}
