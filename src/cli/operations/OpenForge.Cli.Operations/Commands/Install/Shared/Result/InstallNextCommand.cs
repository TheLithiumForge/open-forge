using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Result;

internal static class InstallNextCommand
{
    internal static string Create(InstallBindingInput input, CliWorkspace? workspace, bool confirmationRequired = false, bool diagnostics = false)
    {
        var arguments = new List<string> { "install" };
        if (input.RequestedMainRoot is { } mainRoot)
            CliRetryCommand.Add(arguments, InstallDefinitions.MainRoot.Name, mainRoot);
        if (workspace is { SelectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace })
            CliRetryCommand.Add(arguments, "--workspace", workspace.LexicalRoot);
        var setup = input.Setup;
        var configuration = input.Configuration;
        if (configuration is null && input.SetupRecap is { } recap)
            configuration = new(recap.Configure, recap.Preset, recap.Routes);
        if (configuration is not null)
        {
            setup = new(configuration.Configure, configuration.Preset,
                configuration.Preset switch
                {
                    Models.Configuration.InstallPreset.Custom => configuration.Routes,
                    Models.Configuration.InstallPreset.Minimal => configuration.Routes
                        .Where(row => row.Action != Models.Configuration.InstallRouteAction.Remove).ToImmutableArray(),
                    _ => [],
                })
            { Frontmatter = input.ResolvedFrontmatter ?? setup?.Frontmatter };
        }
        if (setup is not null)
        {
            if (setup.Configure) arguments.Add(InstallDefinitions.ConfigureOption);
            if (setup.Preset is { } preset) CliRetryCommand.Add(arguments, InstallDefinitions.PresetOption, InstallConfigurationChoices.Name(preset));
            foreach (var row in setup.Overrides) CliRetryCommand.Add(arguments, InstallDefinitions.RouteOption, $"{row.Id}={InstallConfigurationChoices.Name(row.Action)}");
            if ((input.ResolvedFrontmatter ?? setup.Frontmatter) is { } form)
                CliRetryCommand.Add(arguments, "--frontmatter", WorkspaceSettingsDefinitions.ReadFrontmatterName(form));
        }
        if (input.Force) arguments.Add("--force");
        if (confirmationRequired || input.Automatic) arguments.Add("--automatic");
        if (input.IsDryRun) arguments.Add("--dry-run");
        CliRetryCommand.AddPresentation(arguments, input.RetryOptions, diagnostics);
        return CliRetryCommand.Build(arguments);
    }
}
