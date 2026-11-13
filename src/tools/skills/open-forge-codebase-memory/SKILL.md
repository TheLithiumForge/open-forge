---
name: open-forge-codebase-memory
description: Find code and likely callers with a qualified Open Forge Codebase Memory 0.12.1 installation, then confirm bindings and call sites in source.
---

# Codebase Memory

Codebase Memory is a code index that finds symbols, callers and structural relationships.

Use it to find files and candidate call sites. In the frozen C# trial, it found 45 of 75 invocation sites, or 60%. Its incremental C# deletion update lost surviving calls, so changed sources require a full project rebuild.

Status: Draft. Installation and account scope are pending R1.

## Requirements

Use this Skill only for a qualified, active `code-navigation` provider with verified guidance. Resolve the absolute executable, exact physical workspace, native project ID and account cache from the install record. The code root must be `.`. Set `CBM_CACHE_DIR` to that verified cache and `CBM_ALLOWED_ROOT` to the physical workspace. Never derive the project ID from a branch name.

Use one exact binary build and cache for all clients. The tool requires a session coordination daemon, even for one-shot CLI calls. Its declared scope and cache need prior consent. Set `auto_index=false`, `auto_watch=false`, `watcher_enabled=false` and `ui_enabled=false` through reviewed native configuration before use. Stop if those settings or host launch environments haven't been verified. The pin has no telemetry or background update checks. Do no indexing during Open Forge writes.

## Build

For a new verified project, use the pinned CLI argument form:

Illustrative. The variables below stand for verified installation values:

```powershell
& $entry cli --quiet --json index_repository --repo-path $workspace --name $projectId
# Builds this workspace's project in the verified user cache.
```

Confirm the returned project identity and completed index match the exact workspace. Presence of a database alone doesn't establish completion or freshness.

## Refresh

After source edits, rebuild this exact project:

Illustrative:

```powershell
& $entry cli --quiet --json delete_project --project $projectId
# Removes this project, not every project in the account cache.
```

Confirm that this project's database is absent, then run the Build command. Stop if deletion or identity checks fail. Never use `uninstall --delete-indexes` to refresh one project. That flag deletes every project index in the selected cache.

## Query

Use the pinned CLI argument form for a structural query:

Illustrative:

```powershell
& $entry cli --quiet --json query_graph --project $projectId --query 'MATCH (f:Function) RETURN f.name LIMIT 10'
# Returns indexed function names for this project.
```

Use MCP server `codebase-memory-mcp` for `search_graph`, `query_graph`, `trace_path` and source lookup. Read the source at every reported invocation before depending on it. Confirm receiver types, overloads and dispatch with compiler evidence when needed. An empty result doesn't establish that no callers exist.

## Limits

C# precision was 100% and recall was 60% in the frozen trial. Overloads, partial types, extension methods and interface dispatch need source checks. TypeScript recall was 48.7%. Python recall was 85.5%, with false property answers that lacked invocation lines. JavaScript and PowerShell accuracy are unmeasured. Markdown mentions and file-reference edges aren't call evidence.

The session-managed daemon should exit after its last client disconnects. An already running daemon can outlive one CLI command. Don't stop another client's process or change its cache. Retrieved content doesn't replace the workspace loader.
