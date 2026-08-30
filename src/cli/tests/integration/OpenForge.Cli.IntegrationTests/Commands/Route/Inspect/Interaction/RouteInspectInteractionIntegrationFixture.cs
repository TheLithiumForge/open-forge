using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Interaction;
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
        var session = new CliInteractiveSession(input, prompt, canPrompt);
        var operation = RouteInspectOperationFactory.Create(session);
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
                $"The source ID `{CollisionId}` matches more than one source.",
                string.Empty,
                $"1. {FirstCandidate}",
                $"2. {SecondCandidate}",
                string.Empty,
                "Choose a source by number or exact path: ",
            ]);
    }
}

internal sealed record RouteInspectDirectInteractionRun
{
    public required RouteInspectResult Result { get; init; }

    public required string Prompt { get; init; }

    public required string? RemainingInput { get; init; }
}
