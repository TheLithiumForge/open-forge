using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.IntegrationTests.Commands.Shared.LibraryRecovery;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

public sealed class DoctorLibraryOuterJsonContractIntegrationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("record-create")]
    [InlineData("link-delete")]
    public static async Task OuterJsonPreservesExactSchemaWithoutRichInternalPayloads(string kind)
    {
        using var workspace = new LibraryResidualWorkspace(dangling: true);
        await workspace.PrepareAsync(kind);

        var run = await CliHostCapture.RunAsync(["doctor", "--json"], workspace.Files.Path);

        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected Doctor result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        var domain = Assert.Single(
            document.RootElement.GetProperty("result").GetProperty("domains").EnumerateArray(),
            value => value.GetProperty("domain").GetString() == "workspace-entry");
        PropertyOrder(
            domain,
            "domain",
            "boundary",
            "coverage",
            "lifecycle",
            "sourceAvailability",
            "limitations",
            "counts",
            "findings",
            "actions");
        var finding = Assert.Single(
            domain.GetProperty("findings").EnumerateArray(),
            value => value.GetProperty("kind").GetString() == "library.recovery-safe-exact");
        var subject = finding.GetProperty("subject");
        PropertyOrder(subject, "kind", "path", "id", "location");
        Assert.Equal("library", subject.GetProperty("kind").GetString());
        var proposal = finding.GetProperty("proposal");
        PropertyOrder(
            proposal,
            "kind",
            "subject",
            "expected",
            "intended",
            "boundary",
            "verification",
            "recovery");
        var proposalSubject = proposal.GetProperty("subject");
        PropertyOrder(proposalSubject, "kind", "path", "id", "location");
        Assert.Equal("library", proposalSubject.GetProperty("kind").GetString());
        var entry = workspace.Evidence.Entry.Input.Context.Entry;
        Assert.Equal(StateIdentity(entry.Intended), proposal.GetProperty("expected").GetString());
        Assert.Equal(StateIdentity(entry.Prior), proposal.GetProperty("intended").GetString());
        Assert.Equal("library-no-follow-exact", proposal.GetProperty("verification").GetString());
        Assert.Equal("repair-receipt-required", proposal.GetProperty("recovery").GetString());
    }

    private static string StateIdentity(RecoveryEntryState state)
        => state.Kind switch
        {
            RecoveryEntryStateKind.Missing =>
                "{\"kind\":\"missing\",\"length\":null,\"sha256\":null,\"linkKind\":null,\"rawRelativeTarget\":null}",
            RecoveryEntryStateKind.OrdinaryFile when state.OrdinaryFile is { } ordinary =>
                $"{{\"kind\":\"ordinary-file\",\"length\":{ordinary.Length.ToString(
                    CultureInfo.InvariantCulture)},\"sha256\":\"{JsonEncodedText.Encode(ordinary.Sha256)}\",\"linkKind\":null,\"rawRelativeTarget\":null}}",
            RecoveryEntryStateKind.RelativeFileLink when state.RelativeFileLink is { } link =>
                $"{{\"kind\":\"relative-file-link\",\"length\":null,\"sha256\":null,\"linkKind\":\"relative-file-symbolic-link\",\"rawRelativeTarget\":\"{JsonEncodedText.Encode(
                    link.RawRelativeTarget)}\"}}",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state.Kind,
                "The recovery state kind is not defined."),
        };

    private static void PropertyOrder(JsonElement element, params string[] expected)
        => Assert.Equal(expected, element.EnumerateObject().Select(property => property.Name));
}
