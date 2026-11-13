using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Registration;

internal sealed class ToolRegistrationFixture : IDisposable
{
    internal TemporaryWorkspace Temporary { get; } = TemporaryWorkspace.Create("tool-registration");
    internal ToolRegistration Definition { get; } = JsonSerializer.Deserialize(ToolRegistrationFixtureSource.Read(ToolRegistrationFixtureSource.Definition),
        ToolJsonContext.Default.ToolRegistration) ?? throw new InvalidDataException();

    internal ToolEntryRewriteRequest Request => new()
    {
        WorkspaceRoot = Temporary.Path,
        Rewrites = Definition.Rewrites,
        DeclinedComponents = [],
        Values = new Dictionary<string, string>(StringComparer.Ordinal) { [ToolVocabulary.OpenForgeVariable] = ToolEntryDeclarations.PathCommand },
    };

    internal ToolRoleGuidanceRequest RoleRequest(string root = WorkspaceLayout.DefaultMainRoot)
    {
        var workspace = new CliWorkspace(Temporary.Path, Temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        return new(Temporary.Path, WorkspaceLayout.Create(workspace, root),
            [new(ToolFixtureSource.Id, Definition.RoleGuidance[0], WorkspaceLayout.NativeSkillsPath + "/fixture")]);
    }

    internal void Seed()
    {
        foreach (var rewrite in Definition.Rewrites)
            Temporary.WriteBytes(rewrite.File, ToolRegistrationFixtureSource.Read(rewrite switch
            {
                ToolJsonCommandRewrite => ToolRegistrationFixtureSource.Mcp,
                ToolTomlCommandRewrite => ToolRegistrationFixtureSource.Toml,
                ToolHookCommandRewrite => ToolRegistrationFixtureSource.Hooks,
                ToolMarkedLineRewrite => ToolRegistrationFixtureSource.Agent,
                _ => throw new ArgumentOutOfRangeException(nameof(rewrite)),
            }));
    }

    internal async ValueTask ApplyAsync(ToolEntryRewriteRequest request, ToolEntryPlan plan, CancellationToken token)
    {
        if (plan.Checks.Any(check => check.State != ToolEntryState.Original)) throw new InvalidDataException();
        foreach (var edit in plan.Edits)
        {
            var checks = await new ToolEntryVerifier().RecheckAsync(request, edit, token);
            if (checks.Length == 0 || checks.Any(check => check.State != ToolEntryState.Original)) throw new IOException();
            Temporary.ReplaceBytes(edit.File, edit.After);
        }
    }

    public void Dispose() => Temporary.Dispose();
}
