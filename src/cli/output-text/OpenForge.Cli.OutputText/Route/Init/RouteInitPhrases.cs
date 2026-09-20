using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Init;

internal static class RouteInitPhrases
{
    internal static string FormatWouldCreate(string pathText)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FormatWouldCreate(pathText);

    internal static string FormatNotStarted(string pathText)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatNotStarted(pathText);

    internal static string FormatFinalStateUnknown(string pathText)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatFinalStateUnknown(pathText);

    internal static string FormatFailed(string pathText)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatFailed(pathText);

    // @OpenForgeText route.init.phrase.the-route-could-not-be-initialized-nothing-was-changed
    internal static string FormatTheRouteCouldNotBeInitializedNothingWasChanged(string trimSentenceText)
        => $"The route could not be initialized: {trimSentenceText}. Nothing was changed.";

    // @OpenForgeText route.init.phrase.cannot-initialize
    internal static string FormatCannotInitialize(string targetText, string trimSentenceText)
        => $"Cannot initialize {targetText}: {trimSentenceText}.";

    // @OpenForgeText route.init.phrase.recovery-the-recovery-bundle-was-retained-at
    internal static string FormatRecoveryTheRecoveryBundleWasRetainedAt(string pathText)
        => $"Recovery: the recovery bundle was retained at {pathText}.";

    // @OpenForgeText route.init.phrase.is-not-a-route-id-or-an-entrypoint-path-under-agents
    internal static string FormatIsNotARouteIdOrAnEntrypointPathUnderAgents(string targetText)
        => $"{targetText} is not a route ID or an entrypoint path under .agents.";
}
