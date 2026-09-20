using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Shell.Pipeline;

internal static class CliOutputStage
{
    internal static async ValueTask<CliOutputReceipt> WriteAsync(
        CliRenderedOutput output,
        CliOutputWriters writers,
        CancellationToken cancellationToken)
    {
        Validate(output, writers);
        cancellationToken.ThrowIfCancellationRequested();

        var primaryWriter = output.PrimaryTarget switch
        {
            CliOutputTarget.StandardOutput => writers.StandardOutput,
            CliOutputTarget.StandardError => writers.StandardError,
            _ => throw new ArgumentOutOfRangeException(
                nameof(output),
                output.PrimaryTarget,
                "The output target is not defined."),
        };
        if (output.TextDocument is { } document)
        {
            foreach (var span in document.Spans)
            {
                await primaryWriter.WriteAsync((span.Authored ? span.Content : CliText.PlatformLineEndings(span.Content)).AsMemory(), cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            await primaryWriter.WriteLineAsync(output.PrimaryContent.AsMemory(), cancellationToken).ConfigureAwait(false);
        }

        var diagnosticWritten = output.DiagnosticContent is not null;
        if (diagnosticWritten)
        {
            await writers.StandardError
                .WriteLineAsync(CliText.PlatformLineEndings(output.DiagnosticContent!).AsMemory(), cancellationToken)
                .ConfigureAwait(false);
        }

        return new CliOutputReceipt(output.Status, output.Format, output.PrimaryTarget, diagnosticWritten);
    }

    private static void Validate(CliRenderedOutput output, CliOutputWriters writers)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(writers);
        ArgumentNullException.ThrowIfNull(output.PrimaryContent);
        _ = CliStatusDefinitions.Read(output.Status);
        CliPresentationDefinitions.ValidateOutputPolicy(output.Status, output.Format, output.PrimaryTarget);
        if (output.DiagnosticContent is { } diagnostics
            && CliText.PlatformLineEndings(diagnostics).Length + Environment.NewLine.Length > CliPresentationDefinitions.MaximumDiagnosticLength)
        {
            throw new InvalidOperationException("Diagnostic content is not bounded.");
        }
    }
}

