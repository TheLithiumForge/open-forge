using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;

internal static class RouteInclusionTargetResolver
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static async ValueTask<RouteInclusionTarget> ResolveAsync(WorkspaceLayout layout, RouteInclusionRequest request, CancellationToken token)
    {
        var target = Resolve(layout, request);
        if (target.Kind != RouteInclusionTargetKind.Folder || request.Action == RouteInclusionAction.Exclude) return target;
        var intended = layout.OutsideRoutes.Any(route => route.DirectoryPath == target.Path)
            ? layout : layout.WithOutsideRoutes(layout.OutsideRoutes.Select(route => route.DirectoryPath).Append(target.Path));
        var reader = new WorkspaceOutsideRouteReader(new PhysicalPathResolver(), intended);
        var observations = await reader.ReadAsync(token).ConfigureAwait(false);
        var directory = observations.Single(read => read.Descriptor.DirectoryPath == target.Path);
        if (directory.State != WorkspaceOutsideRouteReadState.Available)
            throw Invalid(request, directory.Issues.FirstOrDefault()?.Cause ?? "Choose an accessible ordinary directory without aliases or reserved overlaps.");
        return target with { DirectoryRead = directory, Expectations = [.. reader.Expectations] };
    }

    internal static RouteInclusionTarget Resolve(WorkspaceLayout layout, RouteInclusionRequest request)
    {
        var operand = request.File.Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(operand) || Path.IsPathRooted(operand)
            || operand.Any(c => char.IsControl(c) || c is '*' or '?' or '[' or ']' or '{' or '}' or ':' or '#'))
            throw Invalid(request, "Use one workspace-relative text file without a glob, URL or fragment.");
        var absolute = Path.GetFullPath(operand, request.Workspace.LexicalRoot);
        if (!PhysicalContainment.Contains(request.Workspace.LexicalRoot, absolute))
            throw Invalid(request, "Choose a file inside this workspace.");
        var path = Path.GetRelativePath(request.Workspace.LexicalRoot, absolute).Replace('\\', '/');
        if (WorkspaceRuntimePaths.Contains(request.Workspace, Path.GetFullPath(path, request.Workspace.PhysicalRoot)))
            throw Invalid(request, "Choose a project document outside the reserved workspace runtime directory.");
        if (path.Split('/').Any(segment => segment.Equals(WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparison.OrdinalIgnoreCase)))
            throw Invalid(request, "Choose a project document outside Git metadata.");
        var controls = new[] { layout.LoaderPath, WorkspaceSettingsDefinitions.RelativePath, WorkspaceOwnershipDefinitions.RelativePath };
        if (controls.Contains(path, PhysicalIdentityTracker.PathComparer))
            throw Invalid(request, "Choose a project document instead of a workspace control file.");
        if (request.Action == RouteInclusionAction.Exclude && layout.OutsideRoutes.Any(route => route.DirectoryPath == path))
            return ResolveFolder(layout, request, path);
        var resolution = new PhysicalPathResolver().ResolveCandidate(request.Workspace.LexicalRoot, request.Workspace.PhysicalRoot, absolute);
        if (resolution.State is not (PhysicalPathState.Contained or PhysicalPathState.Missing))
            throw Invalid(request, "Choose a file whose links stay inside this workspace.");
        if (resolution.State == PhysicalPathState.Missing)
        {
            if (request.Action == RouteInclusionAction.Include) throw Invalid(request, "Create the file first, then include it.");
        }
        else
        {
            var physical = resolution.GetContainedPhysicalPath();
            if (WorkspaceRuntimePaths.Contains(request.Workspace, physical))
                throw Invalid(request, "Choose a project document outside the reserved workspace runtime directory.");
            if (controls.Any(control => PhysicalIdentityTracker.PathComparer.Equals(physical, Path.GetFullPath(control, request.Workspace.PhysicalRoot))))
                throw Invalid(request, "Choose a project document instead of a workspace control file.");
            var relativePhysical = Path.GetRelativePath(request.Workspace.PhysicalRoot, physical).Replace('\\', '/');
            if (relativePhysical.Split('/').Any(segment => segment.Equals(WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparison.OrdinalIgnoreCase)))
                throw Invalid(request, "Choose a project document outside Git metadata.");
            if (Directory.Exists(physical))
            {
                return ResolveFolder(layout, request, path);
            }
            try { StrictUtf8.GetString(File.ReadAllBytes(physical)); }
            catch (DecoderFallbackException) { throw Invalid(request, "Save the file as UTF-8 text, then retry."); }
        }
        ValidateContentTarget(layout, request, path);
        return new RouteInclusionTarget { Path = path, Kind = RouteInclusionTargetKind.File };
    }

    internal static RouteInclusionTarget ResolveFolder(WorkspaceLayout layout, RouteInclusionRequest request, string path)
    {
        var operand = request.File.Replace('\\', '/');
        if (operand.StartsWith(SourceWorkspaceRelativePath.LiteralReferencePrefix, StringComparison.Ordinal))
            operand = operand[SourceWorkspaceRelativePath.LiteralReferencePrefix.Length..];
        if (operand.TrimEnd('/') != path)
            throw Invalid(request, "Use the canonical directory path without traversal or lexical aliases.");
        WorkspaceOutsideRoutePath.Validate(layout.MainRoot,
            layout.OutsideRoutes.Where(route => route.DirectoryPath != path).Select(route => route.DirectoryPath).Append(path));
        return new RouteInclusionTarget
        {
            Path = path,
            Kind = RouteInclusionTargetKind.Folder,
            CataloguePath = WorkspaceRoutePath.CanonicalEntrypointPath(path),
        };
    }

    internal static void ValidateContentTarget(WorkspaceLayout layout, RouteInclusionRequest request, string path)
    {
        if (layout.IsImplicitContentDestination(path) && layout.TryGetSourceRelativePath(path, out var relative))
        {
            var parts = relative.Split('/');
            if (parts.Length == 1) throw Invalid(request, $"Use a direct child folder's catalogue instead. Files directly in {layout.MainRoot} are not routed.");
            var catalogue = $"{layout.CategoryDirectory(parts[0])}/_{parts[0]}{SourceFormClassifier.MarkdownExtension}";
            if (path != catalogue) throw Invalid(request, $"Use the catalogue '{catalogue}' that lists this file instead.");
            if (request.Description is not null || request.Tags is not null || request.ApplyTo is not null)
                throw Invalid(request, $"Change the frontmatter in '{catalogue}' instead of supplying metadata options.");
        }
    }

    internal static ArgumentException Invalid(RouteInclusionRequest request, string cause)
        => new($"'{request.File}': {cause}");

    internal static async ValueTask<FileStateSnapshot> ReadAsync(CliWorkspace workspace, string path, CancellationToken token)
    {
        var logical = Path.GetFullPath(path, workspace.LexicalRoot);
        if (WorkspaceRuntimePaths.Contains(workspace, Path.GetFullPath(path, workspace.PhysicalRoot)))
            throw new IOException($"'{path}' is inside the reserved workspace runtime directory.");
        var resolution = new PhysicalPathResolver().ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, logical);
        if (resolution.State == PhysicalPathState.Missing) return FileStateSnapshot.Missing(logical);
        if (resolution.State != PhysicalPathState.Contained) throw new IOException($"'{path}' is not safely contained in the workspace.");
        var physical = resolution.GetContainedPhysicalPath();
        if (WorkspaceRuntimePaths.Contains(workspace, physical))
            throw new IOException($"'{path}' is inside the reserved workspace runtime directory.");
        if ((File.GetAttributes(physical) & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
            throw new IOException($"'{path}' must be an ordinary file.");
        return FileStateSnapshot.File(logical, physical, await File.ReadAllBytesAsync(physical, token).ConfigureAwait(false));
    }
}
