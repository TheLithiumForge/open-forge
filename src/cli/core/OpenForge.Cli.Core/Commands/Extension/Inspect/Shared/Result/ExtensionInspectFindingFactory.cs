using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectFindingFactory
{
    private const int MaximumCauseLength = 240;

    internal static ExtensionInspectFinding Create(ExtensionInspectFindingInput input)
    {
        var status = ExtensionInspectDefinitions.ReadFindingStatus(input.Code);
        _ = CliStatusDefinitions.Read(status);
        return new ExtensionInspectFinding
        {
            Code = input.Code,
            Status = status,
            Subject = input.Subject,
            PackageId = input.PackageId,
            Dependency = input.Dependency,
            Path = input.Path,
            Cause = input.Cause.Length <= MaximumCauseLength
                ? input.Cause
                : $"{input.Cause[..(MaximumCauseLength - 3)]}...",
            Location = null,
            Candidates = input.Candidates
                .OrderBy(candidate => candidate.Id, StringComparer.Ordinal)
                .ThenBy(candidate => candidate.Path, StringComparer.Ordinal)
                .ToArray(),
        };
    }
}
