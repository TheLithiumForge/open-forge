using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
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
            SourceLayerVerificationState.Verified => ReadFileFindingCode(read.Read?.State),
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

    internal static FindFindingCode? ReadFileFindingCode(FileReadState? state)
        => state switch
        {
            FileReadState.Complete => null,
            FileReadState.Missing => FindFindingCode.InspectionUnavailable,
            FileReadState.InvalidEncoding => FindFindingCode.InvalidEncoding,
            FileReadState.InvalidSyntax => FindFindingCode.InspectionUnavailable,
            FileReadState.AccessDenied => FindFindingCode.InspectionUnavailable,
            FileReadState.InputOutputFailure => FindFindingCode.InspectionUnavailable,
            FileReadState.Cancelled => FindFindingCode.Interrupted,
            null => FindFindingCode.InspectionUnavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The file read state is not defined."),
        };

    private static FindFinding CreateFinding(
        OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceLogicalSource source,
        OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceLayer layer,
        FindFindingCode code)
    {
        var sourceIdentity = new FindSourceIdentity(
            source.Identity.AutomaticId,
            source.Identity.CanonicalBasePath);
        return new FindFinding(
            code,
            FindDefinitions.ReadFindingStatus(code),
            null,
            ReadFindingCause(code),
            null,
            null,
            sourceIdentity,
            layer.Kind,
            layer.CanonicalPath,
            null,
            null,
            []);
    }

    internal static string ReadFindingCause(FindFindingCode code)
        => code switch
        {
            FindFindingCode.InvalidInput => "The source layer could not be read or verified.",
            FindFindingCode.InvalidSelector => "The source layer could not be read or verified.",
            FindFindingCode.WorkspaceUnavailable => "The source layer could not be read or verified.",
            FindFindingCode.WorkspaceUnsafe => "The source layer could not be read or verified.",
            FindFindingCode.SelectorAmbiguous => "The source layer could not be read or verified.",
            FindFindingCode.SelectorUnsafe => "The source layer could not be read or verified.",
            FindFindingCode.IdentityCollision => "The source layer could not be read or verified.",
            FindFindingCode.CandidateUnsafe => "The source layer left the established workspace boundary.",
            FindFindingCode.LayerUnresolved => "The source layer could not be read or verified.",
            FindFindingCode.InspectionUnavailable => "The source layer could not be read or verified.",
            FindFindingCode.InvalidEncoding => "The source layer is not valid UTF-8.",
            FindFindingCode.FrontmatterUnavailable => "The source layer could not be read or verified.",
            FindFindingCode.SectionAmbiguous => "The source layer could not be read or verified.",
            FindFindingCode.ProjectionMissing => "The source layer could not be read or verified.",
            FindFindingCode.ProjectionUnavailable => "The source layer could not be read or verified.",
            FindFindingCode.OperationFailed => "The source layer could not be read or verified.",
            FindFindingCode.Interrupted => "The source layer read was cancelled.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Find finding code is not defined."),
        };
}
