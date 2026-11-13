using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

// Registration point: bind Services beside ToolEntryPlanner.Services in the writer composition.
internal sealed class ToolRoleGuidancePlanner(FrameworkPayload? payload = null)
{
    private static readonly string[] Files = [WorkspaceLayoutNames.AgentHostFileName, WorkspaceLayoutNames.ClaudeHostFileName];
    internal ToolRoleGuidanceServices Services => new(PlanAsync, ObserveAsync);

    internal ValueTask<ToolRoleGuidancePlan> PlanAsync(ToolRoleGuidanceRequest request, CancellationToken token) => ReadAsync(request, false, token);
    internal async ValueTask<ToolRoleLineCheck[]> ObserveAsync(ToolRoleGuidanceRequest request, CancellationToken token)
        => (await ReadAsync(request, true, token).ConfigureAwait(false)).Checks;

    internal ToolRoleGuidancePlan Plan(ToolRoleGuidanceRequest request, IReadOnlyDictionary<string, byte[]?> files)
        => Evaluate(request, files, false);
    internal ToolRoleLineCheck[] Observe(ToolRoleGuidanceRequest request, IReadOnlyDictionary<string, byte[]?> files)
        => Evaluate(request, files, true).Checks;

    internal async ValueTask<bool> RecheckAsync(ToolRoleGuidanceRequest request, ToolRoleLineEdit edit, CancellationToken token)
    {
        var current = await ToolRegistrationFiles.ReadAsync(request.WorkspaceRoot, edit.File, token).ConfigureAwait(false);
        return current is null ? edit.Before is null : edit.Before is not null && current.AsSpan().SequenceEqual(edit.Before);
    }

    private async ValueTask<ToolRoleGuidancePlan> ReadAsync(ToolRoleGuidanceRequest request, bool observe, CancellationToken token)
    {
        var files = new Dictionary<string, byte[]?>(StringComparer.Ordinal);
        var unavailable = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in Files)
        {
            token.ThrowIfCancellationRequested();
            try { files[file] = await ToolRegistrationFiles.ReadAsync(request.WorkspaceRoot, file, token).ConfigureAwait(false); }
            catch (Exception exception) when (ToolRegistrationFiles.IsReadFailure(exception)) { unavailable[file] = exception.Message; }
        }
        var plan = Evaluate(request, files, observe);
        var checks = plan.Checks.Select(check => unavailable.TryGetValue(check.File, out var cause)
            ? check with { State = ToolRoleLineState.Unavailable, Cause = cause } : check).ToList();
        foreach (var (file, cause) in unavailable.Where(pair => !checks.Any(check => check.File == pair.Key)))
            checks.Add(new(string.Empty, string.Empty, file, ToolRoleLineState.Unavailable, cause));
        return new(checks.Any(check => check.State == ToolRoleLineState.Unavailable) ? [] : plan.Edits, [.. checks]);
    }

    private ToolRoleGuidancePlan Evaluate(ToolRoleGuidanceRequest request, IReadOnlyDictionary<string, byte[]?> files, bool observe)
    {
        _ = ToolRoleGuidanceRenderer.Select(request);
        if (Path.GetFullPath(request.WorkspaceRoot) != request.Layout.Workspace.LexicalRoot)
            throw new ArgumentException("The role guidance layout belongs to another workspace.", nameof(request));
        var edits = new List<ToolRoleLineEdit>();
        var checks = new List<ToolRoleLineCheck>();
        var failed = false;
        foreach (var file in Files)
        {
            files.TryGetValue(file, out var before);
            try
            {
                var rendered = ToolRoleGuidanceRenderer.Render(request, file, before, payload, observe);
                checks.AddRange(rendered.Checks);
                if (!observe && (before is null ? rendered.After.Length > 0 : !before.AsSpan().SequenceEqual(rendered.After)))
                {
                    var validation = ToolRoleGuidanceRenderer.Render(request, file, rendered.After, payload, observe: true);
                    if (validation.Checks.Any(check => check.State != ToolRoleLineState.Matching)) throw new InvalidDataException("The planned role lines failed validation.");
                    edits.Add(new(file, before, rendered.After));
                }
            }
            catch (Exception exception) when (ToolRegistrationFiles.IsShapeFailure(exception))
            {
                failed = true;
                if (request.Active.Length == 0) checks.Add(new(string.Empty, string.Empty, file, ToolRoleLineState.Drifted, exception.Message));
                else checks.AddRange(request.Active.Select(selection => new ToolRoleLineCheck(selection.Guidance.Role, selection.ToolId, file, ToolRoleLineState.Drifted, exception.Message)));
            }
        }
        return new(failed ? [] : [.. edits], [.. checks]);
    }
}
