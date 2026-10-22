namespace OpenForge.Cli.Core.Presentation.Status.Models;

internal sealed record StatusDataIncludedFiles(
    int Listed,
    int Unlisted,
    int Missing,
    int HandWritten,
    StatusDataMeasurement Startup,
    IReadOnlyList<string> Pending);
