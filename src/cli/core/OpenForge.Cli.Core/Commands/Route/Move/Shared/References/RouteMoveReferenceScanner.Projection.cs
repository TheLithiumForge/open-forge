using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.References;

internal sealed partial class RouteMoveReferenceScanner
{
    private static RouteMoveReferenceDocumentInspection FormInspection(
        RouteMoveReferenceScanInput input,
        string sourcePath,
        string intendedSourcePath,
        RouteMoveReferenceDocument document,
        RouteMoveReferenceReplacementScan replacements)
    {
        var intendedText = RouteMoveReferenceChangeProjector.Apply(document.Text, replacements.Replacements);
        var map = new Utf8SourceMap(document.Text);
        return new RouteMoveReferenceDocumentInspection
        {
            Document = new RouteMoveReferenceDocumentPlan
            {
                SourcePath = sourcePath,
                DestinationSourcePath = intendedSourcePath,
                Snapshot = document.Snapshot,
                IntendedText = intendedText,
                Edits = ProjectEdits(map, replacements.Replacements),
                Meanings = replacements.Meanings,
            },
            Rewrites = ProjectRewrites(
                input,
                sourcePath,
                intendedSourcePath,
                map,
                replacements.Replacements),
            OccurrenceCount = replacements.OccurrenceCount,
        };
    }

    private static ImmutableArray<RouteMoveReferenceDocumentEdit> ProjectEdits(
        Utf8SourceMap map,
        IEnumerable<RouteMoveReferenceReplacement> replacements)
        => [.. replacements.OrderBy(value => value.Span.Start).Select(replacement =>
            new RouteMoveReferenceDocumentEdit
            {
                Location = map.Map(replacement.Span.Start, replacement.Span.Length),
                Before = replacement.Before,
                Expected = replacement.Expected,
            })];

    private static RouteMoveReferenceReplacementProjection ProjectReplacement(
        RouteMoveReferenceScanInput input,
        string sourcePath,
        string intendedSourcePath,
        MarkdownLinkFact link,
        SourceLinkDestinationFacts facts)
    {
        var move = ReadTargetMove(input, sourcePath, intendedSourcePath, facts);
        if (move is null)
        {
            return new RouteMoveReferenceReplacementProjection();
        }

        if (link.DestinationSpan is not { } span)
        {
            return new RouteMoveReferenceReplacementProjection
            {
                Boundary = Stop(
                    input,
                    RouteMoveFindingCode.ReferenceCoverageIncomplete,
                    Shell.Definitions.CliSemanticStatus.Incomplete,
                    sourcePath,
                    "A local Markdown destination does not expose one exact replaceable span."),
            };
        }

        var workspace = input.Request.Destination.Inventory.Subject.Request.Workspace;
        if (SourceLinkDestinationResolver.ResolvesToCanonicalPath(
                workspace,
                intendedSourcePath,
                link.RawDestination,
                move.NewPath))
        {
            return new RouteMoveReferenceReplacementProjection();
        }

        var expected = BuildDestination(
            intendedSourcePath,
            move.NewPath,
            link.RawDestination,
            IsAngleEnclosed(link));
        var before = ReadDestinationLiteral(link);
        if (string.Equals(before, expected, StringComparison.Ordinal))
        {
            return new RouteMoveReferenceReplacementProjection();
        }

        return new RouteMoveReferenceReplacementProjection
        {
            Replacement = new RouteMoveReferenceReplacement
            {
                Span = span,
                Before = before,
                Expected = expected,
                OldTarget = move.OldPath,
                NewTarget = move.NewPath,
                Layer = facts.Target.Layer,
            },
        };
    }

    private static RouteMoveReferenceTargetMove? ReadTargetMove(
        RouteMoveReferenceScanInput input,
        string sourcePath,
        string intendedSourcePath,
        SourceLinkDestinationFacts facts)
    {
        if (facts.Target.Kind == SourceLinkTargetKind.External
            || facts.Target.Path is not { } targetPath)
        {
            return null;
        }

        var intendedTargetPath = input.MovedPaths.GetValueOrDefault(targetPath, targetPath);
        if (string.Equals(sourcePath, intendedSourcePath, StringComparison.Ordinal)
            && string.Equals(targetPath, intendedTargetPath, StringComparison.Ordinal))
        {
            return null;
        }

        return new RouteMoveReferenceTargetMove
        {
            OldPath = targetPath,
            NewPath = intendedTargetPath,
        };
    }

    private static ImmutableArray<RouteMoveReferenceRewrite> ProjectRewrites(
        RouteMoveReferenceScanInput input,
        string sourcePath,
        string intendedSourcePath,
        Utf8SourceMap map,
        IEnumerable<RouteMoveReferenceReplacement> replacements)
        => [.. replacements.OrderBy(value => value.Span.Start).Select(replacement =>
            new RouteMoveReferenceRewrite
            {
                SourcePath = sourcePath,
                DestinationSourcePath = intendedSourcePath,
                Layer = ReadLayer(replacement.Layer),
                Location = map.Map(
                    replacement.Span.Start,
                    replacement.Span.Length),
                Before = replacement.Before,
                Expected = replacement.Expected,
                OldTarget = new RouteMoveReferenceTarget
                {
                    Id = input.Request.Destination.Inventory.Subject.Catalogue
                        .FindByPath(replacement.OldTarget)?.Identity.AutomaticId,
                    Path = replacement.OldTarget,
                },
                ExpectedTarget = new RouteMoveReferenceTarget
                {
                    Id = SourceIdentity.DeriveId(replacement.NewTarget),
                    Path = replacement.NewTarget,
                },
            })];

    private static string BuildDestination(
        string sourcePath,
        string targetPath,
        string rawDestination,
        bool angleEnclosed)
    {
        var fragmentOffset = rawDestination.IndexOf('#');
        var fragment = fragmentOffset < 0 ? string.Empty : rawDestination[fragmentOffset..];
        var sourceDirectory = Path.GetDirectoryName(
            sourcePath.Replace('/', Path.DirectorySeparatorChar)) ?? string.Empty;
        if (sourceDirectory.Length == 0)
        {
            sourceDirectory = ".";
        }

        var relative = Path.GetRelativePath(
                sourceDirectory,
                targetPath.Replace('/', Path.DirectorySeparatorChar))
            .Replace(Path.DirectorySeparatorChar, '/');
        var encoded = string.Join('/', relative.Split('/').Select(segment =>
            segment is "." or ".." ? segment : Uri.EscapeDataString(segment)));
        var destination = encoded + fragment;
        return angleEnclosed ? $"<{destination}>" : destination;
    }

    private static bool IsAngleEnclosed(MarkdownLinkFact link)
        => link.DestinationSpan?.Length == link.RawDestination.Length + 2;

    private static string ReadDestinationLiteral(MarkdownLinkFact link)
        => IsAngleEnclosed(link) ? $"<{link.RawDestination}>" : link.RawDestination;

    private static RouteMoveLayerKind? ReadLayer(SourceLayerKind? layer)
        => layer switch
        {
            null => null,
            SourceLayerKind.Base => RouteMoveLayerKind.Base,
            SourceLayerKind.Overwrite => RouteMoveLayerKind.Overwrite,
            _ => throw new ArgumentOutOfRangeException(
                nameof(layer),
                layer,
                "The source link layer is not defined."),
        };
}
