using OpenForge.Cli.Core.Framework.Filesystem.Shared.Reading;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using System.Text;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;

using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational;

internal sealed class ExtensionLifecycleTargetReader(
    PhysicalPathResolver physicalPathResolver)
{
    private readonly FrameworkContentIdentity _contentIdentity = new();
    private readonly ManagedTargetReader _managedTargetReader = new(physicalPathResolver);

    internal async ValueTask<IReadOnlyList<ExtensionManagedTargetObservation>> ReadAsync(
        CliWorkspace workspace,
        WorkspaceOwnershipDocument ownership,
        IReadOnlyList<ExtensionSourceObservation> sources,
        CancellationToken cancellationToken)
    {
        var detailed = await ReadDoctorAsync(
            workspace,
            ownership,
            sources,
            cancellationToken).ConfigureAwait(false);
        return detailed.Select(observation => observation.Target).ToArray();
    }

    internal async ValueTask<IReadOnlyList<ExtensionManagedTargetDoctorObservation>> ReadDoctorAsync(
        CliWorkspace workspace,
        WorkspaceOwnershipDocument ownership,
        IReadOnlyList<ExtensionSourceObservation> sources,
        CancellationToken cancellationToken)
    {
        var observations = new List<ExtensionManagedTargetDoctorObservation>();
        foreach (var path in ownership.Extensions.SelectMany(package => package.Paths).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var intended = ReadIntendedFingerprint(path, ownership, sources);
            var read = await ReadStateAsync(
                workspace,
                path,
                intended,
                cancellationToken).ConfigureAwait(false);
            var observation = new ExtensionManagedTargetObservation
            {
                Path = path,
                Owners = ownership.OwnersOf(path),
                IntendedFingerprint = intended,
                Cause = read.Cause,
                State = read.State,
            };
            observations.Add(read.ReadState == ManagedTargetReadState.Available
                ? ExtensionManagedTargetDoctorObservation.Observed(
                    observation,
                    read.CurrentFingerprint
                        ?? throw new InvalidOperationException(
                            "An observed Extension target requires its current fingerprint."))
                : ExtensionManagedTargetDoctorObservation.Boundary(
                    observation,
                    read.ReadState,
                    read.Cause,
                    read.UnavailableReason));
        }

        return observations.ToArray();
    }

    private async ValueTask<ExtensionManagedTargetDoctorRead> ReadStateAsync(
        CliWorkspace workspace,
        string path,
        string? intendedFingerprint,
        CancellationToken cancellationToken)
    {
        var read = await _managedTargetReader
            .ReadAsync(workspace, path, cancellationToken)
            .ConfigureAwait(false);
        if (read.State != ManagedTargetReadState.Available)
        {
            return ExtensionManagedTargetDoctorRead.Boundary(
                read.State,
                cancellationToken);
        }

        if (intendedFingerprint is null)
        {
            return ExtensionManagedTargetDoctorRead.Unavailable(
                "The current package sources do not identify one intended target.",
                ExtensionManagedTargetUnavailableReason.IntendedComparisonUnavailable);
        }

        try
        {
            var fingerprint = ReadFingerprint(read.Bytes.Span, path);
            var state = string.Equals(fingerprint, intendedFingerprint, StringComparison.Ordinal)
                ? OperationalTargetState.Current : OperationalTargetState.Changed;
            return ExtensionManagedTargetDoctorRead.Observed(state, fingerprint);
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException
            or InvalidOperationException)
        {
            return ExtensionManagedTargetDoctorRead.Blocked(exception.Message);
        }
    }

    private string? ReadIntendedFingerprint(
        string path, WorkspaceOwnershipDocument ownership, IReadOnlyList<ExtensionSourceObservation> sources)
    {
        string? intended = null;
        foreach (var owner in ownership.Extensions.Where(package => package.Paths.Contains(path, StringComparer.Ordinal)))
        {
            var matchingSources = sources.Where(source => string.Equals(source.RecordedSource, owner.Source,
                StringComparison.Ordinal)).ToArray();
            if (matchingSources.Length != 1 || matchingSources[0].Read.State != ExtensionSourceReadState.Complete)
            {
                return null;
            }
            var packages = matchingSources[0].Read.Packages.Where(package => package.Id == owner.Id).ToArray();
            if (packages.Length != 1)
            {
                return null;
            }
            var files = packages[0].Payload.Where(file => file.TargetPath == path).ToArray();
            if (files.Length != 1 || files[0].State != ExtensionPackageFileReadState.Available
                || files[0].Bytes is not { } bytes)
            {
                return null;
            }
            string fingerprint;
            try
            {
                fingerprint = ReadFingerprint(bytes.Span, path);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidDataException or InvalidOperationException)
            {
                return null;
            }
            if (intended is not null && intended != fingerprint)
            {
                return null;
            }
            intended = fingerprint;
        }
        return intended;
    }

    private string ReadFingerprint(ReadOnlySpan<byte> bytes, string path)
    {
        if (!path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            return OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files.FileExpectation.Hash(bytes);
        }
        var facts = _contentIdentity.ReadSourceFingerprint(bytes);
        return facts.IsSemantic && facts.Sha256 is { } fingerprint ? fingerprint
            : throw new InvalidDataException(facts.Cause ?? "The Markdown target cannot be compared semantically.");
    }

}
