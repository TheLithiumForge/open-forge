using System.CommandLine;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Binding;

internal static class InstallBindingInputReader
{
    internal static InstallBindingInput Read(
        ParseResult result,
        InstallSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(symbols);

        var dryRun = result.GetValue(symbols.DryRun);
        var configure = result.GetValue(symbols.Configure);
        var presetText = result.GetValue(symbols.Preset);
        var preset = InstallConfigurationChoices.ReadPreset(presetText);
        var overrides = new Dictionary<string, InstallRouteAction>(StringComparer.Ordinal);
        string? error = null;
        InstallInvalidRoute? invalidRoute = null;
        InstallInvalidOption? invalidOption = null;
        if (presetText is not null && preset is null)
        {
            error = "Preset must be minimal, essentials, full-core, or custom.";
            invalidOption = new(InstallOptionKind.Preset, presetText);
        }
        foreach (var text in result.GetValue(symbols.Route) ?? [])
        {
            var parts = text.Split('=');
            if (parts.Length != 2 || !InstallConfigurationChoices.RouteIds.Contains(parts[0], StringComparer.Ordinal)
                || InstallConfigurationChoices.ReadAction(parts[1]) is not { } action)
            {
                error = "Each route must name a built-in ID and add, remove, or git-ignore.";
                invalidRoute ??= new InstallInvalidRoute(text, InstallConfigurationChoices.RouteIds);
                continue;
            }
            if (overrides.TryGetValue(parts[0], out var existing) && existing != action)
                error = "Repeated route values must agree.";
            overrides[parts[0]] = action;
        }
        if (overrides.Count > 0 && preset is not (InstallPreset.Custom or InstallPreset.Minimal))
            error = "Route overrides require --preset custom or --preset minimal.";
        var frontmatterFacts = CliOptionResultFactsReader.Read(result, symbols.Frontmatter);
        FrontmatterForm? frontmatter = null;
        if (frontmatterFacts.IsExplicit && frontmatterFacts.ValueCount != frontmatterFacts.IdentifierCount)
            error = "Each --frontmatter requires root or scoped.";
        foreach (var value in CliOptionResultFactsReader.ReadValues(result, symbols.Frontmatter))
        {
            if (!WorkspaceSettingsDefinitions.TryReadFrontmatter(value, out var form))
            {
                error = "Frontmatter must be root or scoped.";
                invalidOption ??= new(InstallOptionKind.Frontmatter, value);
                continue;
            }
            if (frontmatter is { } previous && previous != form)
                error = "Repeated frontmatter values must agree.";
            frontmatter = form;
        }
        var mainRootFacts = CliOptionResultFactsReader.Read(result, symbols.MainRoot);
        string? mainRoot = null;
        if (mainRootFacts.IsExplicit && mainRootFacts.ValueCount != mainRootFacts.IdentifierCount)
            error = $"Each {InstallDefinitions.MainRoot.Name} requires a workspace-relative folder.";
        foreach (var value in CliOptionResultFactsReader.ReadValues(result, symbols.MainRoot))
        {
            if (!WorkspaceMainRootPath.IsCanonical(value))
                error = "The context folder must be a canonical workspace-relative content directory.";
            if (mainRoot is not null && mainRoot != value)
                error = "Repeated context folder values must agree.";
            mainRoot = value;
        }
        InstallSetupInput? setup = null;
        if (configure || presetText is not null || overrides.Count > 0 || frontmatterFacts.IsExplicit)
            setup = new InstallSetupInput(configure, preset, InstallConfigurationChoices.RouteIds
                .Where(overrides.ContainsKey).Select(id => new InstallRouteSelection(id, overrides[id])).ToImmutableArray())
            { Frontmatter = frontmatter };
        return new InstallBindingInput(
            Force: result.GetValue(symbols.Force),
            Automatic: result.GetValue(symbols.Automatic),
            Mode: dryRun ? InstallMode.DryRun : InstallMode.Apply, Setup: setup, SetupError: error)
        { InvalidRoute = invalidRoute, InvalidOption = invalidOption, RequestedMainRoot = mainRoot };
    }
}
