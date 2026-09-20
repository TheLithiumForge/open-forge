using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

internal static class CommandOutputDetailVocabulary
{
    internal static IReadOnlyList<CliDetail> All { get; } =
    [
        CliDetail.Minimal,
        CliDetail.Standard,
        CliDetail.Full,
        CliDetail.Debug,
    ];

    internal static string Name(CliDetail detail) => CliReportVocabulary.Name(detail);
}
