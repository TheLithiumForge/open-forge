using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;

internal delegate CliTextDocument CliDataTextRenderer<in TData>(TData data, CliSelection selection, CliTextStyle style)
    where TData : class;
