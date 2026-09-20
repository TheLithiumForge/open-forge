using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;

namespace OpenForge.Cli.Core.Shell.Definitions;

internal static class CliProcessCompletionPolicy
{
    internal static CliProcessCompletion Complete(CliSemanticStatus status, CliOutputTarget primaryOutputTarget)
    {
        var definition = CliStatusDefinitions.Read(status);
        CliPresentationDefinitions.ValidateTarget(primaryOutputTarget);
        return new CliProcessCompletion(status, definition.Disposition.ExitCode, primaryOutputTarget);
    }
}
