using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Core.Commands.Tool.Install;
using OpenForge.Cli.Core.Commands.Tool.Update;
using OpenForge.Cli.Core.Commands.Tool.Remove;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Hosting;

namespace OpenForge.Cli.Composition.Shared.ToolComposition;

internal sealed class ToolCompositionFactory(ToolCatalogue catalogue, CliInteractionComposition interaction, CliToolHostInputs? inputs)
{
    internal IToolVersionService Versions { get; } = ToolHostFactory.CreateVersions();

    internal ToolReadObserver CreateReadObserver()
    {
        var graph = new ToolCompositionGraph(catalogue, inputs, static () => false);
        return new(catalogue, graph.Records, graph.Observations, graph.Homes.HomePath);
    }

    internal ToolWorkspaceObservationReader CreateWorkspaceReader() => new(Versions, () =>
    {
        var graph = new ToolCompositionGraph(catalogue, inputs, static () => false);
        return new(catalogue, graph.Records, graph.Observations, graph.Homes.HomePath);
    });

    internal async ValueTask<ToolInstallOperation> CreateInstallAsync(ToolWriteRequest request, CancellationToken token)
        => new(await CreateWriterAsync(request, token).ConfigureAwait(false));
    internal async ValueTask<ToolUpdateOperation> CreateUpdateAsync(ToolWriteRequest request, CancellationToken token)
        => new(await CreateWriterAsync(request, token).ConfigureAwait(false));
    internal async ValueTask<ToolRemoveOperation> CreateRemoveAsync(ToolWriteRequest request, CancellationToken token)
        => new(await CreateWriterAsync(request, token).ConfigureAwait(false));

    private async ValueTask<ToolWriteOperation> CreateWriterAsync(ToolWriteRequest request, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var prompts = new ToolCompositionInteraction(interaction.Prompts);
        var graph = new ToolCompositionGraph(catalogue, inputs, () => prompts.HasLivePathConsent);
        var settings = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), request.Workspace, token).ConfigureAwait(false);
        if (settings.State is WorkspaceSettingsReadState.Invalid or WorkspaceSettingsReadState.Unavailable)
            throw new IOException(settings.Cause);
        var nativeHomes = new Dictionary<ToolNativeHomePolicy, string>();
        var clients = new Dictionary<string, string>(StringComparer.Ordinal);
        var nativeEnvironment = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var profile in catalogue.Entries.Values.SelectMany(entry => entry.Profiles))
        {
            try
            {
                var home = graph.Homes.Resolve(profile);
                nativeHomes[profile.NativeHomePolicy] = home.NativeHomePath;
                foreach (var pair in home.Clients.Bindings) clients[pair.Key] = pair.Value;
                foreach (var pair in home.NativeEnvironment) nativeEnvironment[pair.Key] = pair.Value;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            { /* Unavailable policies stay absent. D blocks a selected profile that needs one. */ }
        }
        var facts = new ToolWriteHostFacts
        {
            HomePath = graph.Homes.HomePath,
            MainRoot = settings.Document.MainRoot,
            OpenForgeVersion = CliBuildVersion.InformationalVersion,
            NativeEnvironment = nativeEnvironment,
            NativeHomes = nativeHomes,
            ClientRoots = clients,
            Target = await ToolHostFactory.ObserveTargetAsync(graph.Processes, request.Workspace.PhysicalRoot, token).ConfigureAwait(false),
            ProjectManagers = ToolProjectManagerReader.Read(request.Workspace, token),
            Path = await graph.PathHost.ObserveAsync(graph.Homes.HomePath, token).ConfigureAwait(false),
        };
        var publisher = new ToolWriteRecordPublisher(graph.Records);
        var application = new ToolWriteApplication(new(graph.Channels, Versions, publisher), new(graph.Processes, graph.Backups, graph.Config),
            new(graph.Config, publisher), new(graph.Channels, graph.PathHost, publisher));
        return new(new ToolWritePlanner(catalogue, new(Versions, graph.Channels, graph.Config, graph.Config)),
            application, publisher, facts, prompts.Create());
    }
}
