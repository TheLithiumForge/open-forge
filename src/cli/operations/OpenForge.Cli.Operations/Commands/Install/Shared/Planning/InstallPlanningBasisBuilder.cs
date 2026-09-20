using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallPlanningBasisBuilder
{
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal InstallPlanningBasisBuilder(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal InstallPlanningBasisResult Build(InstallInspectionFacts inspection)
    {
        ArgumentNullException.ThrowIfNull(inspection);

        var evidence = Evidence(inspection);
        FileExpectation agentsDirectoryExpectation;
        try
        {
            agentsDirectoryExpectation = ReadAgentsDirectoryExpectation(inspection);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            return Stopped(
                InstallManagementState.Incomplete,
                InstallFindingCode.WorkspaceUnavailable,
                "The .agents directory state is unavailable for exact planning.",
                evidence,
                SourceLogicalPath.AgentsRoot);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or NotSupportedException)
        {
            return Stopped(
                InstallManagementState.Blocked,
                InstallFindingCode.TargetUnsafe,
                $"The .agents directory state is unsafe for exact planning: {exception.Message}",
                evidence,
                SourceLogicalPath.AgentsRoot);
        }

        var context = new InstallPlanContext
        {
            Request = inspection.Request,
            Payload = inspection.Payload,
            IntendedState = inspection.IntendedState,
            AgentsDirectoryExpectation = agentsDirectoryExpectation,
        };
        return new InstallPlanningBasisCompleted(new InstallPlanningBasis(
            context,
            inspection.Ownership,
            inspection.CurrentTargets,
            inspection.CurrentFramework));
    }

    private FileExpectation ReadAgentsDirectoryExpectation(InstallInspectionFacts inspection)
    {
        var path = Path.Combine(
            inspection.Request.Workspace.LexicalRoot,
            SourceLogicalPath.AgentsRoot);
        var resolution = _physicalPathResolver.ResolveCandidate(
            inspection.Request.Workspace.LexicalRoot,
            inspection.Request.Workspace.PhysicalRoot,
            path);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return FileExpectation.Missing(path);
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            throw new InvalidDataException(
                resolution.Failure?.DirectCause
                    ?? "The .agents directory is not a contained ordinary directory.");
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        var attributes = File.GetAttributes(physicalPath);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            != FileAttributes.Directory)
        {
            throw new InvalidDataException("The .agents path is not an ordinary directory.");
        }

        return FileExpectation.Directory(path, physicalPath);
    }

    private static InstallPlanningBasisStopped Stopped(
        InstallManagementState state,
        InstallFindingCode code,
        string cause,
        PlanningEvidence evidence,
        string? subject = null)
        => new(new InstallPlanningBoundary(
            state,
            code,
            cause,
            subject,
            evidence.Payload,
            evidence.IntendedState));

    private static PlanningEvidence Evidence(InstallInspectionFacts inspection)
        => new(inspection.Payload, inspection.IntendedState);

    private sealed record PlanningEvidence(
        FrameworkPayload Payload,
        InstallIntendedState IntendedState);
}
