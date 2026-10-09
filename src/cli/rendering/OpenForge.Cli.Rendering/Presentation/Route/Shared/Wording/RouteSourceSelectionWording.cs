namespace OpenForge.Cli.Core.Presentation.Route.Shared.Wording;

internal static class RouteSourceSelectionWording
{
    internal static string Inspect(string reference, int count)
        => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.InspectSelection(reference, count);

    internal static string Move(string reference, int count)
        => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.MoveSelection(reference, count);

    internal static string Remove(string reference, int count)
        => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.RemoveSelection(reference, count);

    internal static string Update(string reference, int count)
        => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.UpdateSelection(reference, count);
}
