using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Run.Shared.Resolution;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Run.Shared.Resolution;

[Trait("Feature", "tool-run"), Trait("Evidence", "Unit")]
public sealed class ToolRunPinResolverTests
{
    [Fact(DisplayName = "Run resolves the exact declared reviewed local pin and keeps CWD and child arguments")]
    public void ResolvesVerifiedPin()
    {
        var seed = new ToolRunPinSeed();
        var request = seed.Request with { CurrentDirectory = Path.Combine(seed.Workspace.PhysicalRoot, "nested"), Arguments = ["--help", "", "space value"] };
        var result = new ToolRunPinResolver(seed.Catalogue, ToolNativeTarget.Current).Resolve(request, seed.Workspace, seed.Evidence);
        var ready = Assert.IsType<ToolRunReady>(result);
        Assert.Same(seed.Launch, ready.Dispatch.Launch);
        Assert.Equal(request.CurrentDirectory, ready.Dispatch.WorkingDirectory);
        Assert.Equal(request.Arguments, ready.Dispatch.Arguments);
    }

    [Theory(DisplayName = "Run rejects mismatched pins, incomplete verification and absent local installations before launch")]
    [InlineData(0, (int)ToolRunFailureCode.PinMismatch)]
    [InlineData(1, (int)ToolRunFailureCode.PinMismatch)]
    [InlineData(2, (int)ToolRunFailureCode.PinMismatch)]
    [InlineData(3, (int)ToolRunFailureCode.PinMismatch)]
    [InlineData(4, (int)ToolRunFailureCode.DeclaredToolMissing)]
    [InlineData(5, (int)ToolRunFailureCode.DeclaredToolMissing)]
    [InlineData(6, (int)ToolRunFailureCode.VerificationUnavailable)]
    [InlineData(7, (int)ToolRunFailureCode.PinMismatch)]
    [InlineData(8, (int)ToolRunFailureCode.PinMismatch)]
    [InlineData(9, (int)ToolRunFailureCode.VerificationUnavailable)]
    [InlineData(10, (int)ToolRunFailureCode.VerificationUnavailable)]
    [InlineData(11, (int)ToolRunFailureCode.PinMismatch)]
    [InlineData(12, (int)ToolRunFailureCode.PinMismatch)]
    [InlineData(13, (int)ToolRunFailureCode.VerificationUnavailable)]
    [InlineData(14, (int)ToolRunFailureCode.VerificationUnavailable)]
    [InlineData(15, (int)ToolRunFailureCode.VerificationUnavailable)]
    [InlineData(16, (int)ToolRunFailureCode.VerificationUnavailable)]
    public void RefusesUnverifiedFacts(int scenario, int expectedCode)
    {
        var seed = new ToolRunPinSeed();
        var expected = (ToolRunFailureCode)expectedCode;
        var evidence = scenario switch
        {
            0 => seed.Evidence with { Setting = seed.Evidence.Setting with { Version = "2.0.0" } },
            1 => seed.Evidence with { Setting = seed.Evidence.Setting with { CodeRoot = "other" } },
            2 => seed.Evidence with { Setting = seed.Evidence.Setting with { Hosts = [] } },
            3 => seed.WithUnit(seed.Unit with { Verified = false }),
            4 => seed.Evidence with { Registry = seed.Evidence.Registry with { Units = [] } },
            5 => seed.WithUnit(seed.Unit with { Workspaces = [] }),
            6 => seed.WithLaunch(null),
            7 => seed.WithLaunch(seed.Launch with { Version = "2.0.0" }),
            8 => seed.WithLaunch(seed.Launch with { Executable = Path.Combine(seed.Workspace.PhysicalRoot, ToolRunPinSeed.Entry) }),
            9 => seed.WithLaunch(seed.Launch with { Files = [] }),
            10 => seed.WithLaunch(seed.Launch with { Files = [seed.Launch.Files[0] with { Path = "../escape" }] }),
            11 => seed.Evidence with { Receipt = seed.Evidence.Receipt with { Bindings = [] } },
            12 => seed.Evidence with { Receipt = seed.Evidence.Receipt with { Files = [] } },
            13 => seed.WithUnit(seed.Unit with { Dependencies = [new string('a', 64)] }),
            14 => seed.WithLaunch(seed.Launch with { McpArguments = [null!] }),
            15 => seed.WithLaunch(seed.Launch with { McpArguments = [[null!]] }),
            16 => seed.WithLaunch(seed.Launch with { McpArguments = [["invalid\0argument"]] }),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var result = new ToolRunPinResolver(seed.Catalogue, ToolNativeTarget.Current).Resolve(seed.Request, seed.Workspace, evidence);
        var failure = Assert.IsType<ToolRunRefused>(result).Failure;
        Assert.Equal(expected, failure.Code);
        Assert.Same(seed.Workspace, failure.Workspace);
        Assert.Equal(expected == ToolRunFailureCode.DeclaredToolMissing, failure.OfferInstall);
    }

    [Fact(DisplayName = "Run never dispatches an unreviewed manifest from a copied lock")]
    public void RefusesUnreviewedDefinition()
    {
        var seed = new ToolRunPinSeed();
        var result = new ToolRunPinResolver(new ToolCatalogueReader(), ToolNativeTarget.Current).Resolve(seed.Request, seed.Workspace, seed.Evidence);
        Assert.Equal(ToolRunFailureCode.PinMismatch, Assert.IsType<ToolRunRefused>(result).Failure.Code);
    }
}
