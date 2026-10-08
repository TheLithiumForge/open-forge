using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

internal static class InstallFrontmatterResolver
{
    internal static InstallFrontmatterResolution Resolve(
        InstallSetupInput? setup,
        WorkspaceSettingsRead settings,
        bool installed,
        bool canPrompt)
    {
        var configure = setup?.Configure == true;
        if (installed && !configure && setup?.Frontmatter is not null)
            return Invalid("An explicit frontmatter form on an installed workspace requires --configure.");
        if (configure && !canPrompt && setup is { Preset: null, Frontmatter: null })
            return Invalid("Noninteractive --configure requires an explicit --preset or --frontmatter.");

        var effective = settings.Document.Frontmatter;
        if (setup?.Frontmatter is { } explicitForm)
            return Selected(explicitForm);
        if (installed && !configure)
            return Selected(effective);
        if (!installed && settings.Document.DeclaredFrontmatter is { } declared)
            return Selected(declared);
        if (canPrompt)
        {
            var initial = installed ? effective : FrontmatterForm.Root;
            return new(null, new(WorkspaceSettingsDefinitions.ReadFrontmatterName(initial)));
        }
        return Selected(installed ? effective : FrontmatterForm.Root);

        InstallFrontmatterResolution Selected(FrontmatterForm form)
        {
            var persist = !installed || (configure && settings.Document.DeclaredFrontmatter != form);
            if (installed && configure && setup?.Frontmatter is null)
                persist = false;
            return new(new(settings, form, installed ? effective : null, persist, installed && configure));
        }
    }

    private static InstallFrontmatterResolution Invalid(string cause)
        => new(null, Boundary: new(InstallFindingCode.InvalidInput, cause));
}
