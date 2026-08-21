using System.Text;
using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

public sealed class RouteListReadCauseRefinementIntegrationTests
{
    [Fact(DisplayName = "Route-list invalid UTF-8 produces an incomplete canonical selection with a bounded direct cause"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task InvalidUtf8HasSpecificBoundedCause()
    {
        using var workspace = RouteListTestWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.WriteRoute(".agents/root/_root.md", "Root", "Root");
        byte[] invalidBytes = [0xFF, 0xFE];
        workspace.WriteBytes(".agents/root/invalid-utf8.md", invalidBytes);
        var before = workspace.SnapshotHashes();
        AssertInvalidUtf8Capability(invalidBytes);

        var result = await RouteListOperation.RunAsync(
            workspace.Request("root", RouteListDepth.All),
            TestContext.Current.CancellationToken);

        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal("root", result.Result.Selection.SourceId);
        Assert.Equal(".agents/root/_root.md", result.Result.Selection.SourcePath);
        var invalidUtf8 = Assert.Single(result.Result.Findings, finding => finding.Path == ".agents/root/invalid-utf8.md");
        Assert.Equal(RouteListFindingCodes.SourceReadFailed, invalidUtf8.Code);
        Assert.Contains("invalid", invalidUtf8.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UTF-8", invalidUtf8.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "could not be read as strict UTF-8",
            invalidUtf8.Message,
            StringComparison.OrdinalIgnoreCase);
        Assert.InRange(invalidUtf8.Message.Length, 1, 512);
        AssertNoUnsafeExceptionFraming(invalidUtf8.Message);
    }

    [Fact(DisplayName = "Route-list locked source produces an incomplete canonical selection with a non-generic bounded direct cause"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task LockedSourceHasNonGenericBoundedCause()
    {
        using var workspace = RouteListTestWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.WriteRoute(".agents/root/_root.md", "Root", "Root");
        workspace.WriteRoute(".agents/root/io-failure.md", "Locked source", "Locked");
        var before = workspace.SnapshotHashes();
        var ioFailurePath = System.IO.Path.Combine(workspace.Path, ".agents", "root", "io-failure.md");

        RouteListResult result;
        using (var exclusive = OpenExclusiveOrSkip(ioFailurePath))
        {
            VerifyReadDeniedOrSkip(ioFailurePath);
            result = await RouteListOperation.RunAsync(
                workspace.Request("root", RouteListDepth.All),
                TestContext.Current.CancellationToken);
        }

        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal("root", result.Result.Selection.SourceId);
        Assert.Equal(".agents/root/_root.md", result.Result.Selection.SourcePath);
        var ioFailure = Assert.Single(result.Result.Findings, finding => finding.Path == ".agents/root/io-failure.md");
        Assert.Equal(RouteListFindingCodes.SourceReadFailed, ioFailure.Code);
        Assert.DoesNotContain("strict UTF-8", ioFailure.Message, StringComparison.OrdinalIgnoreCase);
        AssertStableReadFailureCategory(ioFailure.Message);
        Assert.InRange(ioFailure.Message.Length, 1, 512);
        AssertNoUnsafeExceptionFraming(ioFailure.Message);
    }

    private static FileStream OpenExclusiveOrSkip(string path)
    {
        try
        {
            return new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (PlatformNotSupportedException exception)
        {
            Assert.Skip($"An exclusive file handle cannot be established on this OS boundary: {exception.GetType().Name}: {exception.Message}");
            throw;
        }
    }

    private static void AssertInvalidUtf8Capability(byte[] contents)
    {
        var strictUtf8 = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true);
        Assert.Throws<DecoderFallbackException>(() => strictUtf8.GetString(contents));
    }

    private static void VerifyReadDeniedOrSkip(string path)
    {
        try
        {
            _ = File.ReadAllBytes(path);
            Assert.Skip("This OS boundary does not deny a second read while the test owns an exclusive file handle.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
        }
        catch (PlatformNotSupportedException exception)
        {
            Assert.Skip($"The OS cannot establish the locked-read boundary: {exception.Message}");
        }
    }

    private static void AssertStableReadFailureCategory(string message)
    {
        var normalized = new string(message
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
        Assert.True(
            normalized.Contains("iofailure", StringComparison.Ordinal)
            || normalized.Contains("inputoutput", StringComparison.Ordinal)
            || normalized.Contains("access", StringComparison.Ordinal)
            || normalized.Contains("lock", StringComparison.Ordinal));
    }

    private static void AssertNoUnsafeExceptionFraming(string message)
    {
        Assert.DoesNotContain("System.", message, StringComparison.Ordinal);
        Assert.DoesNotContain("\n   at ", message, StringComparison.Ordinal);
        Assert.DoesNotContain("\r\n   at ", message, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception:", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", message, StringComparison.OrdinalIgnoreCase);
    }
}
