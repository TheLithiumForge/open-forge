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
        var asset = payload.Payload?.Find(SourceLogicalPath.AuthoringGuidePath)
            ?? throw new InvalidDataException(payload.Cause ?? "The embedded authoring reference is missing.");
        var authoring = Encoding.UTF8.GetString(asset.Bytes.AsSpan());
        var document = new MarkdownDocumentParser().Parse(authoring);
        var title = document.Headings.FirstOrDefault(heading => heading.Level == 1 && heading.IsTopLevel)
            ?? throw new InvalidDataException("The embedded authoring reference has no title.");
        var titleEnd = title.Span.End;
        if (titleEnd < authoring.Length && authoring[titleEnd] == '\r') titleEnd++;
        if (titleEnd < authoring.Length && authoring[titleEnd] == '\n') titleEnd++;
        authoring = authoring.Remove(title.Span.Start, titleEnd - title.Span.Start);

        // The reference becomes one section of the concepts topic, so its own sections nest one level below it.
        foreach (var heading in new MarkdownDocumentParser().Parse(authoring).Headings
                     .Where(heading => heading.Form == MarkdownHeadingForm.Atx && heading.Level is >= 2 and < 6)
                     .OrderByDescending(heading => heading.Span.Start))
        {
            authoring = authoring.Insert(heading.Span.Start, "#");
        }

        return new HelpResult
        {
            Status = CliSemanticStatus.Complete,
            Topic = request.Topic,
            Authoring = authoring,
        };
    }
}
