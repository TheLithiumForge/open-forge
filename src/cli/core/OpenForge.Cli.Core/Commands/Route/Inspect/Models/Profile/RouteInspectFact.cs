namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

internal enum RouteInspectFactState
{
    Value,
    Unavailable,
    NotApplicable,
}

internal sealed class RouteInspectFact<T>
{
    private RouteInspectFact(RouteInspectFactState state, T? value, string? reason)
    {
        State = state;
        Value = value;
        Reason = reason;
    }

    internal RouteInspectFactState State { get; }

    internal T? Value { get; }

    internal string? Reason { get; }

    internal static RouteInspectFact<T> Available(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return new RouteInspectFact<T>(RouteInspectFactState.Value, value, null);
    }

    internal static RouteInspectFact<T> Unavailable(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        return new RouteInspectFact<T>(RouteInspectFactState.Unavailable, default, reason);
    }

    internal static RouteInspectFact<T> NotApplicable(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        return new RouteInspectFact<T>(RouteInspectFactState.NotApplicable, default, reason);
    }
}
