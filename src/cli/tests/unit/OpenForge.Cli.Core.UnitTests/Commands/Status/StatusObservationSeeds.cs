using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

internal static class StatusObservationSeeds
{
    internal static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-status-unit"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    internal static StatusRequest Request()
        => new(Workspace());

    internal static OperationalIntegerObservation Available(long value)
        => new(OperationalValueState.Available, value);

    internal static OperationalIntegerObservation Unavailable()
        => new(OperationalValueState.Unavailable, null);

    internal static OperationalIntegerObservation NotApplicable()
        => new(OperationalValueState.NotApplicable, null);

    internal static ContextMeasurementObservation Measurement(
        long files,
        long characters,
        long utf8Bytes,
        long estimatedTokens)
        => new(
            Available(files),
            Available(characters),
            Available(utf8Bytes),
            Available(estimatedTokens));
}
