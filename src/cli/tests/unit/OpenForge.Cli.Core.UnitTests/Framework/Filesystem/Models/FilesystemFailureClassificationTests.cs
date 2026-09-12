using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Filesystem.Models;

public sealed class FilesystemFailureClassificationTests
{
    [Fact(DisplayName = "Unauthorized access exceptions classify as access denied"), Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void UnauthorizedAccessClassifiesAsAccessDenied()
    {
        Assert.Equal(
            FilesystemFailureKind.AccessDenied,
            FilesystemFailure.ClassifyException(new UnauthorizedAccessException()));
    }

    [Fact(DisplayName = "Unsupported operation and platform exceptions classify as unsupported"), Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void OperationAndPlatformExceptionsClassifyAsUnsupported()
    {
        Assert.Equal(
            FilesystemFailureKind.Unsupported,
            FilesystemFailure.ClassifyException(new NotSupportedException()));
        Assert.Equal(
            FilesystemFailureKind.Unsupported,
            FilesystemFailure.ClassifyException(new PlatformNotSupportedException()));
    }

    [Fact(DisplayName = "Argument and path-length exceptions classify as invalid paths"), Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void ArgumentAndPathLengthExceptionsClassifyAsInvalidPaths()
    {
        Assert.Equal(
            FilesystemFailureKind.InvalidPath,
            FilesystemFailure.ClassifyException(new ArgumentException()));
        Assert.Equal(
            FilesystemFailureKind.InvalidPath,
            FilesystemFailure.ClassifyException(new ArgumentNullException()));
        Assert.Equal(
            FilesystemFailureKind.InvalidPath,
            FilesystemFailure.ClassifyException(new PathTooLongException()));
    }

    [Fact(DisplayName = "I/O and file-not-found exceptions classify as input-output failures"), Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void InputOutputAndMissingFileExceptionsClassifyAsInputOutputFailures()
    {
        Assert.Equal(
            FilesystemFailureKind.InputOutput,
            FilesystemFailure.ClassifyException(new IOException()));
        Assert.Equal(
            FilesystemFailureKind.InputOutput,
            FilesystemFailure.ClassifyException(new FileNotFoundException()));
    }

    [Fact(DisplayName = "Invalid operation exceptions retain the unsupported classifier contract"), Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void InvalidOperationRetainsUnsupportedClassifierDetails()
    {
        var failure = Assert.Throws<ArgumentOutOfRangeException>(() =>
            FilesystemFailure.ClassifyException(new InvalidOperationException()));

        Assert.Equal("exception", failure.ParamName);
        Assert.Equal(typeof(InvalidOperationException), failure.ActualValue);
        var expected = new ArgumentOutOfRangeException(
            paramName: "exception",
            actualValue: typeof(InvalidOperationException),
            message: "The filesystem exception is not defined.");
        Assert.Equal(expected.Message, failure.Message);
    }

    [Fact(DisplayName = "Invalid data exceptions retain the unsupported classifier contract"), Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void InvalidDataRetainsUnsupportedClassifierDetails()
    {
        Exception exception = new InvalidDataException();
        var failure = Assert.Throws<ArgumentOutOfRangeException>(() =>
            FilesystemFailure.ClassifyException(exception));

        Assert.Equal("exception", failure.ParamName);
        Assert.Equal(typeof(InvalidDataException), failure.ActualValue);
        var expected = new ArgumentOutOfRangeException(
            paramName: "exception",
            actualValue: typeof(InvalidDataException),
            message: "The filesystem exception is not defined.");
        Assert.Equal(expected.Message, failure.Message);
    }
}
