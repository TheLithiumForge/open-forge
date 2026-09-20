using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;

namespace OpenForge.Cli.Core.Commands.Extension.List;

internal static class ExtensionListOperationFactory
{
    internal static ExtensionListOperation Create()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var sourceReader = new ExtensionSourceReader(physicalPathResolver);
        return new(
            sourceReader,
            physicalPathResolver,
            new ExtensionLifecycleDoctorReader(
                new ExtensionSourceObservationReader(sourceReader),
                new ExtensionLifecycleTargetReader(physicalPathResolver),
                new ExtensionBridgeRegistrationObservationReader()));
    }
}
