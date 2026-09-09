using OpenForge.Cli.Core.Framework.Extensions.Shared.Manifest;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Planning;

internal sealed class ExtensionCreateDestinationInspector(PhysicalPathResolver physicalPathResolver)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal ExtensionCreateDestinationObservation Observe(
        string catalogue,
        string cataloguePhysicalIdentity,
        string destination,
        ReadOnlyMemory<byte> manifestBytes)
        => Observe(
            _physicalPathResolver.ResolveCandidate(
                lexicalWorkspaceRoot: catalogue,
                physicalWorkspaceRoot: cataloguePhysicalIdentity,
                candidatePath: destination),
            manifestBytes);

    internal ExtensionCreateDestinationObservation Observe(ExtensionCreatePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return Observe(
            _physicalPathResolver.ResolveCandidate(
                lexicalWorkspaceRoot: plan.Catalogue,
                physicalWorkspaceRoot: plan.CataloguePhysicalIdentity,
                candidatePath: plan.Destination),
            plan.ManifestBytes);
    }

    private static ExtensionCreateDestinationObservation Observe(
        PhysicalPathResolution resolution,
        ReadOnlyMemory<byte> manifestBytes)
        => resolution.State switch
        {
            PhysicalPathState.Missing => new(
                state: ExtensionCreateDestinationState.Absent,
                physicalIdentity: null,
                cause: null),
            PhysicalPathState.Contained => InspectExisting(
                resolution.GetContainedPhysicalPath(),
                manifestBytes),
            PhysicalPathState.External or PhysicalPathState.Dangling or PhysicalPathState.Cycle
                or PhysicalPathState.Unsupported => new(
                    state: ExtensionCreateDestinationState.Unsafe,
                    physicalIdentity: resolution.ResolvedPhysicalPath,
                    cause: Describe(resolution)),
            PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure => new(
                state: ExtensionCreateDestinationState.Unavailable,
                physicalIdentity: null,
                cause: Describe(resolution)),
            PhysicalPathState.Invalid => new(
                state: ExtensionCreateDestinationState.Collision,
                physicalIdentity: null,
                cause: Describe(resolution)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The destination physical state is not defined."),
        };

    private static ExtensionCreateDestinationObservation InspectExisting(
        string destinationPhysicalIdentity,
        ReadOnlyMemory<byte> manifestBytes)
    {
        try
        {
            if (!IsOrdinaryDirectory(destinationPhysicalIdentity)
                || !HasExactEntries(
                    destinationPhysicalIdentity,
                    [ExtensionPackageLayout.ManifestFileName, ExtensionPackageLayout.ContentDirectoryName]))
            {
                return Collision(destinationPhysicalIdentity, "The destination is not the exact scaffold.");
            }

            var manifestPath = Path.Combine(destinationPhysicalIdentity, ExtensionPackageLayout.ManifestFileName);
            var payloadPath = Path.Combine(destinationPhysicalIdentity, ExtensionPackageLayout.ContentDirectoryName);
            var agentsPath = Path.Combine(payloadPath, ExtensionCreateDefinitions.AgentsDirectoryName);
            if (!IsOrdinaryFile(manifestPath)
                || !IsOrdinaryDirectory(payloadPath)
                || !HasExactEntries(payloadPath, [ExtensionCreateDefinitions.AgentsDirectoryName])
                || !IsOrdinaryDirectory(agentsPath)
                || Directory.EnumerateFileSystemEntries(agentsPath).Any())
            {
                return Collision(destinationPhysicalIdentity, "The destination contains a partial, additional, or unsafe scaffold entry.");
            }

            var actualBytes = File.ReadAllBytes(manifestPath);
            if (!actualBytes.AsSpan().SequenceEqual(manifestBytes.Span))
            {
                return Collision(destinationPhysicalIdentity, "The existing manifest bytes differ from the intended manifest.");
            }

            _ = ExtensionManifestReader.Read(actualBytes, manifestPath, []);
            return new ExtensionCreateDestinationObservation(
                state: ExtensionCreateDestinationState.Exact,
                physicalIdentity: destinationPhysicalIdentity,
                cause: null);
        }
        catch (Exception exception) when (IsUnavailable(exception))
        {
            return new ExtensionCreateDestinationObservation(
                state: ExtensionCreateDestinationState.Unavailable,
                physicalIdentity: destinationPhysicalIdentity,
                cause: exception.Message);
        }
        catch (JsonException exception)
        {
            return Collision(destinationPhysicalIdentity, exception.Message);
        }
    }

    private static bool HasExactEntries(string directory, IReadOnlyList<string> expected)
    {
        var actual = Directory
            .EnumerateFileSystemEntries(directory)
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return actual.SequenceEqual(expected.Order(StringComparer.Ordinal), StringComparer.Ordinal);
    }

    private static bool IsOrdinaryDirectory(string path)
    {
        var attributes = File.GetAttributes(path);
        return (attributes & FileAttributes.Directory) != 0
            && (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) == 0;
    }

    private static bool IsOrdinaryFile(string path)
    {
        var attributes = File.GetAttributes(path);
        return (attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) == 0;
    }

    private static ExtensionCreateDestinationObservation Collision(string? physicalIdentity, string cause)
        => new(
            state: ExtensionCreateDestinationState.Collision,
            physicalIdentity: physicalIdentity,
            cause: cause);

    private static string Describe(PhysicalPathResolution resolution)
        => resolution.Failure?.DirectCause ?? $"The path resolved as {resolution.State}.";

    private static bool IsUnavailable(Exception exception)
        => exception is IOException
            or UnauthorizedAccessException
            or NotSupportedException
            or ArgumentException;
}
