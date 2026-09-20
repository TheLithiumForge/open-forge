using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Text;

namespace OpenForge.Cli.Core.Presentation.Route.List.Shared.Wording;

internal static class RouteListWording
{
    internal static string Listed(int count)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListPhrases.FormatListed(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(count, "route")}"));

    internal static string NoRoutes(string? id)
        => id is null ? global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageTheLoaderExposesNoRoutes() : global::OpenForge.Cli.OutputText.Route.List.RouteListPhrases.FormatNoRoutesUnder($"{id}");

    internal static string ListedWithWarnings(int count)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListPhrases.FormatListedWithWarnings(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(count, "route")}"));

    internal static string Incomplete() => global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageTheListingIsIncomplete();

    internal static string CannotList(string reason)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListPhrases.FormatCannotListRoutes($"{TrimSentence(reason)}");

    internal static string Failed(string reason)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListPhrases.FormatRouteListStoppedBecauseOfAnUnexpectedError($"{TrimSentence(reason)}");

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageRouteListWasCancelled();

    internal static string InvalidSourceReference(string operand)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListWording.InvalidSourceReference(operand);

    internal static string InvalidDepth() => global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageDepthMustBeAWholeNumberOrAll();

    internal static string LoaderSubject()
        => global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageTheLoaderIsTheRootOfEveryRouteRunRouteListWithoutAnOperand();

    internal static string PhysicalBoundary(string path)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListWording.PhysicalBoundary(path);

    internal static string LoaderUnavailable() => global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageAgentsLoaderMdCouldNotBeRead();

    internal static string LoaderMalformed() => global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageAgentsLoaderMdHasNoUsableEntriesSection();

    internal static string UnsupportedSource(string path)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListWording.UnsupportedSource(path);

    internal static string ReadUnavailable(string path)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListWording.ReadUnavailable(path);

    internal static string MetadataMissing(string path)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListWording.MetadataMissing(path);

    internal static string MetadataMalformed(string path, string reason)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListPhrases.FormatTheFrontmatterOfCouldNotBeRead($"{path}", $"{TrimSentence(reason)}");

    internal static string AuthoredForm(string path, string name)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListWording.AuthoredForm(path, name);

    internal static string RenameEntrypoint(string path, string name)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListWording.RenameEntrypoint(path, name);

    internal static string RepairMetadata(string id)
        => global::OpenForge.Cli.OutputText.Route.List.RouteListWording.RepairMetadata(id);

    internal static string FixByHand() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFixByHand();

    internal static string NextReason(string action) => global::OpenForge.Cli.OutputText.Route.List.RouteListWording.NextReason(action);

    internal static string DepthTrailer(int count, RouteListPresentationDepth depth)
    {
        var routeWord = Plural(count, global::OpenForge.Cli.OutputText.Shared.SharedText.LabelRoute());
        return global::OpenForge.Cli.OutputText.Route.List.RouteListPhrases.FormatToDepthDeeperRoutesOpenForgeRouteListDepthAll(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{routeWord}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{depth.MachineValue}"));
    }

    internal static string Kind(RouteListPresentationRowKind kind) => kind switch
    {
        RouteListPresentationRowKind.Entrypoint => global::OpenForge.Cli.OutputText.Route.List.RouteListText.LabelEntrypoint(),
        RouteListPresentationRowKind.RoutedLeaf => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFile(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The route-list row kind is not defined."),
    };

    internal static string SelectedAs(RouteListPresentationSelectionProvenance provenance) => provenance switch
    {
        RouteListPresentationSelectionProvenance.LoaderRoot => global::OpenForge.Cli.OutputText.Route.List.RouteListText.LabelLoaderRoot(),
        RouteListPresentationSelectionProvenance.ExplicitRoot => global::OpenForge.Cli.OutputText.Route.List.RouteListText.LabelExplicitRoot(),
        RouteListPresentationSelectionProvenance.DetachedRoot => global::OpenForge.Cli.OutputText.Route.List.RouteListText.LabelDetachedRoot(),
        RouteListPresentationSelectionProvenance.Descendant => global::OpenForge.Cli.OutputText.Route.List.RouteListText.LabelDescendant(),
        _ => throw new ArgumentOutOfRangeException(nameof(provenance), provenance, "The route-list selection provenance is not defined."),
    };

    internal static string DiagnosticsDepth(RouteListPresentationDepth? depth)
        => depth?.MachineValue ?? "none";

    internal static string SelectionKind(RouteListPresentationSelectionKind kind) => kind switch
    {
        RouteListPresentationSelectionKind.LoaderRoots => "loader-roots",
        RouteListPresentationSelectionKind.SourceId => "source-id",
        RouteListPresentationSelectionKind.SourcePath => "source-path",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The route-list selection kind is not defined."),
    };

    internal static string RoutePlural(int count) => Plural(count, global::OpenForge.Cli.OutputText.Shared.SharedText.LabelRoute());

    private static string Plural(int count, string singular) => CliText.Plural(count, singular);

    private static string TrimSentence(string value) => CliFindingWording.PlainCause(value);
}
