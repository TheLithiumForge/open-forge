using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Wording;
using TheLithium.Imprint;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

public sealed class RouteInitLayoutWordingTests
{
    private const string MainRoot = "handbook/context";
    private const string Target = "outside";
    private const string Loader = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;

    [Fact(DisplayName = "Route Init custom-root guidance uses projected content and loader locations")]
    [Trait("Feature", "route-init"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void CustomRootGuidance()
        => string.Join(Environment.NewLine,
            RouteInitWording.InvalidTarget(Target, MainRoot),
            RouteInitWording.InvalidTargetLabel(MainRoot),
            RouteInitWording.LoaderUnsafe(Loader))
            .AssertSnapshot();
}
