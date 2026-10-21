namespace OpenForge.Cli.Core.Presentation.Status.Models;

internal sealed record StatusDataIncludedFiles(
    int On,
    int Off,
    int Missing,
    int HandWritten,
    StatusDataMeasurement Startup,
    IReadOnlyList<string> Pending);
