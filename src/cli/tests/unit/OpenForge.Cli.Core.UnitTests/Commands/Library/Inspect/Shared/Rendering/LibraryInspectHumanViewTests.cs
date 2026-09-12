using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Inspect.Shared.Rendering;

public sealed class LibraryInspectHumanViewTests
{
    [Theory(DisplayName = "Library Inspect groups only actual registration and eligibility facts and keeps unexamined paths"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void MembershipComesFromFacts(int value)
    {
        var seed = LibraryInspectResultFixture.Create();
        var record = seed.Result.Record.RegisteredPaths[0] with { SourcePath = "uncompared.md", DestinationPath = "docs/uncompared.md", SourceId = null };
        var eligible = seed.Result.Source.EligiblePaths[0] with { SourcePath = "new.md", DestinationPath = "docs/new.md", SourceId = null };
        var result = seed with
        {
            Result = seed.Result with
            {
                Record = seed.Result.Record with { RegisteredPaths = [.. seed.Result.Record.RegisteredPaths, record] },
                Source = seed.Result.Source with { EligiblePaths = [.. seed.Result.Source.EligiblePaths, eligible] },
            }
        };
        var before = LibraryInspectPresentation.RenderJson(new(result, new(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));
        var text = LibraryInspectPresentation.RenderHuman(new(result, new(CliOutputFormat.Human, (CliView)value, CliVerbosity.Normal)));
        const string mapping = ".agents/directives/review.md -> .agents/directives/review.md; ID directives/review";
        Assert.Equal(1, text.Split(mapping, StringSplitOptions.None).Length - 1);
        Assert.Contains("missing; registered; eligible", text, StringComparison.Ordinal);
        Assert.Contains("uncompared.md -> docs/uncompared.md; ID unavailable", text, StringComparison.Ordinal);
        Assert.Contains("registered; not compared", text, StringComparison.Ordinal);
        Assert.Contains("new.md -> docs/new.md; ID unavailable", text, StringComparison.Ordinal);
        Assert.Contains("eligible; not compared", text, StringComparison.Ordinal);
        Assert.Equal(value == (int)CliView.Expanded, text.Contains("Expected target:", StringComparison.Ordinal));
        Assert.Equal(before, LibraryInspectPresentation.RenderJson(new(result, new(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal))));
        var withoutMembership = seed with
        {
            Result = seed.Result with
            {
                Record = seed.Result.Record with { RegisteredPaths = [] },
                Source = seed.Result.Source with { EligiblePaths = [] },
                Projection = seed.Result.Projection with { Comparisons = [seed.Result.Projection.Comparisons[0] with { Registered = null }] },
            }
        };
        var unknown = LibraryInspectPresentation.RenderHuman(new(withoutMembership, new(CliOutputFormat.Human, (CliView)value, CliVerbosity.Normal)));
        Assert.Contains("missing; not registered; not in the observed eligible inventory", unknown, StringComparison.Ordinal);
    }
}
