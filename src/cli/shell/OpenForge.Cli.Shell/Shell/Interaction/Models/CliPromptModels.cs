using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Shell.Interaction.Models;

internal readonly record struct CliPromptPolicy(bool Allowed);
internal enum CliPromptState { Answered, Cancelled, Unavailable }
internal sealed record CliPromptReply<T> where T : notnull
{
    private readonly T? _value;
    private CliPromptReply(CliPromptState state, T? value = default) { State = state; _value = value; }
    internal CliPromptState State { get; }
    internal T Value => State == CliPromptState.Answered && _value is { } value
        ? value
        : throw new InvalidOperationException("The prompt has no answer.");
    internal static CliPromptReply<T> Answered(T value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return new(CliPromptState.Answered, value);
    }
    internal static CliPromptReply<T> Cancelled() => new(CliPromptState.Cancelled);
    internal static CliPromptReply<T> Unavailable() => new(CliPromptState.Unavailable);
}

internal delegate ValueTask<CliPromptReply<TAnswer>> CliPrompt<TQuestion, TAnswer>(
    TQuestion question, CliPromptPolicy policy, CancellationToken cancellationToken)
    where TQuestion : notnull where TAnswer : notnull;
internal delegate ValueTask<CliPromptReply<bool>> CliPlanConfirmation<TResult, TQuestion>(
    TResult preview, TQuestion question, CliPromptPolicy policy, CancellationToken cancellationToken)
    where TResult : ICliCommandResult where TQuestion : notnull;

internal sealed record CliConfirmQuestion(string Sentence);
internal sealed record CliChoice<T>(T Value, string Label, string? Description = null) where T : notnull;
internal sealed record CliSelectQuestion<T>(string Question, IReadOnlyList<CliChoice<T>> Choices) where T : notnull;
internal enum CliDependencyDirection { Requires, Dependents }
internal sealed record CliDependency<T>(T Value, T Dependency) where T : notnull;
internal sealed record CliMultiSelectQuestion<T>(
    string Question, IReadOnlyList<CliChoice<T>> Choices, IReadOnlyList<CliDependency<T>> Dependencies,
    IReadOnlySet<T> Disabled, CliDependencyDirection Direction = CliDependencyDirection.Requires) where T : notnull;
internal sealed record CliMultiSelection<T>(IReadOnlyList<T> Chosen, IReadOnlyList<T> Required) where T : notnull;
internal sealed record CliTextValidation<T>(bool IsValid, T? Value, string? Error) where T : notnull;
internal sealed record CliTextQuestion<T>(string Label, string Rule, bool Required, Func<string, CliTextValidation<T>> Validate)
    where T : notnull;
internal enum CliPermissionChoice { Always, Once, Cancel }
internal sealed record CliPermissionPath(string Path, bool IsDirectory);
internal sealed record CliPermissionQuestion(string Owner, IReadOnlyList<CliPermissionPath> Paths);
