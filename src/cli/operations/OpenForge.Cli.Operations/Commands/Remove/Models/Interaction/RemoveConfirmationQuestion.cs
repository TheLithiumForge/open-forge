namespace OpenForge.Cli.Core.Commands.Remove.Models.Interaction;

internal sealed record RemoveConfirmationQuestion(
    string Target,
    bool IsMissing,
    int FileCount,
    int DirectoryCount);
