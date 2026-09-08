using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Resolution;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Planning;

internal sealed partial class ExtensionCreatePlanner
{
    private static ExtensionCreatePlanningOutcome RootFailure(
        ExtensionCreateResolvedRequest request,
        string catalogue,
        PhysicalPathResolution resolution)
    {
        var (status, code) = resolution.State switch
        {
            PhysicalPathState.Missing or PhysicalPathState.Invalid => (
                CliSemanticStatus.Invalid,
                ExtensionCreateFindingCode.InvalidInput),
            PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure => (
                CliSemanticStatus.Incomplete,
                ExtensionCreateFindingCode.CatalogueUnavailable),
            PhysicalPathState.Dangling or PhysicalPathState.External or PhysicalPathState.Cycle
                or PhysicalPathState.Unsupported => (
                    CliSemanticStatus.Blocked,
                    ExtensionCreateFindingCode.CatalogueUnsafe),
            PhysicalPathState.Contained => throw new ArgumentException(
                "A contained catalogue cannot form a root failure.",
                nameof(resolution)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The catalogue physical state is not defined."),
        };
        return Terminal(request, status, code, catalogue, Describe(resolution));
    }

    internal static ExtensionCreatePlanningOutcome DestinationFailure(
        ExtensionCreateResolvedRequest request,
        string catalogue,
        string destination,
        ExtensionCreateDestinationObservation observation)
    {
        var (status, code) = observation.State switch
        {
            ExtensionCreateDestinationState.Unavailable => (
                CliSemanticStatus.Incomplete,
                ExtensionCreateFindingCode.CatalogueUnavailable),
            ExtensionCreateDestinationState.Unsafe => (
                CliSemanticStatus.Blocked,
                ExtensionCreateFindingCode.CatalogueUnsafe),
            ExtensionCreateDestinationState.Collision => (
                CliSemanticStatus.Blocked,
                ExtensionCreateFindingCode.DestinationCollision),
            ExtensionCreateDestinationState.Absent => throw new InvalidOperationException(
                "An absent destination cannot form a destination failure."),
            ExtensionCreateDestinationState.Exact => throw new InvalidOperationException(
                "An exact destination cannot form a destination failure."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(observation),
                observation.State,
                "The destination state is not defined."),
        };
        var finding = ExtensionCreateResultFactory.Finding(
            code: code,
            status: status,
            subject: destination,
            cause: observation.Cause ?? "The destination is not eligible for creation.");
        return ExtensionCreatePlanningOutcome.Terminal(
            ExtensionCreateResultFactory.Create(
                request,
                new ExtensionCreateResultOutcome
                {
                    Status = status,
                    Catalogue = catalogue,
                    Destination = destination,
                    IntendedEffects = CreateEffects(destination),
                    Verification = new ExtensionCreateVerification
                    {
                        Catalogue = ExtensionCreateVerificationState.Verified,
                        Destination = ExtensionCreateVerificationState.Failed,
                        Manifest = ExtensionCreateVerificationState.NotStarted,
                        Payload = ExtensionCreateVerificationState.NotStarted,
                        Cause = finding.Cause,
                    },
                    Finding = finding,
                }));
    }

    private static ExtensionCreatePlanningOutcome Terminal(
        ExtensionCreateResolvedRequest request,
        CliSemanticStatus status,
        ExtensionCreateFindingCode code,
        string? subject,
        string cause)
    {
        var finding = ExtensionCreateResultFactory.Finding(
            code: code,
            status: status,
            subject: subject,
            cause: cause);
        return ExtensionCreatePlanningOutcome.Terminal(
            ExtensionCreateResultFactory.Create(
                request,
                new ExtensionCreateResultOutcome
                {
                    Status = status,
                    Catalogue = subject,
                    Verification = new ExtensionCreateVerification
                    {
                        Catalogue = ExtensionCreateVerificationState.Failed,
                        Destination = ExtensionCreateVerificationState.NotStarted,
                        Manifest = ExtensionCreateVerificationState.NotStarted,
                        Payload = ExtensionCreateVerificationState.NotStarted,
                        Cause = cause,
                    },
                    Finding = finding,
                }));
    }

    private static ExtensionCreateEffect[] CreateEffects(string destination)
        =>
        [
            new ExtensionCreateEffect
            {
                Kind = ExtensionCreateEffectKind.ManifestFile,
                Path = Path.Combine(destination, ExtensionCreateDefinitions.ManifestFileName),
            },
            new ExtensionCreateEffect
            {
                Kind = ExtensionCreateEffectKind.PayloadAgentsDirectory,
                Path = Path.Combine(
                    destination,
                    ExtensionPackageLayout.ContentDirectoryName,
                    ExtensionCreateDefinitions.AgentsDirectoryName) + Path.DirectorySeparatorChar,
            },
        ];

    private static ExtensionCreateFinding Changed(string subject, string cause)
        => ExtensionCreateResultFactory.Finding(
            code: ExtensionCreateFindingCode.DestinationChanged,
            status: CliSemanticStatus.Blocked,
            subject: subject,
            cause: cause);

    private static bool IsOrdinaryDirectory(string path)
    {
        var attributes = File.GetAttributes(path);
        return (attributes & FileAttributes.Directory) != 0
            && (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) == 0;
    }

    private static bool IsUnavailable(PhysicalPathState state)
        => state is PhysicalPathState.Inaccessible
            or PhysicalPathState.InputOutputFailure;

    private static bool IsUnavailable(Exception exception)
        => exception is IOException or UnauthorizedAccessException or NotSupportedException;

    private static bool IsInvalidPath(Exception exception)
        => exception is ArgumentException or PathTooLongException or NotSupportedException;

    private static string Describe(PhysicalPathResolution resolution)
        => resolution.Failure?.DirectCause ?? $"The path resolved as {resolution.State}.";
}
