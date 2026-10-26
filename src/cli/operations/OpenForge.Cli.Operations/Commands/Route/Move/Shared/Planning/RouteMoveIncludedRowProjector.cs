using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.References;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed class RouteMoveIncludedRowProjector(SourceLinkDestinationResolver destinationResolver)
{
    internal static GeneratedNavigationRegion PreserveAuthoredBody(GeneratedNavigationRegion region, MarkdownDocumentFacts? document)
    {
        if (region.Change is not { } change || document?.GeneratedRegion.EntriesBlock is not { } block
            || SourceGeneratedEntriesParser.Parse(document) is not { State: SourceGeneratedEntriesState.Complete, Entries.Count: > 0 }) return region;
        var generated = string.Join(block.LineEnding, region.Entries.Select(entry => entry.Line));
        var body = document.Source[block.Span.Start..block.Span.End];
        if (generated.Length > 0) body = generated + block.LineEnding + body;
        return GeneratedNavigationRegion.Available(region.Source, region.Entries, new GeneratedNavigationBoundedChange(new()
        {
            ContentLocation = change.ContentLocation,
            BeforeBody = change.BeforeBody,
            ExpectedBody = body,
            Prefix = change.Prefix,
            Suffix = change.Suffix,
        })) with
        { AuthoredLinkCount = region.AuthoredLinkCount };
    }

    internal async ValueTask<RouteMoveIncludedRowProjection> ProjectAsync(RouteMoveReferenceScanInput input,
        RouteMoveReferenceDocument document, string sourcePath, string intendedSourcePath, CancellationToken cancellationToken)
    {
        var subject = input.Request.Destination.Inventory.Subject;
        if (!RouteMoveMaintenanceBoundaryInspector.IsOutside(subject.Catalogue, subject.SelectedSource.Base.CanonicalPath)) return new(null, null);
        var outsideSource = RouteMoveMaintenanceBoundaryInspector.IsOutside(subject.Catalogue, sourcePath);
        var retained = subject.Catalogue.IncludedRows.Where(row => row.ParentPath == sourcePath
            && (input.MovedPaths.ContainsKey(row.Path) || row.AliasOf is { } alias && input.MovedPaths.ContainsKey(alias))).ToArray();
        if (!outsideSource && retained.Length == 0) return new(null, null);
        var parsed = SourceGeneratedEntriesParser.Parse(document.Facts);
        if (parsed.State == SourceGeneratedEntriesState.Absent) return new(null, null);
        if (parsed.State != SourceGeneratedEntriesState.Complete)
            return Stop(RouteMoveFindingCode.ReferenceUnsafe, parsed.Cause ?? "An authored outside Entries section is malformed.");
        var replacements = new List<RouteMoveReferenceReplacement>();
        var meanings = new List<RouteMoveReferenceMeaning>();
        foreach (var row in parsed.Entries)
        {
            if (!outsideSource && !retained.Any(value => value.Entry.Span == row.Span)) continue;
            var links = document.Facts.Links.Where(link => link.Span.Start >= row.Span.Start && link.Span.End <= row.Span.End).ToArray();
            if (links.Length != 1) return Stop(RouteMoveFindingCode.ReferenceUnsafe, "An authored outside row lacks one exact link span.");
            var link = links[0];
            var facts = await destinationResolver.ResolveAsync(new SourceLinkDestinationInput
            {
                Workspace = subject.Request.Workspace,
                Catalogue = subject.Catalogue,
                SourceCanonicalPath = sourcePath,
                RawDestination = link.RawDestination,
            }, cancellationToken).ConfigureAwait(false);
            if (facts.Target.Kind == SourceLinkTargetKind.External) continue;
            if (facts.Target.Path is not { } target || facts.Finding is not null && facts.Target.Resolution != SourceLinkTargetResolution.FragmentMissing)
                return Stop(RouteMoveFindingCode.ReferenceCoverageIncomplete, facts.Finding?.Cause ?? "An authored outside row cannot be resolved completely.");
            var expectedTarget = input.MovedPaths.GetValueOrDefault(target, target);
            meanings.Add(RouteMoveReferenceScanner.ProjectMeaning(input, facts)
                ?? throw new InvalidOperationException("A resolved local row requires its meaning."));
            if (SourceLinkDestinationResolver.ResolvesToCanonicalPath(subject.Request.Workspace, intendedSourcePath, link.RawDestination, expectedTarget)) continue;
            if (link.DestinationSpan is not { } span) return Stop(RouteMoveFindingCode.ReferenceUnsafe, "An authored row has no replaceable destination span.");
            var expected = RouteMoveReferenceScanner.BuildDestination(intendedSourcePath, expectedTarget, link.RawDestination,
                span.Length == link.RawDestination.Length + 2, facts.IsFolder);
            replacements.Add(new RouteMoveReferenceReplacement
            {
                Span = span,
                Before = document.Text[span.Start..span.End],
                Expected = expected,
                OldTarget = target,
                NewTarget = expectedTarget,
                Layer = facts.Target.Layer,
            });
        }
        if (replacements.Count > 0 && !RouteMoveMaintenanceBoundaryInspector.SameBoundary(subject.Catalogue.Layout,
                subject.SelectedSource.Base.CanonicalPath, sourcePath))
            return Stop(RouteMoveFindingCode.ReferenceUnsafe, "A required authored row replacement is outside the selected maintenance boundary.");
        if (!outsideSource) return new(null, null);
        if (replacements.Count > 0 && RouteMoveMaintenanceBoundaryInspector.ReadPathProblem(subject.LayoutRead, sourcePath, false) is { } problem)
            return Stop(RouteMoveFindingCode.ReferenceUnsafe, problem);
        var text = RouteMoveReferenceChangeProjector.Apply(document.Text, replacements);
        var expectedRows = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(text));
        var map = new Utf8SourceMap(document.Text);
        return new(new RouteMoveIncludedRowPlan
        {
            IntendedText = text,
            OriginalEntries = [.. parsed.Entries],
            ExpectedEntries = [.. expectedRows.Entries],
            Meanings = [.. meanings],
            Edits = [.. replacements.Select(edit => new RouteMoveReferenceDocumentEdit
            {
                Location = map.Map(edit.Span.Start, edit.Span.Length), Before = edit.Before, Expected = edit.Expected,
            })],
        }, null);

        RouteMoveIncludedRowProjection Stop(RouteMoveFindingCode code, string cause)
            => new(null, RouteMoveReferenceScanner.Stop(input, code,
                code == RouteMoveFindingCode.ReferenceCoverageIncomplete ? CliSemanticStatus.Incomplete : CliSemanticStatus.Blocked, sourcePath, cause));
    }

    internal static SourceCatalogue ProjectCatalogue(RouteMoveNavigationPlanningRequest request)
    {
        var catalogue = request.Destination.Inventory.Subject.Catalogue;
        var paths = RouteMoveReferenceChangeProjector.BuildMovedPathMap(request.Destination);
        var documents = request.ReferenceDocuments.ToDictionary(document => document.SourcePath, StringComparer.Ordinal);
        var rows = catalogue.IncludedRows.Select(row =>
        {
            var entry = row.Entry;
            if (documents.GetValueOrDefault(row.ParentPath)?.IncludedRows is { } plan)
            {
                var index = Array.FindIndex(plan.OriginalEntries.ToArray(), value => value.Span == row.Entry.Span);
                if (index >= 0) entry = plan.ExpectedEntries[index];
            }
            return row with
            {
                Path = paths.GetValueOrDefault(row.Path, row.Path),
                ParentPath = paths.GetValueOrDefault(row.ParentPath, row.ParentPath),
                AliasOf = row.AliasOf is { } alias ? paths.GetValueOrDefault(alias, alias) : null,
                Entry = entry,
            };
        });
        return new SourceCatalogue(catalogue.Layout, catalogue.Candidates, catalogue.Sources, catalogue.Issues,
            catalogue.IsCancelled, catalogue.Aliases, rows);
    }
}
