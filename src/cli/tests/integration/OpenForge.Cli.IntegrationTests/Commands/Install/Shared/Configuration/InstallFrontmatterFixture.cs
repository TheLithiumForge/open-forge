using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Operation;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Configuration;

internal static class InstallFrontmatterFixture
{
    internal static InstallOperation Operation(InstallOperationWorkspace workspace,
        CliPlanConfirmation<InstallResult, InstallConfirmationFacts>? confirmation = null)
    {
        var paths = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(paths);
        return new(confirmation ?? InstallInteractionTestSupport.Unavailable(), new InstallPlanBuilder(paths),
            new MutationPreflight(validator), InstallApplicationOperationFactory.Create(paths, validator, workspace.LockStoreRoot));
    }

    internal static async Task<InstallRequest> RequestAsync(InstallOperationWorkspace workspace,
        FrontmatterForm form, bool installed = false, bool dryRun = false)
    {
        var settings = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        return workspace.Request(mode: dryRun ? InstallMode.DryRun : InstallMode.Apply) with
        {
            Frontmatter = new(settings, form, installed ? settings.Document.Frontmatter : null,
                persist: !installed || settings.Document.DeclaredFrontmatter != form, convertOwnedFiles: installed),
        };
    }

    internal static async Task SeedScopedAsync(InstallOperationWorkspace workspace)
    {
        var result = await Operation(workspace).ExecuteAsync(await RequestAsync(workspace, FrontmatterForm.Scoped), TestContext.Current.CancellationToken);
        Assert.Equal(Core.Shell.Definitions.CliSemanticStatus.Complete, result.Status);
    }
}
