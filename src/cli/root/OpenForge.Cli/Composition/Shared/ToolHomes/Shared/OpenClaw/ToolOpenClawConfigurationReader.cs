using System.Collections.Immutable;

namespace OpenForge.Cli.Composition.Shared.ToolHomes.Shared.OpenClaw;

// Derived from CBM v0.12.1 src/cli/config_json_like.c, jl_scan_object and cbm_json_like_get_string_at_path,
// and src/cli/cli.c, cbm_openclaw_workspace_path. Copyright (c) 2025 DeusData.
// MIT licence, full Codebase Memory notice in root LICENSE.
// Contract: .agents/memory/crystallized/documents/cli/technical-designs/tool-setup.md#native-config-reader (I1).
internal static class ToolOpenClawConfigurationReader
{
    internal const string AgentsProperty = "agents";
    internal const string DefaultsProperty = "defaults";
    internal const string WorkspaceProperty = "workspace";
    internal const string IncludeProperty = "$include";
    private static readonly ImmutableArray<string> Parents = [AgentsProperty, DefaultsProperty];

    internal static string? ReadWorkspace(ReadOnlySpan<byte> source)
    {
        if (source.IsEmpty) return null;
        var syntax = new ToolOpenClawJsonSyntax(source);
        var root = syntax.ValidateDocument();
        var current = root;
        foreach (var name in Parents)
        {
            if (Find(source, current, name) is not { } member) return Missing(source, root);
            current = member.Start.Value;
            if (source[current] != '{') throw Invalid();
        }
        if (Find(source, current, WorkspaceProperty) is not { } workspace) return Missing(source, root);
        return ToolOpenClawJsonStrings.DecodeValue(source[workspace]);
    }

    private static string? Missing(ReadOnlySpan<byte> source, int root)
    {
        if (Find(source, root, IncludeProperty) is not null) throw Invalid();
        return null;
    }

    private static Range? Find(ReadOnlySpan<byte> source, int objectStart, string name)
    {
        var syntax = new ToolOpenClawJsonSyntax(source) { Position = objectStart + 1 };
        Range? found = null;
        syntax.SkipTrivia();
        if (syntax.Take((byte)'}')) return null;
        while (true)
        {
            var key = syntax.ReadKey();
            syntax.SkipTrivia();
            syntax.Require((byte)':');
            syntax.SkipTrivia();
            var value = syntax.ReadValue(1);
            if (ToolOpenClawJsonStrings.KeyEquals(source[key], name))
            {
                if (found is not null) throw Invalid();
                found = value;
            }
            syntax.SkipTrivia();
            if (syntax.Take((byte)'}')) return found;
            syntax.Require((byte)',');
            syntax.SkipTrivia();
            if (syntax.Take((byte)'}')) return found;
        }
    }

    internal static IOException Invalid() => new("The native OpenClaw configuration is invalid or ambiguous.");
}
