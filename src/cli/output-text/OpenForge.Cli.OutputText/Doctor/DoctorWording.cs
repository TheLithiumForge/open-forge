using System.Globalization;

namespace OpenForge.Cli.OutputText.Doctor;

internal static class DoctorWording
{
    internal static string OwnershipObservation(string what)
        => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.OwnershipObservation(what);
}
