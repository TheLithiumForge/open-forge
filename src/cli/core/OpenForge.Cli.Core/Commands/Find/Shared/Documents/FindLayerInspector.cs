using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Documents;

internal sealed class FindLayerInspector(
    FindSelectedLayerReader selectedLayerReader,
    FindMarkdownDocumentReader markdownDocumentReader,
    FindFrontmatterFactsReader frontmatterFactsReader,
    FindBodyTagScanner bodyTagScanner)
{
    private readonly FindSelectedLayerReader _selectedLayerReader = selectedLayerReader;
    private readonly FindMarkdownDocumentReader _markdownDocumentReader = markdownDocumentReader;
    private readonly FindFrontmatterFactsReader _frontmatterFactsReader = frontmatterFactsReader;
    private readonly FindBodyTagScanner _bodyTagScanner = bodyTagScanner;

    internal ValueTask<FindLayerInspectionFacts> InspectAsync(
        FindLayerInspectionInput input,
        CancellationToken cancellationToken)
        => InspectCoreAsync(input, cancellationToken);

    private async ValueTask<FindLayerInspectionFacts> InspectCoreAsync(
        FindLayerInspectionInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var read = await _selectedLayerReader(
                input.DocumentReader,
                input.Layer,
                cancellationToken)
            .ConfigureAwait(false);
        var readFinding = ReadFinding(input.Source, input.Layer, read);
        if (readFinding is not null)
        {
            return Unavailable(input, readFinding);
        }

        var source = read.Read?.Value
            ?? throw new InvalidOperationException("A complete source document read requires source text.");
        var document = _markdownDocumentReader(source);
        var frontmatter = _frontmatterFactsReader(new FindFrontmatterInput(input.Layer, document));
        var bodyTags = _bodyTagScanner.Scan(new FindBodyTagInput(document));
        return new FindLayerInspectionFacts(
            input.Source,
            input.Layer,
            document,
            frontmatter,
            bodyTags,
            frontmatter.Description,
            []);
    }

    private static FindLayerInspectionFacts Unavailable(
        FindLayerInspectionInput input,
        FindFinding finding)
        => new(input.Source, input.Layer, null, null, null, null, [finding]);

    private static FindFinding? ReadFinding(
        OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceLogicalSource source,
        OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceLayer layer,
        SourceDocumentReadResult read)
    {
        var code = read.Verification.State switch
        {
            SourceLayerVerificationState.Verified => read.Read?.State switch
            {
                FileReadState.Complete => (FindFindingCode?)null,
                FileReadState.InvalidEncoding => FindFindingCode.InvalidEncoding,
                FileReadState.Cancelled => FindFindingCode.Interrupted,
                _ => FindFindingCode.InspectionUnavailable,
            },
            SourceLayerVerificationState.Unsafe => FindFindingCode.CandidateUnsafe,
            SourceLayerVerificationState.Cancelled => FindFindingCode.Interrupted,
            SourceLayerVerificationState.Missing
                or SourceLayerVerificationState.Unavailable
                or SourceLayerVerificationState.Changed => FindFindingCode.InspectionUnavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.Verification.State,
                "The source layer verification state is not defined."),
        };

        return code is { } findingCode
            ? CreateFinding(source, layer, findingCode)
            : null;
    }

    private static FindFinding CreateFinding(
        OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceLogicalSource source,
        OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceLayer layer,
        FindFindingCode code)
    {
        var sourceIdentity = new FindSourceIdentity(
            source.Identity.AutomaticId,
            source.Identity.CanonicalBasePath);
        var cause = code switch
        {
            FindFindingCode.CandidateUnsafe => "The source layer left the established workspace boundary.",
            FindFindingCode.InvalidEncoding => "The source layer is not valid UTF-8.",
            FindFindingCode.Interrupted => "The source layer read was cancelled.",
            _ => "The source layer could not be read or verified.",
        };
        return new FindFinding(
            code,
            FindDefinitions.ReadFindingStatus(code),
            null,
            cause,
            null,
            null,
            sourceIdentity,
            layer.Kind,
            layer.CanonicalPath,
            null,
            null,
            []);
    }
}
