using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Templates;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

internal static class ToolEntryDeclarations
{
    internal const string PathCommand = "open-forge";

    internal static ToolEntryRewriteRequest Resolve(ToolEntryRewriteRequest request)
    {
        var values = request.Values.ToDictionary(pair => pair.Key, pair => (string?)pair.Value, StringComparer.Ordinal);
        string Render(string value)
        {
            var result = ToolTemplate.Render(value, values);
            if (result.Contains("${", StringComparison.Ordinal)) throw new InvalidDataException("A registration value contains an unresolved template.");
            return result;
        }
        string FileName(string value)
        {
            var file = Render(value);
            if (file.StartsWith("./", StringComparison.Ordinal)) file = file[2..];
            if (!ToolIdentity.IsRelativePath(file)) throw new InvalidDataException("A registration file must be workspace-relative.");
            return file;
        }
        ToolCommandValue Command(ToolCommandValue value) => new(Render(value.Command), [.. value.Args.Select(Render)]);
        ToolHookCommandRewrite Hook(ToolHookCommandRewrite hook, string file)
        {
            var chosen = ToolHookPlatformValues.Resolve(new ToolHookCommandValue(hook.Expected, hook.Replacement), hook.PlatformOverrides, request.Values);
            var result = hook with
            {
                File = file,
                Expected = Render(chosen.Expected),
                Replacement = Render(chosen.Replacement),
                PlatformOverrides = null,
                RequiredReplacementSubstring = hook.RequiredReplacementSubstring is null ? null : Render(hook.RequiredReplacementSubstring),
            };
            if (result.MatchCommand && result.Host != ToolVocabulary.ClaudeCodeHost)
                throw new InvalidDataException("Event command matching requires the Claude hook format.");
            if (result.RequiredReplacementSubstring is { } required && (string.IsNullOrWhiteSpace(required) || !result.Replacement.Contains(required, StringComparison.Ordinal)))
                throw new InvalidDataException("The hook replacement is missing its required uninstaller substring.");
            return result;
        }
        if (values.TryGetValue(ToolVocabulary.OpenForgeVariable, out var launch)
            && launch != PathCommand && (launch is null || !Path.IsPathFullyQualified(launch)))
            throw new InvalidDataException("The host must supply the PATH command or an absolute running executable.");
        var declined = request.DeclinedComponents.Select(component => component with
        {
            DeclinedEntries = [.. component.DeclinedEntries.Select(entry => entry with
            {
                File = FileName(entry.File),
                Expected = RenderJson(ToolHookPlatformValues.Resolve(entry.Expected, entry.PlatformOverrides, request.Values), Render),
                PlatformOverrides = null,
                AbsentCommands = entry.AbsentCommands is null ? null : [.. entry.AbsentCommands.Select(Render)],
            })],
        }).ToArray();
        var skipped = declined.Select(component => component.Id).ToHashSet(StringComparer.Ordinal);
        var rewrites = request.Rewrites.Where(rewrite => rewrite.Component is null || !skipped.Contains(rewrite.Component)).Select(rewrite =>
        {
            var file = FileName(rewrite.File);
            return rewrite switch
            {
                ToolJsonCommandRewrite json => json with { File = file, Expected = Command(json.Expected), Replacement = Command(json.Replacement) },
                ToolTomlCommandRewrite toml => toml with { File = file, Expected = Command(toml.Expected), Replacement = Command(toml.Replacement) },
                ToolHookCommandRewrite hook => Hook(hook, file),
                ToolMarkedLineRewrite line => (ToolEntryRewrite)(line with { File = file, Expected = Render(line.Expected), Replacement = Render(line.Replacement) }),
                _ => throw new InvalidDataException("The registration rewrite kind is unavailable."),
            };
        }).ToArray();
        if (request.Rewrites.Select(rewrite => rewrite.Id).Distinct(StringComparer.Ordinal).Count() != request.Rewrites.Length
            || declined.Select(component => component.Id).Distinct(StringComparer.Ordinal).Count() != declined.Length
            || declined.Any(component => component.Flag != ToolComponentFlags.PromptHook))
            throw new InvalidDataException("Registration identities or component flags are invalid.");
        if (rewrites.Any(rewrite => !ToolIdentity.IsId(rewrite.Id) || !ToolVocabulary.Hosts.Contains(rewrite.Host))
            || declined.Any(component => !ToolIdentity.IsId(component.Id) || component.DeclinedEntries.Any(entry => !ToolVocabulary.Hosts.Contains(entry.Host))))
            throw new InvalidDataException("A registration declaration has an invalid ID or host.");
        var files = rewrites.Select(rewrite => rewrite.File).Concat(declined.SelectMany(component => component.DeclinedEntries.Select(entry => entry.File))).Distinct(StringComparer.Ordinal).ToArray();
        if (files.Select(PortableWorkspacePath.CreatePortableKey).Distinct(StringComparer.Ordinal).Count() != files.Length)
            throw new InvalidDataException("Registration file paths contain portable aliases.");
        if (declined.SelectMany(component => component.DeclinedEntries).Any(entry =>
            entry.MatchCommand && entry.Host != ToolVocabulary.ClaudeCodeHost || !entry.MatchCommand && (entry.RemoveCreatedEmptyGroup || entry.AbsentCommands is not null)))
            throw new InvalidDataException("Hook-group removal and absence spellings require an event command selector.");
        return request with { Rewrites = rewrites, DeclinedComponents = declined };
    }

    private static JsonElement RenderJson(JsonElement value, Func<string, string> render)
    {
        var node = JsonNode.Parse(value.GetRawText());
        JsonNode? Visit(JsonNode? current)
        {
            if (current is JsonValue scalar && scalar.TryGetValue<string>(out var text)) return JsonValue.Create(render(text));
            if (current is JsonObject obj)
                foreach (var key in obj.Select(pair => pair.Key).ToArray()) obj[key] = Visit(obj[key]);
            if (current is JsonArray array)
                for (var index = 0; index < array.Count; index++) array[index] = Visit(array[index]);
            return current;
        }
        using var document = JsonDocument.Parse(Visit(node)?.ToJsonString() ?? "null");
        return document.RootElement.Clone();
    }
}
