using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Interaction;

internal static class RouteInspectInteractionIntegrationFixture
{
    internal const string CollisionId = "root/collision";
    internal const string FirstCandidate = ".agents/root/collision.md";
    internal const string SecondCandidate = ".agents/root/collision/_collision.md";

    internal static async Task<RouteInspectDirectInteractionRun> RunOperationAsync(
        RouteInspectProfileIntegrationWorkspace workspace,
        string sourceReference,
        string standardInput,
        bool allowInteractiveSourceSelection,
        bool canPrompt)
    {
        using var input = new StringReader(standardInput);
        using var prompt = new StringWriter();
        var terminal = new CliTerminal(
            new CliTerminalCapabilities(canPrompt, canReadKeys: false, canRedraw: false),
            (content, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                prompt.Write(content.Span);
                return ValueTask.CompletedTask;
            },
            cancellationToken => input.ReadLineAsync(cancellationToken),
            _ => ValueTask.FromResult<CliKeyStroke?>(null));
        var operation = RouteInspectOperationFactory.Create(
            RouteInspectSourceSelectionPrompt.Create(new CliPrompts(terminal)));
        var result = await operation(
            Request(workspace, sourceReference, allowInteractiveSourceSelection),
            TestContext.Current.CancellationToken);
        return new RouteInspectDirectInteractionRun
        {
            Result = result,
            Prompt = prompt.ToString(),
            RemainingInput = await input.ReadLineAsync(CancellationToken.None),
        };
    }

    internal static RouteInspectRequest Request(
        RouteInspectProfileIntegrationWorkspace workspace,
        string sourceReference,
        bool allowInteractiveSourceSelection)
    {
        return new RouteInspectRequest(
            workspace.Workspace,
            sourceReference,
            allowInteractiveSourceSelection);
    }

    internal static RouteInspectProfileIntegrationWorkspace CreateCollisionWorkspace(
        bool ambiguousRoute = false)
    {
        var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "Root")]);
        workspace.WriteEntrypoint(new RouteInspectProfileIntegrationEntrypoint
        {
            RelativePath = ".agents/root/_root.md",
            Description = "Root",
            Tags = ["Root"],
            Entries =
            [
                RouteInspectProfileIntegrationWorkspace.Entry("Collision leaf", "collision.md", "Route"),
                RouteInspectProfileIntegrationWorkspace.Entry("Collision entrypoint", "collision/_collision.md", "Route"),
            ],
        });
        if (ambiguousRoute)
        {
            workspace.WriteEntrypoint(new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/index.md",
                Description = "Duplicate root",
                Tags = ["Route"],
            });
        }

        workspace.WriteRoutedMarkdown(
            FirstCandidate,
            "Collision leaf",
            ["Route"],
            "# Collision leaf\n");
        workspace.WriteEntrypoint(new RouteInspectProfileIntegrationEntrypoint
        {
            RelativePath = SecondCandidate,
            Description = "Collision entrypoint",
            Tags = ["Route"],
        });
        return workspace;
    }

    internal static string ExpectedPrompt()
    {
        return string.Join(
            Environment.NewLine,
            [
                $"{CollisionId} matches 2 sources. Which one?",
                string.Empty,
                $"  1. {FirstCandidate}",
                $"  2. {SecondCandidate}",
                string.Empty,
                "Choose a number (1-2), or press Enter to cancel:",
            ]) + Environment.NewLine;
    }
}

internal sealed record RouteInspectDirectInteractionRun
{
    public required RouteInspectResult Result { get; init; }

    public required string Prompt { get; init; }

    public required string? RemainingInput { get; init; }
}
