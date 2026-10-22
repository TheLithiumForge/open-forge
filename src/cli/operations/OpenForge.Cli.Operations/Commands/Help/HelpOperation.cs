using System.Text;
using OpenForge.Cli.Core.Commands.Help.Models.Request;
using OpenForge.Cli.Core.Commands.Help.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Help;

internal sealed class HelpOperation
{
    internal ValueTask<HelpResult> ExecuteAsync(HelpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return ValueTask.FromResult(Execute(request, cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromResult(new HelpResult
            {
                Status = CliSemanticStatus.Interrupted,
                Topic = request.Topic,
                Cause = "Help was cancelled.",
                Next = new CliNextAction("open-forge help", "Rerun Help."),
            });
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return ValueTask.FromResult(new HelpResult
            {
                Status = CliSemanticStatus.Failed,
                Topic = request.Topic,
                Cause = $"The embedded help content could not be read: {exception.GetType().Name}.",
                Next = new CliNextAction("open-forge help concepts --detail debug", "Report the failure and retry Help."),
            });
        }
    }

    private static HelpResult Execute(HelpRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request.Topic is null)
            return new HelpResult { Status = CliSemanticStatus.Complete, Topic = null };

        if (request.Topic != HelpDefinitions.ConceptsTopic)
        {
            var suggestion = request.NamesCommand ? $"open-forge {request.Topic} --help" : null;
            var cause = $"Unknown help topic '{request.Topic}'. Available topics: {string.Join(", ", HelpDefinitions.Topics)}.";
            if (suggestion is not null) cause = $"{cause} Run {suggestion}.";
            return new HelpResult
            {
                Status = CliSemanticStatus.Invalid,
                Topic = request.Topic,
                Cause = cause,
                SuggestedCommand = suggestion,
                Next = new CliNextAction(suggestion ?? "open-forge help --help", "Choose a listed topic or read the command's options."),
            };
        }

        var payload = EmbeddedFrameworkPayloadReader.Read();
        var asset = payload.Payload?.Find(SourceLogicalPath.HumanGuidePath)
            ?? throw new InvalidDataException(payload.Cause ?? "The embedded guide is missing.");
        return new HelpResult
        {
            Status = CliSemanticStatus.Complete,
            Topic = request.Topic,
            FileFormats = ReadFileFormats(Encoding.UTF8.GetString(asset.Bytes.AsSpan())),
        };
    }

    // The shipped guide's file-format section, from its heading up to the next section of the same level.
    // Help reads the bundled copy, so it still works in a workspace whose guide was deleted.
    private static string ReadFileFormats(string guide)
    {
        var headings = new MarkdownDocumentParser().Parse(guide).Headings
            .Where(heading => heading.Form == MarkdownHeadingForm.Atx && heading.Level <= 2)
            .OrderBy(heading => heading.Span.Start)
            .ToArray();
        var index = Array.FindIndex(headings, heading =>
            heading.Level == 2 && heading.VisibleText == HelpDefinitions.FileFormatsHeading);
        if (index < 0) throw new InvalidDataException("The embedded guide has no file-format section.");
        var start = headings[index].Span.Start;
        var end = index + 1 < headings.Length ? headings[index + 1].Span.Start : guide.Length;
        return guide[start..end].TrimEnd() + "\n";
    }
}
