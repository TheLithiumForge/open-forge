using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.MainRoot;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

[Trait("Feature", "install-main-root"), Trait("Evidence", "EndToEnd")]
public sealed class PublishedMainRootInstallProcessTests
{
    private const string MainRoot = "handbook/team/context";
    private const string OtherRoot = "handbook/other";

    [Theory(DisplayName = "The real Install option previews installs observes and converges in both metadata forms")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter), InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    public async Task InstallAndObserve(string form)
    {
        using var workspace = new PublishedMainRootWorkspace(MainRoot);
        var before = workspace.Bytes();
        using var preview = await workspace.Run(PublishedTokens.Install.Command, PublishedTokens.Install.MainRoot, MainRoot,
            PublishedTokens.Install.Frontmatter, form, PublishedTokens.Install.DryRun);
        workspace.AssertBytes(before);
        Assert.False(Directory.Exists(workspace.Combine(TemporaryWorkspace.RuntimeDirectoryName)));
        await workspace.Install(form);
        Assert.True(File.Exists(workspace.Combine(workspace.Loader)));
        Assert.True(File.Exists(workspace.Combine(PublishedTokens.Layout.CliSkillDirectory + "/" + PublishedTokens.Layout.NativeSkillFileName)));
        Assert.False(Directory.Exists(workspace.Combine(MainRoot + "/" + PublishedTokens.Layout.SkillsDirectory)));
        Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
        using var ownership = JsonDocument.Parse(workspace.Read(PublishedTokens.Ownership.RelativePath));
        Assert.Contains(ownership.RootElement.GetProperty(PublishedTokens.Ownership.Framework).GetProperty(PublishedTokens.Ownership.Paths).EnumerateArray(),
            path => path.GetString() == workspace.Loader);
        var installed = workspace.Bytes();
        using var status = await workspace.Run(PublishedTokens.Status.Command);
        Assert.Equal(MainRoot, status.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.LayoutData.Layout)
            .GetProperty(PublishedTokens.LayoutData.MainRoot).GetString());
        using var context = await workspace.Run(PublishedTokens.Context.Command);
        Assert.Contains(context.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.ContextData.Sources).EnumerateArray(),
            source => source.GetProperty(PublishedTokens.Json.Path).GetString() == workspace.Loader);
        using var index = await workspace.Run(PublishedTokens.Index.Command);
        using var repeated = await workspace.Run(PublishedTokens.Install.Command, PublishedTokens.Install.MainRoot, MainRoot, PublishedTokens.Install.Automatic);
        workspace.AssertBytes(installed);
        if (form == PublishedTokens.Settings.ScopedFrontmatter)
        {
            var refused = await workspace.RunRaw([PublishedTokens.Install.Command, PublishedTokens.Install.MainRoot, OtherRoot,
                PublishedTokens.Install.Automatic, PublishedTokens.Install.Configure,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);
            Assert.Equal(PublishedTokens.Statuses.InvalidExitCode, refused.ExitCode);
            workspace.AssertBytes(installed);
        }
    }

}
