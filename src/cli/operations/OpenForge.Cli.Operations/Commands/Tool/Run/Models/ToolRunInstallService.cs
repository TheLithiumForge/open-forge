using OpenForge.Cli.Core.Commands.Tool.Install.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Run.Models;

// Registration point: Writers supplies normal Install planning, offer and all consent.
// A null result declines the offer. The default stub leaves the missing-tool refusal intact.
internal delegate ValueTask<ToolInstallResult?> ToolRunInstallService(ToolInstallRequest request, CancellationToken token);
