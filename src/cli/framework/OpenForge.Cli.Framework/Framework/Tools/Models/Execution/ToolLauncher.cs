using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Execution;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolLauncher([property: JsonRequired] string Executable, [property: JsonRequired] string[] Args);
