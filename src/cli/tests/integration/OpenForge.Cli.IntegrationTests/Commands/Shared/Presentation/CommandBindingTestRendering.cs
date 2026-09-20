using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.UnitTests.Commands.Shared.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.Presentation;

// These tests isolate typed command composition; real command output has its own renderer evidence.
internal static class CommandBindingTestRendering
{
    private static readonly CommandBindingTestJsonContext Json = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });

    internal static CliReportRendering<TResult, CommandBindingTestData> Create<TResult>(
        string headline = "human", Action<TResult>? selected = null, Action? textRendered = null,
        IReadOnlyList<string>? diagnostics = null) where TResult : ICliCommandResult => new()
        {
            Selector = (result, selection) =>
            {
                selected?.Invoke(result);
                return new CliReport<CommandBindingTestData>
                {
                    Command = result.Command,
                    Status = result.Status,
                    Headline = new(headline, CliHeadlineKind.Done),
                    Data = new("json"),
                    Diagnostics = diagnostics ?? [],
                };
            },
            DataTextRenderer = (data, selection, style) => { textRendered?.Invoke(); return new([]); },
            DataJsonTypeInfo = Json.CommandBindingTestData,
            Shape = CliCommandShape.Summary,
        };
}

[JsonSerializable(typeof(CommandBindingTestData))]
internal sealed partial class CommandBindingTestJsonContext : JsonSerializerContext;
