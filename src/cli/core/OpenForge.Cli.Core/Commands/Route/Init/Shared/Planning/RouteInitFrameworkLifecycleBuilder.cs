using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal enum RouteInitFrameworkTrustState
{
    Current,
    InstallRequired,
    UpdateRequired,
    Incomplete,
    Blocked,
    Cancelled,
}

internal sealed record RouteInitFrameworkTrust(
    RouteInitFrameworkTrustState State,
    LifecycleStoreReadResult Read,
    LifecycleWritePlanResult? SameReadGate,
    FrameworkLifecycleCurrentness? Currentness,
    string? Cause)
{
    internal bool IsCurrent => State == RouteInitFrameworkTrustState.Current;
}

internal sealed record RouteInitFrameworkManagedState
{
    internal RouteInitFrameworkManagedState(
        string path,
        string? sourceAssetPath,
        ReadOnlySpan<byte> finalBytes,
        bool ownsGeneratedEntries)
    {
        if (!SourceLogicalPath.IsCanonicalSource(path))
        {
            throw new ArgumentException("A scoped Framework lifecycle path must be canonical.", nameof(path));
        }

        if (sourceAssetPath is not null && !SourceLogicalPath.IsCanonicalSource(sourceAssetPath))
        {
            throw new ArgumentException("A scoped Framework lifecycle source asset path must be canonical.", nameof(sourceAssetPath));
        }

        if (sourceAssetPath is null && !ownsGeneratedEntries)
        {
            throw new ArgumentException(
                "A scoped Framework lifecycle addition must own a whole source or its generated Entries region.",
                nameof(ownsGeneratedEntries));
        }

        Path = path;
        SourceAssetPath = sourceAssetPath;
        FinalBytes = ImmutableArray.CreateRange(finalBytes.ToArray());
        OwnsGeneratedEntries = ownsGeneratedEntries;
    }

    internal string Path { get; }

    internal string? SourceAssetPath { get; }

    internal ImmutableArray<byte> FinalBytes { get; }

    internal bool OwnsGeneratedEntries { get; }
}

internal enum RouteInitFrameworkLifecyclePlanState
{
    Complete,
    Blocked,
}

internal sealed record RouteInitFrameworkLifecyclePlan(
    RouteInitFrameworkLifecyclePlanState State,
    FrameworkLifecycleState? Intended,
    LifecycleWritePlanResult? WritePlan,
    string? Cause);

internal sealed class RouteInitFrameworkLifecycleBuilder
{
    private const string GeneratedEntriesRegion = "entries";
    private readonly FrameworkContentIdentity _contentIdentity = new();
    private readonly FrameworkLifecycleCurrentnessReader _currentnessReader;
    private readonly LifecycleStore _store;

