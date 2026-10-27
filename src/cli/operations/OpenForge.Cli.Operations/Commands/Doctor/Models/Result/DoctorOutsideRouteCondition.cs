namespace OpenForge.Cli.Core.Commands.Doctor.Models.Result;

internal enum DoctorOutsideRouteState
{
    MissingDirectory,
    MissingAnchor,
    Unlisted,
    UnsafeBoundary,
    PhysicalAlias,
    Unavailable,
    AmbiguousAnchor,
    StaleLoaderTarget,
}

internal sealed record DoctorOutsideRouteCondition(DoctorOutsideRouteState State, string? Cause = null, string? InitTarget = null);
