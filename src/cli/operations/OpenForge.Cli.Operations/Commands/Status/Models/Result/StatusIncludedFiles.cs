namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal sealed record StatusIncludedFiles(
    int Listed,
    int Unlisted,
    int Missing,
    int HandWritten,
    StatusMeasurement Startup,
    IReadOnlyList<string> Pending);
