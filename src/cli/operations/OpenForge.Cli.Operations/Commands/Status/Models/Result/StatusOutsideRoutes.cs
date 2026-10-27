namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal enum StatusOutsideRouteState
{
    Available,
    Missing,
    Unsafe,
    Unavailable,
}

internal sealed record StatusOutsideRoute(string Path, StatusOutsideRouteState State, bool? Listed);

internal static class StatusOutsideRouteStates
{
    internal const string Available = "available";
    internal const string Missing = "missing";
    internal const string Unsafe = "unsafe";
    internal const string Unavailable = "unavailable";

    internal static string Read(StatusOutsideRouteState state) => state switch
    {
        StatusOutsideRouteState.Available => Available,
        StatusOutsideRouteState.Missing => Missing,
        StatusOutsideRouteState.Unsafe => Unsafe,
        StatusOutsideRouteState.Unavailable => Unavailable,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The outside route state is not defined."),
    };
}
