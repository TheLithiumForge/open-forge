using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Composition;

internal static class CliCompositionRoot
{
    internal static CliCoreApplication Create(CliProcessIdentity process)
        => Create(
            process,
            new CliCompositionInputs
            {
                StandardInput = TextReader.Null,
                PromptOutput = TextWriter.Null,
                StandardInputRedirected = true,
                PromptOutputRedirected = true,
            });

    internal static CliCoreApplication Create(
        CliProcessIdentity process,
        CliCompositionInputs inputs)
    {
        var interactiveSession = CreateInteractiveSession(inputs);
        var route = CliRouteComposer.Compose(interactiveSession, inputs.LockStoreRoot);
        var standalone = CliStandaloneComposer.Compose(interactiveSession, inputs.LockStoreRoot);
        var extension = CliExtensionComposer.Compose(
            interactiveSession,
            inputs.LockStoreRoot);
        var tree = CliCommandTree.Create(
            CreateRootHelp(),
            [route.Branch, extension.Branch],
            [
                route.ListBinding,
                route.InspectBinding,
                route.InitBinding,
                route.CreateBinding,
                route.UpdateBinding,
                route.MoveBinding,
                standalone.FindBinding,
                standalone.IndexBinding,
                standalone.InstallBinding,
                standalone.ReferencesBinding,
                extension.ListBinding,
                extension.InspectBinding,
                extension.CreateBinding,
                extension.InstallBinding,
                standalone.ContextBinding,
            ],
            rootLeaves: standalone.RootLeaves);
        return new CliCoreApplication(
            process,
            tree,
            new CliWorkspaceSelector(new PhysicalPathResolver()));
    }

    private static CliInteractiveSession CreateInteractiveSession(CliCompositionInputs inputs)
        => new(
            standardInput: inputs.StandardInput,
            promptOutput: inputs.PromptOutput,
            canPrompt: !inputs.StandardInputRedirected && !inputs.PromptOutputRedirected);

    private static CliHelpContent CreateRootHelp()
        => new(
        [
            new CliHelpSection(
                "Product",
                $"  Open Forge CLI (`{CliSyntaxDefinitions.ExecutableName}`)."),
            new CliHelpSection(
                "Discovery",
                """
                  route list        List routed sources and descendants at a structural depth.
                  route inspect     Explain one source's route behavior without returning authored content.
                  route init        Initialize every missing entrypoint in one exact route chain.
                  route update      Update selected fields or an eligible Template body on one routed source.
                  route move        Move one routed source or category while preserving its route meaning.
                  find              Find Markdown sources by authored tags and structural headings.
                  extension list    List installed and available Extension packages.
                  extension inspect Inspect one installed or available Extension package.
                  extension create  Create one local Extension package scaffold.
                  extension install Install reviewed Extension packages into a Framework workspace.
                """),
            new CliHelpSection(
                heading: "Lifecycle",
                body: "  Framework management is established or verified by install without reconciling managed divergence."),
        ]);
}

internal sealed record CliCompositionInputs
{
    public required TextReader StandardInput { get; init; }

    public required TextWriter PromptOutput { get; init; }

    public required bool StandardInputRedirected { get; init; }

    public required bool PromptOutputRedirected { get; init; }

    public WorkspaceLockStoreRoot? LockStoreRoot { get; init; }
}
