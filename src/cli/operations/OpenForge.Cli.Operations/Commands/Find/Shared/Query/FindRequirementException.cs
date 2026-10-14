namespace OpenForge.Cli.Core.Commands.Find.Shared.Query;

internal sealed class FindRequirementException(string value)
    : ArgumentException("--require accepts only all or any.", nameof(value))
{
    internal string Value { get; } = value;
}
