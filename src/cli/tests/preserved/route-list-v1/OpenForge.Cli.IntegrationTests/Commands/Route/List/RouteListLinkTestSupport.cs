namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

internal static class RouteListLinkTestSupport
{
    internal static void CreateDirectorySymbolicLinkOrSkip(string linkPath, string targetPath)
    {
        ValidateLinkLocation(linkPath);
        try
        {
            Directory.CreateSymbolicLink(linkPath, targetPath);
        }
        catch (Exception exception) when (IsUnavailable(exception))
        {
            Assert.Skip($"Directory symbolic-link creation is unavailable: {exception.GetType().Name}: {exception.Message}");
        }
    }

    internal static void CreateFileSymbolicLinkOrSkip(string linkPath, string targetPath)
    {
        ValidateLinkLocation(linkPath);
        try
        {
            File.CreateSymbolicLink(linkPath, targetPath);
        }
        catch (Exception exception) when (IsUnavailable(exception))
        {
            Assert.Skip($"File symbolic-link creation is unavailable: {exception.GetType().Name}: {exception.Message}");
        }
    }

    private static bool IsUnavailable(Exception exception)
    {
        return exception is UnauthorizedAccessException or PlatformNotSupportedException;
    }

    private static void ValidateLinkLocation(string linkPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(linkPath);
        var parent = System.IO.Path.GetDirectoryName(linkPath);
        if (parent is null || !Directory.Exists(parent))
        {
            throw new InvalidOperationException("The owned symbolic-link parent directory must exist before link creation.");
        }

        if (Directory.Exists(linkPath) || File.Exists(linkPath))
        {
            throw new InvalidOperationException("The owned symbolic-link path must not exist before link creation.");
        }
    }
}
