
namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

internal sealed record LibraryRecordPublication
{

    public required LibraryRecordPublicationState State { get; init; }

    public required bool? PublishedLast { get; init; }
}