    internal RouteInitFrameworkLifecycleBuilder()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        _store = new LifecycleStore(physicalPathResolver);
        _currentnessReader = new FrameworkLifecycleCurrentnessReader(physicalPathResolver);
    }

    internal async ValueTask<RouteInitFrameworkTrust> ReadTrustAsync(
        CliWorkspace workspace,
        FrameworkPayload payload,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(payload);
        var read = await _store.ReadAsync(
                workspace,
                LifecycleSection.Framework,
                cancellationToken)
            .ConfigureAwait(false);
        if (read.State != LifecycleStoreReadState.Available || read.Framework is not { } framework)
        {
            return FromReadBoundary(read);
        }

        var gate = _store.PlanFrameworkUpdate(read, framework);
        if (gate.State != LifecycleWritePlanState.Unchanged)
        {
            return new RouteInitFrameworkTrust(
                RouteInitFrameworkTrustState.Blocked,
                read,
                gate,
                Currentness: null,
                gate.Cause ?? "The lifecycle envelope is not one complete same-read Framework basis.");
        }

        var currentness = await _currentnessReader.ReadAsync(
                workspace,
                framework,
                payload,
                cancellationToken)
            .ConfigureAwait(false);
        return FromCurrentness(read, gate, currentness);
    }

    internal RouteInitFrameworkLifecyclePlan BuildPlan(
        RouteInitFrameworkTrust trust,
        FrameworkPayload payload,
        IEnumerable<RouteInitFrameworkManagedState> additions)
    {
        ArgumentNullException.ThrowIfNull(trust);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(additions);
        if (!trust.IsCurrent || trust.Read.Framework is not { } current)
        {
            return Blocked(trust.Cause ?? "A scoped Framework lifecycle plan requires trusted current Install state.");
        }

        try
        {
            var targets = current.Targets.ToDictionary(
                target => (target.Path, target.Region),
                target => target);
            var generated = current.GeneratedRegions.ToDictionary(
                region => (region.Path, region.Region),
                region => region);
            foreach (var addition in additions.OrderBy(value => value.Path, StringComparer.Ordinal))
            {
                var sourceAssetPath = addition.SourceAssetPath;
                if (sourceAssetPath is not null
                    && payload.Find(sourceAssetPath) is null)
                {
                    return Blocked(
                        $"The scoped Framework source '{sourceAssetPath}' is not present in the running embedded inventory.");
                }

                if (sourceAssetPath is not null)
                {
                    targets.TryAdd(
                        (addition.Path, Region: (string?)null),
                        new FrameworkLifecycleTarget
                        {
                            Path = addition.Path,
                            SourceAssetPath = sourceAssetPath,
                            Region = null,
                            BaselineFingerprint = _contentIdentity.ReadSourceFingerprint(
                                addition.FinalBytes.AsSpan(),
                                LifecycleSchema.SemanticFingerprintKind),
                            FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
                        });
                }

                if (!addition.OwnsGeneratedEntries)
                {
                    continue;
                }

                targets[(addition.Path, GeneratedEntriesRegion)] = new FrameworkLifecycleTarget
                {
                    Path = addition.Path,
                    SourceAssetPath = null,
                    Region = GeneratedEntriesRegion,
                    BaselineFingerprint = _contentIdentity.ReadGeneratedEntriesFingerprint(
                        addition.FinalBytes.AsSpan(),
                        LifecycleSchema.ExactBytesFingerprintKind),
                    FingerprintKind = LifecycleSchema.ExactBytesFingerprintKind,
                };
                generated.TryAdd(
                    (addition.Path, GeneratedEntriesRegion),
                    new FrameworkGeneratedRegion
                    {
                        Path = addition.Path,
                        Region = GeneratedEntriesRegion,
                    });
            }

            var intended = new FrameworkLifecycleState
            {
                Coverage = current.Coverage,
                Source = current.Source,
                Targets = targets.Values
                    .OrderBy(target => target.Path, StringComparer.Ordinal)
                    .ThenBy(target => target.Region, StringComparer.Ordinal)
                    .ToArray(),
                GeneratedRegions = generated.Values
                    .OrderBy(region => region.Path, StringComparer.Ordinal)
                    .ThenBy(region => region.Region, StringComparer.Ordinal)
                    .ToArray(),
            };
            var writePlan = _store.PlanFrameworkUpdate(trust.Read, intended);
            return writePlan.State == LifecycleWritePlanState.Blocked
                ? Blocked(writePlan.Cause ?? "The scoped Framework lifecycle update is blocked.")
                : new RouteInitFrameworkLifecyclePlan(
                    RouteInitFrameworkLifecyclePlanState.Complete,
                    intended,
                    writePlan,
                    Cause: null);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or InvalidOperationException)
        {
            return Blocked($"The scoped Framework lifecycle plan is invalid: {exception.Message}");
        }
    }

    private static RouteInitFrameworkTrust FromReadBoundary(LifecycleStoreReadResult read)
    {
        var state = read.State switch
        {
            LifecycleStoreReadState.DocumentMissing => RouteInitFrameworkTrustState.InstallRequired,
            LifecycleStoreReadState.SectionMissing => RouteInitFrameworkTrustState.Blocked,
            LifecycleStoreReadState.Unavailable => RouteInitFrameworkTrustState.Incomplete,
            LifecycleStoreReadState.Cancelled => RouteInitFrameworkTrustState.Cancelled,
            LifecycleStoreReadState.Invalid
                or LifecycleStoreReadState.Blocked => RouteInitFrameworkTrustState.Blocked,
            LifecycleStoreReadState.Available => RouteInitFrameworkTrustState.Blocked,
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.State,
                "The lifecycle read state is not defined."),
        };
        return new RouteInitFrameworkTrust(
            state,
            read,
            SameReadGate: null,
            Currentness: null,
            read.Cause);
    }

    private static RouteInitFrameworkTrust FromCurrentness(
        LifecycleStoreReadResult read,
        LifecycleWritePlanResult gate,
        FrameworkLifecycleCurrentness currentness)
    {
        var state = currentness.State switch
        {
            FrameworkLifecycleCurrentnessState.Current => RouteInitFrameworkTrustState.Current,
            FrameworkLifecycleCurrentnessState.SourceMismatch
                or FrameworkLifecycleCurrentnessState.Changed
                or FrameworkLifecycleCurrentnessState.Missing => RouteInitFrameworkTrustState.UpdateRequired,
            FrameworkLifecycleCurrentnessState.Unavailable => RouteInitFrameworkTrustState.Incomplete,
            FrameworkLifecycleCurrentnessState.Blocked => RouteInitFrameworkTrustState.Blocked,
            FrameworkLifecycleCurrentnessState.Cancelled => RouteInitFrameworkTrustState.Cancelled,
            _ => throw new ArgumentOutOfRangeException(
                nameof(currentness),
                currentness.State,
                "The Framework lifecycle currentness state is not defined."),
        };
        return new RouteInitFrameworkTrust(
            state,
            read,
            gate,
            currentness,
            currentness.Cause);
    }

    private static RouteInitFrameworkLifecyclePlan Blocked(string cause)
        => new(RouteInitFrameworkLifecyclePlanState.Blocked, Intended: null, WritePlan: null, cause);
}
