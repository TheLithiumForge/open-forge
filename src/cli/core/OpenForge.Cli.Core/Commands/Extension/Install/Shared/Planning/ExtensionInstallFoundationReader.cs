using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Identity;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallFoundationReader(
    LifecycleStore lifecycleStore,
    FrameworkLifecycleCurrentnessReader frameworkCurrentness)
{
    private readonly LifecycleStore _lifecycleStore = lifecycleStore;
    private readonly FrameworkLifecycleCurrentnessReader _frameworkCurrentness = frameworkCurrentness;
    private readonly ExtensionInstallTopologyBuilder _topologyBuilder = new();

    internal async ValueTask<ExtensionInstallFoundationObservation> ReadAsync(
        ExtensionInstallRequest request,
        IReadOnlyList<ExtensionPackageFact> packages,
        CancellationToken cancellationToken)
    {
        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        if (payloadRead.State != FrameworkPayloadReadState.Available
            || payloadRead.Payload is not { } frameworkPayload)
        {
            return Stop(
                ExtensionInstallFindingCode.FrameworkUnavailable,
                payloadRead.Cause ?? "The running Framework inventory is unavailable.");
        }

        var frameworkRead = await _lifecycleStore.ReadAsync(
            request.Workspace,
            LifecycleSection.Framework,
            cancellationToken).ConfigureAwait(false);
        if (frameworkRead.State != LifecycleStoreReadState.Available
            || frameworkRead.Framework is not { } frameworkLifecycle)
        {
            return Stop(ExtensionInstallResultFactory.LifecycleFinding(
                frameworkRead,
                ExtensionInstallFindingCode.FrameworkUnavailable,
                ExtensionInstallFindingCode.FrameworkUnsafe));
        }

        ExtensionInstallTopology topology;
        try
        {
            topology = await _topologyBuilder.BuildAsync(request, packages, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                ExtensionInstallFindingCode.Interrupted,
                "Extension Install topology planning was interrupted.");
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException
            or IOException)
        {
            return Stop(
                ExtensionInstallFindingCode.GeneratedRegionUnsafe,
                exception.Message);
        }

        var currentness = await _frameworkCurrentness.ReadAsync(
            request.Workspace,
            frameworkLifecycle,
            frameworkPayload,
            cancellationToken).ConfigureAwait(false);
        if (currentness.State == FrameworkLifecycleCurrentnessState.Changed
            && frameworkLifecycle.GeneratedRegions.Any(region =>
                string.Equals(region.Path, currentness.Path, StringComparison.Ordinal)))
        {
            try
            {
                currentness = await _frameworkCurrentness.ReadAsync(
                    request.Workspace,
                    WithGeneratedTopology(frameworkLifecycle, topology),
                    frameworkPayload,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is ArgumentException
                or DecoderFallbackException
                or InvalidDataException
                or InvalidOperationException)
            {
                return Stop(
                    ExtensionInstallFindingCode.FrameworkUnsafe,
                    $"The intended generated Framework topology is invalid: {exception.Message}",
                    currentness.Path);
            }
        }

        if (currentness.State != FrameworkLifecycleCurrentnessState.Current)
        {
            return Stop(
                currentness.State switch
                {
                    FrameworkLifecycleCurrentnessState.Cancelled => ExtensionInstallFindingCode.Interrupted,
                    FrameworkLifecycleCurrentnessState.Unavailable
                        or FrameworkLifecycleCurrentnessState.Missing => ExtensionInstallFindingCode.FrameworkUnavailable,
                    FrameworkLifecycleCurrentnessState.SourceMismatch
                        or FrameworkLifecycleCurrentnessState.Changed
                        or FrameworkLifecycleCurrentnessState.Blocked => ExtensionInstallFindingCode.FrameworkUnsafe,
                    FrameworkLifecycleCurrentnessState.Current => throw new InvalidOperationException(
                        "A current Framework lifecycle has no failure boundary."),
                    _ => throw new ArgumentOutOfRangeException(
                        null,
                        currentness.State,
                        "The Framework lifecycle currentness state is not defined."),
                },
                currentness.Cause ?? "The installed Framework anchor is not current.",
                currentness.Path);
        }

        var extensionsRead = await _lifecycleStore.ReadAsync(
            request.Workspace,
            LifecycleSection.Extensions,
            cancellationToken).ConfigureAwait(false);
        if (extensionsRead.State != LifecycleStoreReadState.Available
            || extensionsRead.Extensions is not { } currentExtensions)
        {
            return Stop(ExtensionInstallResultFactory.LifecycleFinding(
                extensionsRead,
                ExtensionInstallFindingCode.LifecycleUnavailable,
                ExtensionInstallFindingCode.LifecycleBlocked));
        }

        var recovery = await RecoveryBundleCatalogue.ReadAsync(
            request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (recovery.State != RecoveryBundleCatalogueState.Available
            || recovery.Candidates.Length > 0)
        {
            return Stop(
                recovery.State switch
                {
                    RecoveryBundleCatalogueState.Cancelled => ExtensionInstallFindingCode.Interrupted,
                    RecoveryBundleCatalogueState.Available => ExtensionInstallFindingCode.RecoveryConflict,
                    RecoveryBundleCatalogueState.Unavailable => ExtensionInstallFindingCode.RecoveryUnavailable,
                    _ => throw new ArgumentOutOfRangeException(
                        null,
                        recovery.State,
                        "The recovery catalogue state is not defined."),
                },
                recovery.Cause ?? "Recognized recovery residuals block Extension Install.");
        }

        return new ExtensionInstallFoundationObservation(
            new ExtensionInstallFoundation(
                frameworkPayload,
                frameworkLifecycle,
                extensionsRead,
                currentExtensions,
                topology),
            Finding: null);
    }

    internal static FrameworkLifecycleState WithGeneratedTopology(
        FrameworkLifecycleState lifecycle,
        ExtensionInstallTopology topology)
    {
        var contentIdentity = new FrameworkContentIdentity();
        var generated = lifecycle.GeneratedRegions
            .Select(region => (region.Path, region.Region))
            .ToHashSet();
        var targets = lifecycle.Targets.Select(target =>
        {
            if (target.Region is not { } region
                || !generated.Contains((target.Path, region)))
            {
                return new FrameworkLifecycleTarget
                {
                    Path = target.Path,
                    SourceAssetPath = target.SourceAssetPath,
                    Region = target.Region,
                    BaselineFingerprint = target.BaselineFingerprint,
                    FingerprintKind = target.FingerprintKind,
                };
            }

            System.Collections.Immutable.ImmutableArray<byte> bytes;
            if (topology.IntendedTargetBytes.TryGetValue(target.Path, out var intended))
            {
                bytes = intended;
            }
            else if (topology.GeneratedTargetBytes.TryGetValue(target.Path, out var projected))
            {
                bytes = projected;
            }
            else
            {
                throw new InvalidDataException(
                    $"Generated Framework target '{target.Path}' is absent from the intended topology.");
            }
            return new FrameworkLifecycleTarget
            {
                Path = target.Path,
                SourceAssetPath = target.SourceAssetPath,
                Region = target.Region,
                BaselineFingerprint = contentIdentity.ReadGeneratedEntriesFingerprint(
                    bytes.AsSpan(),
                    target.FingerprintKind),
                FingerprintKind = target.FingerprintKind,
            };
        }).ToArray();
        return new FrameworkLifecycleState
        {
            Coverage = lifecycle.Coverage,
            Source = new FrameworkLifecycleSource
            {
                Id = lifecycle.Source.Id,
                Version = lifecycle.Source.Version,
                InventoryFingerprint = lifecycle.Source.InventoryFingerprint,
            },
            Targets = targets,
            GeneratedRegions = [.. lifecycle.GeneratedRegions.Select(region => new FrameworkGeneratedRegion
            {
                Path = region.Path,
                Region = region.Region,
            })],
        };
    }

    private static ExtensionInstallFoundationObservation Stop(
        ExtensionInstallFindingCode code,
        string cause,
        string? target = null)
        => Stop(new ExtensionInstallFinding(code, cause, target));

    private static ExtensionInstallFoundationObservation Stop(
        ExtensionInstallFinding finding)
        => new(Foundation: null, finding);

    internal static string SourceSignature(ExtensionSourceReadResult source)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Append(source.Identity);
        foreach (var package in source.Packages.OrderBy(value => value.Id, StringComparer.Ordinal))
        {
            Append(package.Id);
            Append(package.Version);
            foreach (var dependency in package.Dependencies.Order(StringComparer.Ordinal))
            {
                Append(dependency);
            }

            foreach (var file in package.Payload.OrderBy(value => value.TargetPath, StringComparer.Ordinal))
            {
                Append(file.TargetPath ?? string.Empty);
                Append(file.Sha256 ?? string.Empty);
            }
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());

        void Append(string value)
            => hash.AppendData(Encoding.UTF8.GetBytes($"{value}\n"));
    }
}
