using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

// The fixed projection follows the Tool record Interface. It contains authored recipe facts only.
internal static class ToolProfileDigest
{
    internal static string Compute(ToolVersionProfile profile, Func<string, byte[]> readTemplate)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(profile.Id), profile.Id);
            writer.WritePropertyName(nameof(profile.Range));
            writer.WriteStartObject();
            writer.WriteNumber(nameof(profile.Range.Major), profile.Range.Major);
            writer.WriteNumber(nameof(profile.Range.Minor), profile.Range.Minor);
            writer.WriteEndObject();
            writer.WriteString(nameof(profile.MinimumOpenForgeVersion), profile.MinimumOpenForgeVersion);
            writer.WriteString(nameof(profile.MinimumToolVersion), profile.MinimumToolVersion);
            writer.WriteString(nameof(profile.NativeHomePolicy), profile.NativeHomePolicy.ToString());
            writer.WritePropertyName(nameof(profile.Setup));
            Setup(writer, profile.Setup);
            writer.WritePropertyName(nameof(profile.Uninstall));
            Setup(writer, profile.Uninstall);
            writer.WriteStartArray(nameof(profile.Changes));
            foreach (var change in profile.Changes) Change(writer, change, readTemplate);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        using var document = JsonDocument.Parse(stream.ToArray());
        using var ordered = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ordered)) Canonical(writer, document.RootElement);
        return FileExpectation.Hash(ordered.ToArray());
    }

    internal static byte[] Template(string resource)
    {
        // Hash the authored resource bytes. Apply expands its strict UTF-8 content separately.
        using var input = typeof(ToolProfileDigest).Assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidDataException($"The managed-file template resource is unavailable: {resource}.");
        using var output = new MemoryStream();
        input.CopyTo(output);
        return output.ToArray();
    }

    private static void Setup(Utf8JsonWriter writer, ToolSetupSpec setup)
    {
        writer.WriteStartObject();
        Strings(writer, nameof(setup.Arguments), setup.Arguments);
        if (setup.DryRunArguments is { } dryRun) Strings(writer, nameof(setup.DryRunArguments), dryRun);
        else writer.WriteNull(nameof(setup.DryRunArguments));
        Environment(writer, nameof(setup.Environment), setup.Environment);
        writer.WriteNumber(nameof(setup.Timeout), setup.Timeout.Ticks);
        writer.WriteNumber(nameof(setup.MaximumOutputBytes), setup.MaximumOutputBytes);
        writer.WritePropertyName(nameof(setup.NativePlan));
        if (setup.NativePlan is { } plan)
        {
            writer.WriteStartObject();
            Strings(writer, nameof(plan.Arguments), plan.Arguments);
            writer.WriteString(nameof(plan.Parser), plan.Parser.ToString());
            writer.WriteNumber(nameof(plan.Timeout), plan.Timeout.Ticks);
            writer.WriteNumber(nameof(plan.MaximumOutputBytes), plan.MaximumOutputBytes);
            writer.WriteEndObject();
        }
        else writer.WriteNullValue();
        writer.WriteStartArray(nameof(setup.PossibleOutputs));
        foreach (var output in setup.PossibleOutputs)
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(output.PathTemplate), output.PathTemplate);
            writer.WriteString(nameof(output.Scope), output.Scope.ToString());
            writer.WriteString(nameof(output.Match), output.Match.ToString());
            writer.WriteString(nameof(output.Purpose), output.Purpose.ToString());
            writer.WriteString(nameof(output.OperatingSystem), output.OperatingSystem?.ToString());
            writer.WriteString(nameof(output.ClientId), output.ClientId);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteStartArray(nameof(setup.Effects));
        foreach (var effect in setup.Effects)
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(effect.Id), effect.Id);
            writer.WriteString(nameof(effect.Scope), effect.Scope.ToString());
            writer.WriteString(nameof(effect.Kind), effect.Kind.ToString());
            writer.WriteString(nameof(effect.Description), effect.Description);
            writer.WriteString(nameof(effect.PathTemplate), effect.PathTemplate);
            writer.WriteString(nameof(effect.Selector), effect.Selector);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void Change(Utf8JsonWriter writer, ToolConfigChange change, Func<string, byte[]> readTemplate)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(change.Id), change.Id);
        writer.WriteString(nameof(change.Scope), change.Scope.ToString());
        writer.WriteString(nameof(change.InitialState), change.InitialState.ToString());
        writer.WriteString(nameof(change.Description), change.Description);
        switch (change)
        {
            case ToolJsonValueChange json:
                writer.WriteString(nameof(json.PathTemplate), json.PathTemplate);
                writer.WriteString(nameof(json.Pointer), json.Pointer);
                writer.WriteString(nameof(json.Operation), json.Operation.ToString());
                writer.WritePropertyName(nameof(json.Value));
                if (json.Value is { } value) Canonical(writer, value);
                else writer.WriteNullValue();
                break;
            case ToolMarkedBlockChange block:
                writer.WriteString(nameof(block.PathTemplate), block.PathTemplate);
                writer.WriteString(nameof(block.StartMarker), block.StartMarker);
                writer.WriteString(nameof(block.EndMarker), block.EndMarker);
                writer.WriteString(nameof(block.ContentTemplate), FileExpectation.Hash(ToolConfigBytes.Utf8.GetBytes(block.ContentTemplate)));
                writer.WriteString(nameof(block.WhenNotIgnored), block.WhenNotIgnored?.WorkspaceRelativePath);
                break;
            case ToolManagedFileChange file:
                writer.WriteString(nameof(file.PathTemplate), file.PathTemplate);
                writer.WriteString(nameof(file.TemplateResource), FileExpectation.Hash(readTemplate(file.TemplateResource)));
                break;
            case ToolNativeConfigChange native:
                writer.WriteString(nameof(native.AllowListId), native.AllowListId);
                Strings(writer, nameof(native.ApplyArguments), native.ApplyArguments);
                Strings(writer, nameof(native.ReadArguments), native.ReadArguments);
                Environment(writer, nameof(native.ReadEnvironment), native.ReadEnvironment);
                writer.WriteString(nameof(native.ValueParserId), native.ValueParserId);
                if (native.ReadTimeout is { } timeout) writer.WriteNumber(nameof(native.ReadTimeout), timeout.Ticks);
                else writer.WriteNull(nameof(native.ReadTimeout));
                if (native.ReadMaximumOutputBytes is { } bound) writer.WriteNumber(nameof(native.ReadMaximumOutputBytes), bound);
                else writer.WriteNull(nameof(native.ReadMaximumOutputBytes));
                writer.WritePropertyName(nameof(native.StoredValue));
                if (native.StoredValue is { } stored)
                {
                    writer.WriteStartObject();
                    writer.WriteString(nameof(stored.PathTemplate), stored.PathTemplate);
                    writer.WriteString(nameof(stored.Parser), stored.Parser.ToString());
                    writer.WriteString(nameof(stored.MissingValue), stored.MissingValue);
                    writer.WriteNumber(nameof(stored.MaximumBytes), stored.MaximumBytes);
                    writer.WriteEndObject();
                }
                else writer.WriteNullValue();
                writer.WriteString(nameof(native.DesiredValue), native.DesiredValue);
                Strings(writer, nameof(native.ManualRestoreArgumentsTemplate), native.ManualRestoreArgumentsTemplate);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(change), change, "The config recipe kind is unsupported.");
        }
        writer.WriteEndObject();
    }

    private static void Strings(Utf8JsonWriter writer, string name, IReadOnlyList<string>? values)
    {
        if (values is null) { writer.WriteNull(name); return; }
        writer.WriteStartArray(name);
        foreach (var value in values) writer.WriteStringValue(value);
        writer.WriteEndArray();
    }

    private static void Environment(Utf8JsonWriter writer, string name, IReadOnlyDictionary<string, string?>? values)
    {
        if (values is null) { writer.WriteNull(name); return; }
        writer.WriteStartObject(name);
        foreach (var (key, value) in values.OrderBy(pair => pair.Key, StringComparer.Ordinal)) writer.WriteString(key, value);
        writer.WriteEndObject();
    }

    private static void Canonical(Utf8JsonWriter writer, JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in value.EnumerateObject().OrderBy(property => property.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    Canonical(writer, property.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in value.EnumerateArray()) Canonical(writer, item);
                writer.WriteEndArray();
                break;
            default: value.WriteTo(writer); break;
        }
    }
}
