using System.CommandLine;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Parsing.Shared.Replay;

namespace OpenForge.Cli.Core.UnitTests.Shell.Parsing.Shared.Replay;

public sealed class CliRequestReplayReaderTests
{
    [Theory(DisplayName = "Apply commands remove native preview options while retaining the selected request")]
    [InlineData("--dry-run"), InlineData("--dry-run=true"), InlineData("--dry-run:true")]
    [Trait("Feature", "command-retry"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ApplyPreservesRequest(string preview)
    {
        var root = new RootCommand();
        var command = new Command("remove");
        command.Arguments.Add(new Argument<string[]>("ids"));
        command.Options.Add(new Option<bool>("--dry-run"));
        command.Options.Add(new Option<string>("--workspace"));
        command.Options.Add(new Option<string>("--allow-path"));
        root.Subcommands.Add(command);
        string[] arguments = ["remove", "toolkit", "planning", "--workspace", "other workspace", "--allow-path", "notes folder", preview];
        var parse = root.Parse(arguments);
        Assert.Empty(parse.Errors);
        var replay = CliRequestReplayReader.Read(new CliBindingParse(parse, arguments), command);
        Assert.Equal("open-forge remove toolkit planning --workspace 'other workspace' --allow-path 'notes folder'", replay.ApplyCommand);
    }

    [Theory(DisplayName = "Confirmation retries replace an explicit false automatic option without losing other options")]
    [InlineData("--automatic=false"), InlineData("--automatic:false")]
    [Trait("Feature", "command-retry"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ConfirmationReplacesFalseAutomatic(string automatic)
    {
        var root = new RootCommand();
        var command = new Command("cleanup");
        command.Options.Add(new Option<bool>("--automatic"));
        command.Options.Add(new Option<string>("--workspace"));
        root.Subcommands.Add(command);
        string[] arguments = ["cleanup", "--workspace", "other workspace", automatic];
        var parse = root.Parse(arguments);
        Assert.Empty(parse.Errors);
        var replay = CliRequestReplayReader.Read(new CliBindingParse(parse, arguments), command);
        Assert.Equal("open-forge cleanup --workspace 'other workspace' --automatic", replay.AutomaticCommand);
    }

    [Theory(DisplayName = "A diagnostic retry preserves operands and every other parsed option across native detail spellings")]
    [InlineData("--detail", "full"), InlineData("--detail=full", null), InlineData("--detail:full", null)]
    [Trait("Feature", "command-retry"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void DiagnosticRetryPreservesRequest(string detail, string? value)
    {
        var root = new RootCommand();
        var command = new Command("references");
        command.Arguments.Add(new Argument<string>("source"));
        command.Options.Add(new Option<string>("--direction"));
        command.Options.Add(new Option<string>("--detail"));
        command.Options.Add(new Option<string>("--workspace"));
        root.Subcommands.Add(command);
        string[] detailArguments = value is null ? [detail] : [detail, value];
        string[] arguments = ["references", "team's notes", "--direction", "out", "--workspace", "other workspace", .. detailArguments];
        var parse = root.Parse(arguments);
        Assert.Empty(parse.Errors);
        var replay = CliRequestReplayReader.Read(new CliBindingParse(parse, arguments), command);
        Assert.Equal("open-forge references", replay.Command);
        var quoted = OperatingSystem.IsWindows() ? "'team''s notes'" : "'team'\\''s notes'";
        Assert.Equal($"open-forge references {quoted}", replay.SubjectCommand);
        Assert.Equal($"open-forge references {quoted} --direction out --workspace 'other workspace' --detail debug", replay.DebugCommand);
    }
}
