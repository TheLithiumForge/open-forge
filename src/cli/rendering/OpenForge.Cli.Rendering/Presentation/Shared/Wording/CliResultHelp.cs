using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Shared.Wording;

internal static class CliResultHelp
{
    internal static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTextCompletedCompletedWithWarningsAndIncompleteResultsUseStdoutInvalidInputBlockedFailedAndCancelledResultsUseStderr()),
            ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.MessageJsonWritesOneMinifiedSchemaVersion3EnvelopeToStdoutForEverySemanticStatusDebugDiagnosticsUseBoundedStderr()),
        };
        foreach (var status in Enum.GetValues<CliSemanticStatus>())
        {
            var definition = CliStatusDefinitions.Read(status);
            lines.Add(global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatExitAndText(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{definition.MachineName}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{definition.Disposition.ExitCode}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Stream(definition.Disposition.HumanOutputTarget)}")));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Stream(CliOutputTarget target)
        => target switch
        {
            CliOutputTarget.StandardOutput => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelStdout(),
            CliOutputTarget.StandardError => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelStderr(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(target),
                target,
                "The output target is not defined."),
        };
}
