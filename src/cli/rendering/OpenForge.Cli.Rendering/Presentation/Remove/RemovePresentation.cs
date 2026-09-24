using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Remove.Models;
using OpenForge.Cli.Core.Presentation.Remove.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Remove.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Remove;

internal static class RemovePresentation
{
    private static readonly RemoveDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<RemoveResult, RemoveData> Rendering { get; } = new()
    {
        Selector = RemoveReportSelector.Select,
        DataTextRenderer = RemoveDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.RemoveData,
        Shape = CliCommandShape.ChangeReport,
    };

    internal static CliHelpContent CreateHelp()
        => new(
        [
            new CliHelpSection("Usage", "  open-forge remove <target> [--kind path|route|extension|library] [--dry-run] [--automatic] [--allow-path path]"),
            new CliHelpSection("Kinds", "  path is the default. route, extension, and library delegate to their typed removal operations."),
        ]);
}
