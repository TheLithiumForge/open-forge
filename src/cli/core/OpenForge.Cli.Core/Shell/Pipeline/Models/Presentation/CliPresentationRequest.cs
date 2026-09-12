using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

internal sealed record CliPresentationRequest<TResult>(
    TResult Result,
    CliPresentation Presentation)
    where TResult : ICliCommandResult;
