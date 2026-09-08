using OpenForge.Cli.Core.Commands.Extension.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Application;

internal sealed class ExtensionCreateDestinationWriter
{
    private readonly PhysicalPathResolver _physicalPathResolver = new();

    internal async ValueTask<ExtensionCreateEffectApplication> ApplyAsync(
        ExtensionCreatePlan plan,
        ExtensionCreateEffect effect,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(effect);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (effect.Kind)
            {
                case ExtensionCreateEffectKind.ManifestFile:
                    await CreateManifestAsync(plan, effect, cancellationToken).ConfigureAwait(false);
                    break;
                case ExtensionCreateEffectKind.PayloadAgentsDirectory:
                    CreatePayload(plan, effect);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "The create effect kind is not defined.");
            }

            return new ExtensionCreateEffectApplication(applied: true, finding: null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failed(
                ExtensionCreateFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                effect.Path,
                "Extension Create was cancelled while applying an effect.");
        }
        catch (Exception exception) when (IsApplicationFailure(exception))
        {
            return Failed(
                ExtensionCreateFindingCode.ApplicationFailed,
                CliSemanticStatus.Failed,
                effect.Path,
                exception.Message);
        }
    }

    private async ValueTask CreateManifestAsync(
        ExtensionCreatePlan plan,
        ExtensionCreateEffect effect,
        CancellationToken cancellationToken)
    {
        var expectedPath = Path.Combine(plan.Destination, ExtensionCreateDefinitions.ManifestFileName);
        RequireExactEffectPath(effect.Path, expectedPath);
        Directory.CreateDirectory(plan.Destination);
        RequireContainedOrdinaryDirectory(plan, plan.Destination);
        await using (var stream = new FileStream(
            expectedPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            FileOptions.Asynchronous))
        {
            await stream.WriteAsync(plan.ManifestBytes, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        var attributes = File.GetAttributes(expectedPath);
        if ((attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
        {
            throw new IOException("The created manifest is not an ordinary file.");
        }

        var actualBytes = await File.ReadAllBytesAsync(expectedPath, cancellationToken).ConfigureAwait(false);
        if (!actualBytes.AsSpan().SequenceEqual(plan.ManifestBytes.Span))
        {
            throw new IOException("The created manifest bytes do not match the plan.");
        }

        _ = ExtensionManifestReader.Read(actualBytes, expectedPath, []);
    }

    private void CreatePayload(ExtensionCreatePlan plan, ExtensionCreateEffect effect)
    {
        var payload = Path.Combine(plan.Destination, ExtensionPackageLayout.ContentDirectoryName);
        var agents = Path.Combine(payload, ExtensionCreateDefinitions.AgentsDirectoryName);
        RequireExactEffectPath(effect.Path.TrimEnd(Path.DirectorySeparatorChar), agents);
        RequireMissing(plan, payload);
        Directory.CreateDirectory(agents);
        RequireContainedOrdinaryDirectory(plan, payload);
        RequireContainedOrdinaryDirectory(plan, agents);
        if (Directory.EnumerateFileSystemEntries(agents).Any())
        {
            throw new IOException("The created payload .agents directory is not empty.");
        }
    }

    private void RequireMissing(ExtensionCreatePlan plan, string path)
    {
        var resolution = ResolveCandidate(plan, path);
        if (resolution.State != PhysicalPathState.Missing)
        {
            throw new IOException($"The create-only path is no longer absent ({resolution.State}).");
        }
    }

    private void RequireContainedOrdinaryDirectory(ExtensionCreatePlan plan, string path)
    {
        var resolution = ResolveCandidate(plan, path);
        if (resolution.State != PhysicalPathState.Contained)
        {
            throw new IOException($"The created directory is not safely contained ({resolution.State}).");
        }

        var attributes = File.GetAttributes(resolution.GetContainedPhysicalPath());
        if ((attributes & FileAttributes.Directory) == 0
            || (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
        {
            throw new IOException("The created path is not an ordinary directory.");
        }
    }

    private static void RequireExactEffectPath(string actual, string expected)
    {
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            throw new ArgumentException("The effect path does not match the planned scaffold.", nameof(actual));
        }
    }

    private PhysicalPathResolution ResolveCandidate(
        ExtensionCreatePlan plan,
        string path)
        => _physicalPathResolver.ResolveCandidate(
            lexicalWorkspaceRoot: plan.Catalogue,
            physicalWorkspaceRoot: plan.CataloguePhysicalIdentity,
            candidatePath: path);

    private static ExtensionCreateEffectApplication Failed(
        ExtensionCreateFindingCode code,
        CliSemanticStatus status,
        string subject,
        string cause)
        => new(
            applied: false,
            finding: ExtensionCreateResultFactory.Finding(
                code: code,
                status: status,
                subject: subject,
                cause: cause));

    private static bool IsApplicationFailure(Exception exception)
        => exception is IOException
            or UnauthorizedAccessException
            or NotSupportedException
            or (ArgumentException and not ArgumentOutOfRangeException);
}
