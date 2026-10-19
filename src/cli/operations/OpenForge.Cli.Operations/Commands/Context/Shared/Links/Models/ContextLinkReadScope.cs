using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Links.Models;

internal sealed record ContextLinkReadScope(ContextRequest Request, ContextGraph Graph, WorkspaceSettingsDocument Settings);
