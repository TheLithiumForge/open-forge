namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal sealed record StatusIncludedFiles(
    int On,
    int Off,
    int Missing,
    int HandWritten,
    StatusMeasurement Startup,
    IReadOnlyList<string> Pending);
