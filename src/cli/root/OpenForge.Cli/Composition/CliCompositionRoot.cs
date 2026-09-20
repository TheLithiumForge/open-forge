using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Libraries.Operational;

using OpenForge.Cli.Core.Framework.Distribution.Operational;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Operational;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Presentation.Models;

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
        var interaction = CreateInteraction(inputs);
        var physicalPathResolver = new PhysicalPathResolver();
        var sourceSessionReader = new SourceReadSessionReader(physicalPathResolver);
        var routeSourceInspector = new RouteSourceInspector(
            sourceSessionReader,
            physicalPathResolver);
        var operationalContributors = new OperationalContributorCatalogue(
            new WorkspaceEntryOperationalContributor(
                new WorkspacePathObserver(physicalPathResolver)),
            new RecoveryResidualOperationalContributor(
                new RecoveryBundleTargetStateReader(physicalPathResolver),
                physicalPathResolver),
            new RouteOperationalContributor(
                new RouteObservationReader(
                    routeSourceInspector,
                    new RouteGeneratedNavigationReader(physicalPathResolver),
                    new RouteDoctorGeneratedNavigationReader())),
            new LocalReferenceOperationalContributor(
                routeSourceInspector,
                new SourceLinkDestinationResolver(
                    (workspace, lexicalPath) => physicalPathResolver.ResolveCandidate(
                        workspace.LexicalRoot,
                        workspace.PhysicalRoot,
                        lexicalPath),
                    StrictUtf8FileReader.ReadAsync,
                    new MarkdownDocumentParser().Parse),
                new LocalReferenceCandidateReader()),
            new FrameworkLifecycleOperationalContributor(
                physicalPathResolver,
                new FrameworkLifecycleTargetReader(physicalPathResolver)),
            new ExtensionLifecycleOperationalContributor(
                physicalPathResolver,
                new ExtensionSourceReader(physicalPathResolver),
                new ExtensionLifecycleTargetReader(physicalPathResolver)),
            new LibraryOperationalContributor());
        var route = CliRouteComposer.Compose(interaction, inputs.LockStoreRoot);
        var standalone = CliStandaloneComposer.Compose(
            interaction,
            inputs.LockStoreRoot,
            operationalContributors,
            physicalPathResolver);
        var extension = CliExtensionComposer.Compose(
            interaction,
            inputs.LockStoreRoot);
        var library = CliLibraryComposer.Compose(interaction);
        var tree = CliCommandTree.Create(
            CreateRootHelp(),
            [route.Branch, extension.Branch, library.Branch],
            [
                route.ListBinding,
                route.InspectBinding,
                route.InitBinding,
                route.CreateBinding,
                route.UpdateBinding,
                route.MoveBinding,
                route.RemoveBinding,
                standalone.FindBinding,
                standalone.IndexBinding,
                standalone.StatusBinding,
                standalone.DoctorBinding,
                standalone.RepairBinding,
                standalone.CleanupBinding,
                standalone.ContextBinding,
                standalone.ReferencesBinding,
                standalone.InstallBinding,
                standalone.UpdateBinding,
                extension.ListBinding,
                extension.InspectBinding,
                extension.CreateBinding,
                extension.InstallBinding,
                extension.UpdateBinding,
                extension.RemoveBinding,
                library.ListBinding,
                library.InspectBinding,
                library.AttachBinding,
                library.SyncBinding,
                library.DetachBinding,
            ],
            rootLeaves: standalone.RootLeaves);
        return new CliCoreApplication(
            process,
            tree,
            new CliWorkspaceSelector(physicalPathResolver));
    }

    internal static CliInteractionComposition CreateInteraction(CliCompositionInputs inputs)
    {
        var canPrompt = !inputs.StandardInputRedirected && !inputs.PromptOutputRedirected;
        var terminal = inputs.Terminal ?? new CliTerminal(new CliTerminalCapabilities(canPrompt, false, false),
            async (content, token) =>
            {
                await inputs.PromptOutput.WriteAsync(content, token).ConfigureAwait(false);
                await inputs.PromptOutput.FlushAsync(token).ConfigureAwait(false);
            }, token => inputs.StandardInput.ReadLineAsync(token),
            _ => throw new InvalidOperationException("No key reader was supplied."));
        return new(terminal, new CliPrompts(terminal, inputs.StandardErrorColor));
    }

    private static CliHelpContent CreateRootHelp()
        => new(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGettingStarted(),
                ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGettingStartedExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingCommandHelp(),
                ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpCommandHelpDescription())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingManagedContent(),
                ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpManagedContentDescription())),
        ]);
}
