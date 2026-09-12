namespace OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation.Models;

internal enum LifecycleSectionValidationState
{
    Valid,
    Blocked,
}

internal sealed record LifecycleSectionValidation
{
    private LifecycleSectionValidation(
        LifecycleSectionValidationState state,
        string? cause)
    {
        State = state;
        Cause = cause;
    }

    internal LifecycleSectionValidationState State { get; }

    internal string? Cause { get; }

    internal static LifecycleSectionValidation Valid()
        => new(LifecycleSectionValidationState.Valid, cause: null);

    internal static LifecycleSectionValidation Blocked(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return new LifecycleSectionValidation(
            LifecycleSectionValidationState.Blocked,
            cause);
    }
}
