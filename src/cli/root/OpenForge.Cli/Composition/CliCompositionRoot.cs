using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
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
        var interactiveSession = CreateInteractiveSession(inputs);
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
                new LifecycleStore(physicalPathResolver),
                new FrameworkLifecycleTargetReader(physicalPathResolver)),
            new ExtensionLifecycleOperationalContributor(
                new LifecycleDocumentReader(physicalPathResolver),
                new ExtensionSourceReader(physicalPathResolver),
                new ExtensionLifecycleTargetReader(physicalPathResolver),
                new LifecycleOwnershipReader(physicalPathResolver)),
            new LibraryOperationalContributor());
        var route = CliRouteComposer.Compose(interactiveSession, inputs.LockStoreRoot);
        var standalone = CliStandaloneComposer.Compose(
            interactiveSession,
            inputs.LockStoreRoot,
            operationalContributors,
            new LifecycleDocumentSnapshotReader(physicalPathResolver));
        var extension = CliExtensionComposer.Compose(
            interactiveSession,
            inputs.LockStoreRoot);
        var library = CliLibraryComposer.Compose(interactiveSession);
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

    private static CliInteractiveSession CreateInteractiveSession(CliCompositionInputs inputs)
        => new(
            standardInput: inputs.StandardInput,
            promptOutput: inputs.PromptOutput,
            canPrompt: !inputs.StandardInputRedirected && !inputs.PromptOutputRedirected);

    private static CliHelpContent CreateRootHelp()
        => new(
        [
            new CliHelpSection(
                "Getting started",
                """
                  open-forge install --dry-run
                  open-forge context
                  open-forge route list
                  open-forge doctor
                """),
            new CliHelpSection(
                "Command help",
                """
                  Use open-forge <command> --help for options and examples.
                  Use open-forge route --help, open-forge extension --help, or
                  open-forge library --help to list their subcommands.
                """),
            new CliHelpSection(
                "Managed content",
                "  Install establishes or verifies Framework management. Update reconciles managed files; --force and --prune permit only their documented changes."),
        ]);
}
