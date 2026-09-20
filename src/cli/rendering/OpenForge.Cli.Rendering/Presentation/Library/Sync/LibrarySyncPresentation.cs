using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Sync.Models;
using OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Help;
using OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Sync;

internal static class LibrarySyncPresentation
{
    private static readonly LibrarySyncDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<LibrarySyncResult, LibrarySyncData> Rendering { get; } = new()
    {
        Selector = LibrarySyncReportSelector.Select,
        DataTextRenderer = LibrarySyncDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.LibrarySyncData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            ShowWorkspace = selected.Selection.Detail >= CliDetail.Standard,
            TextEffects = [],
            TextCounts = [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => LibrarySyncHelpSections.Create();
}
