using OpenForge.Cli.Core.Commands.Extension.Shared.Recovery;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallFoundationReader(
    PhysicalPathResolver physicalPathResolver)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly ExtensionInstallTopologyBuilder _topologyBuilder = new();

    internal async ValueTask<ExtensionInstallFoundationObservation> ReadAsync(
        ExtensionInstallRequest request,
        IReadOnlyList<ExtensionPackageFact> packages,
        WorkspaceLayoutRead layoutRead,
        CancellationToken cancellationToken)
    {
        var layout = layoutRead.Layout ?? throw new InvalidOperationException("Extension Install requires its observed layout.");
        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        if (payloadRead.State != FrameworkPayloadReadState.Available
            || payloadRead.Payload is not { } frameworkPayload)
        {
            return Stop(
                ExtensionInstallFindingCode.FrameworkUnavailable,
                payloadRead.Cause ?? "The running Framework inventory is unavailable.");
        }

        var container = Path.Combine(request.Workspace.LexicalRoot, layout.MainRoot.Replace('/', Path.DirectorySeparatorChar));
        var boundary = _physicalPathResolver.ResolveCandidate(request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot, container);
        if (boundary.State == PhysicalPathState.Missing)
        {
            return Stop(ExtensionInstallFindingCode.FrameworkUnavailable,
                $"The {layout.MainRoot} Framework container is missing.", layout.MainRoot);
        }
        if (boundary.State != PhysicalPathState.Contained
            || !Directory.Exists(boundary.GetContainedPhysicalPath())
            || (File.GetAttributes(container) & FileAttributes.ReparsePoint) != 0)
        {
            return Stop(ExtensionInstallFindingCode.FrameworkUnsafe,
                $"The {layout.MainRoot} Framework container is not a contained ordinary directory.", layout.MainRoot);
        }

        var ownership = layoutRead.Ownership;

        ExtensionInstallTopologyBuild topologyBuild;
        try
        {
            topologyBuild = await _topologyBuilder.BuildWithFindingsAsync(
                    layout,
                    packages,
                    layoutRead.Settings.Document,
                    cancellationToken, ownership)
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


        if (ownership.Document.Extensions.GroupBy(extension => extension.Id, StringComparer.Ordinal)
            .Any(group => group.Count() > 1))
        {
            return Stop(ExtensionInstallFindingCode.LifecycleObservation,
                "Recorded Extension identities are duplicated; no ownership or file effects were inferred.");
        }

        var stores = await RecoveryBundleCatalogue.ReadStoresAsync(
            request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (stores.Any(store => store.Catalogue.State == RecoveryBundleCatalogueState.Cancelled))
        {
            return Stop(ExtensionInstallFindingCode.Interrupted,
                "Extension Install recovery inspection was interrupted.");
        }

        var historyFindings = stores.SelectMany(store => store.Catalogue.Candidates)
            .Where(ExtensionRecoveryHistory.RequiresWarning).Select(candidate => new ExtensionInstallFinding(
            ExtensionInstallFindingCode.RecoveryHistory,
            "Recovery data from an earlier run remains.", candidate.Path)).ToList();
        foreach (var store in stores.Where(store => store.Catalogue.State == RecoveryBundleCatalogueState.Unavailable))
        {
            historyFindings.Add(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.RecoveryCoverageUnavailable,
                store.Catalogue.Cause ?? "Recovery history could not be checked.",
                store.Location?.WorkspaceDirectory));
        }

        return new ExtensionInstallFoundationObservation(
            new ExtensionInstallFoundation(
                frameworkPayload,
                ownership,
                topologyBuild.Topology,
                topologyBuild.Findings)
            {
                HistoryFindings = historyFindings.ToArray(),
            },
            Finding: null);
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
