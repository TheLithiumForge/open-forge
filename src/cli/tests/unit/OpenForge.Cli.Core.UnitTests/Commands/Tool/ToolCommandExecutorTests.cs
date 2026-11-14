using OpenForge.Cli.Core.Commands.Tool.Shared.Execution;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool;

[Trait("Feature", "tool-execution"), Trait("Evidence", "Unit")]
public sealed class ToolCommandExecutorTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Declared commands persist planned, actual-start and exit evidence in their requested order")]
    public async Task RecordsOrderedExecution()
    {
        var input = Input(ToolVocabulary.UninstallAction, ToolVocabulary.InstallAction);
        var records = new List<ToolRunRecord>();
        var called = new List<string>();
        var executor = new ToolCommandExecutor(async (request, token) =>
        {
            called.Add(request.Action);
            Assert.Equal(ToolCommandState.Planned, records[^1].Commands[called.Count - 1].State);
            Assert.NotNull(request.Started);
            await request.Started(token);
            Assert.Equal(ToolCommandState.Started, records[^1].Commands[called.Count - 1].State);
            return new(ToolCommandState.Exited, 0, null);
        }, (record, _) => { records.Add(record); return ValueTask.CompletedTask; });
        var result = await executor.ExecuteAsync(input, Token);
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.CompletedCommands);
        Assert.Equal(input.Commands.Select(command => command.Action), called);
        Assert.Equal(5, records.Count);
        Assert.All(result.Record.Commands, command => Assert.Equal(ToolCommandState.Exited, command.State));
        Assert.All(result.Record.Commands, command => Assert.Equal(0, command.ExitCode));
        Assert.Equal(result.Record.Commands.Select(command => command.State), result.Record.ToSummary().Commands.Select(command => command.State));
    }

    [Fact(DisplayName = "A nonzero exit stops later declared commands and keeps their planned evidence")]
    public async Task StopsAfterFailedCommand()
    {
        var called = 0;
        var executor = new ToolCommandExecutor(async (request, token) =>
        {
            called++;
            Assert.NotNull(request.Started);
            await request.Started(token);
            return new(ToolCommandState.Exited, 7, null);
        }, static (_, _) => ValueTask.CompletedTask);
        var result = await executor.ExecuteAsync(Input(ToolVocabulary.UninstallAction, ToolVocabulary.InstallAction), Token);
        Assert.False(result.Succeeded);
        Assert.Equal(1, called);
        Assert.Equal(ToolCommandState.Failed, result.Record.State);
        Assert.Equal(7, result.Record.Commands[0].ExitCode);
        Assert.Equal(ToolCommandState.Planned, result.Record.Commands[1].State);
    }

    [Fact(DisplayName = "Failure to save the complete proposed execution prevents every process launch")]
    public async Task RequiresPlanPublication()
    {
        var called = 0;
        var executor = new ToolCommandExecutor((_, _) => { called++; return ValueTask.FromResult(new ToolProcessOutcome(ToolCommandState.Exited, 0, null)); },
            static (_, _) => throw new IOException("fixture failure"));
        var result = await executor.ExecuteAsync(Input(ToolVocabulary.InstallAction), Token);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.PublicationCause);
        Assert.Equal(0, called);
        Assert.Equal(ToolCommandState.Planned, result.Record.Commands[0].State);
    }

    [Fact(DisplayName = "Failure to save an actual process start stops the run and is kept alongside its outcome")]
    public async Task KeepsStartPublicationFailure()
    {
        var count = 0;
        var executor = new ToolCommandExecutor(async (request, token) =>
        {
            count++;
            Assert.NotNull(request.Started);
            await request.Started(token);
            return new(ToolCommandState.Exited, 0, null);
        }, static (record, _) => record.State == ToolCommandState.Started
            ? throw new IOException("fixture failure") : ValueTask.CompletedTask);
        var result = await executor.ExecuteAsync(Input(ToolVocabulary.UninstallAction, ToolVocabulary.InstallAction), Token);
        Assert.False(result.Succeeded);
        Assert.Equal(1, count);
        Assert.NotNull(result.PublicationCause);
        Assert.Equal(ToolCommandState.Failed, result.Record.Commands[0].State);
        Assert.Equal(ToolCommandState.Planned, result.Record.Commands[1].State);
    }

    [Fact(DisplayName = "Cancellation after a start still saves interrupted evidence without the cancelled token")]
    public async Task SavesInterruptedEvidence()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        var finalSaved = false;
        var executor = new ToolCommandExecutor(async (request, token) =>
        {
            Assert.NotNull(request.Started);
            await request.Started(token);
            cancellation.Cancel();
            throw new OperationCanceledException(token);
        }, (record, token) =>
        {
            if (record.State == ToolCommandState.Interrupted) { Assert.False(token.IsCancellationRequested); finalSaved = true; }
            return ValueTask.CompletedTask;
        });
        var result = await executor.ExecuteAsync(Input(ToolVocabulary.InstallAction), cancellation.Token);
        Assert.False(result.Succeeded);
        Assert.True(finalSaved);
        Assert.Equal(ToolCommandState.Interrupted, result.Record.Commands[0].State);
    }

    [Fact(DisplayName = "A successful child with failed final evidence remains unfinished")]
    public async Task KeepsFinalPublicationFailure()
    {
        var executor = new ToolCommandExecutor(async (request, token) =>
        {
            Assert.NotNull(request.Started);
            await request.Started(token);
            return new(ToolCommandState.Exited, 0, null);
        }, static (record, _) => record.State == ToolCommandState.Exited
            ? throw new IOException("fixture failure") : ValueTask.CompletedTask);
        var result = await executor.ExecuteAsync(Input(ToolVocabulary.InstallAction), Token);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.PublicationCause);
        Assert.Equal(1, result.CompletedCommands);
        Assert.Equal(ToolCommandState.Exited, result.Record.Commands[0].State);
    }

    private static ToolCommandExecution Input(params string[] actions)
    {
        var snapshot = ToolFixtureSource.ManifestBytes();
        var root = Path.GetFullPath("fixture-install");
        var working = Path.GetFullPath("fixture-workspace");
        var commands = actions.Select(action => new ToolProcessRequest
        {
            Action = action,
            Host = null,
            ExecutableRoot = root,
            Executable = Path.Combine(root, ToolFixtureSource.ArtifactName),
            EntrySha256 = ToolCatalogueReader.Hash(ToolFixtureSource.ArtifactBytes()),
            WorkingDirectory = working,
            Arguments = [action],
            Environment = new(StringComparer.Ordinal),
            TimeoutSeconds = 30,
        }).ToArray();
        var manifest = new ToolCatalogueReader().Definition(snapshot, ToolFixtureSource.ManifestName, false).Manifest;
        return new(new()
        {
            Version = ToolFixtureSource.Version,
            Target = manifest.Targets.Keys.Order(StringComparer.Ordinal).First(),
            ManifestSha256 = ToolCatalogueReader.Hash(snapshot),
            ManifestBytes = snapshot,
            ManifestFormat = "json",
            CodeRoot = ".",
            Hosts = [],
            Action = ToolVocabulary.ConfigureAction,
            State = ToolCommandState.Planned,
            Commands = [],
        }, commands);
    }
}
