using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;

internal enum RoutedMetadataEligibility
{
    Eligible,
    NotSelected,
    OutsideRoot,
    UnsupportedForm,
    Unreadable,
    UnsafeLeaf,
    ManagedContent,
    GeneratedContent,
    SettingsExcluded,
    PrivateContent,
    LibraryProjection,
}

internal sealed record RoutedMetadataCompletionResult
{
    public required RoutedMetadataEligibility Eligibility { get; init; }
    public required FrameworkMetadataCompletionResult Completion { get; init; }

    internal string? FactCode => Completion.State switch
    {
        FrameworkMetadataCompletionState.Completed or FrameworkMetadataCompletionState.NoOp => null,
        FrameworkMetadataCompletionState.Malformed => "index.metadata-skipped",
        FrameworkMetadataCompletionState.Unsafe => "index.metadata-unsafe",
        _ => throw new ArgumentOutOfRangeException(nameof(Completion), "The metadata completion state is not defined."),
    };
}
