using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using TheLithium.Imprint;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

[Trait("Feature", "heading-entries-migration"), Trait("Evidence", "Unit")]
public sealed class RouteInitScaffoldSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Init scaffold matches its reviewed Markdown bytes")]
    public void Scaffold()
    {
        var metadata = new RouteInitMetadata("Review rules", default, null, default, ["Contextual"], default);
        var scaffold = new RouteInitScaffoldComposer().Compose("rules", "Rules", metadata);
        Encoding.UTF8.GetString(scaffold.Bytes.AsSpan()).AssertSnapshot();
    }
}
