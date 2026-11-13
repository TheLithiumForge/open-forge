using System.Text.Json;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration.Models;

internal sealed record ToolHookMatch(string GroupPointer, string EntryPointer, JsonElement Group, JsonElement Entry);
