---
open-forge:
  description: Define published channels, native setup and additive effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool behavior

Tool behavior is the common flow that detects a tool, plans setup and checks its additions.
For example, CodeGraph runs its own local installer before Open Forge adds its archive exclusion.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Detection

Use a bounded version probe and typed version parser.
Declare timeout, output cap and environment changes in the catalogue.
CodeGraph probes set `CODEGRAPH_NO_DOWNLOAD=1`.
Status and dry-run therefore don't invoke its missing-bundle download fallback.
Normal tool execution keeps the upstream fallback.
Probes don't invoke setup or indexing. Open Forge's read code writes no tool records.
Native cache behavior stays with the tool.

Status detects self-updates against the lock and machine facts.
Inside support, offer reapply through named Install.
Outside support, warn and keep the existing install.
Don't disable native update checks, hooks, permissions, watchers or daemons.

## Channel choice

Unless an advertised `--channel` overrides it, use this order:

1. Reuse an existing supported install that meets the requested version.
2. Use a compatible manager from the tool's published ecosystem.
3. For an npm tool without a suitable Node, use a private Node and its own npm.
4. Use a verified GitHub release archive.

Detect npm, pnpm, Yarn Classic and bun through one bounded CLI version call each.
Check Yarn's major version and exclude Yarn Modern from global installation.
Read the project's lockfiles as preference hints.
Check each manager's runtime, version and global-bin PATH readiness.
A catalogue entry can require one client.
Ask about multiple usable choices. The common Interface defines the `--yes` fallback.
Keep the manager responsible for an existing installation and never create a second install.
A failed transfer doesn't silently switch channels.

## Versions

Profiles are non-overlapping major.minor ranges with exact tested versions and a minimum Open Forge version.
V1 starts with CodeGraph `1.6.x` at `1.6.2` and Codebase Memory `0.12.x` at `0.12.1`.
The minimum reviewed patch also limits the versions offered within its profile.
Keep older supported profiles and pins after later updates.

Named Install and Update without a selector choose latest support.
Bare Install uses the exact lock version when present.
A lock version outside support blocks before effects.
Resolve and disclose an exact version before installation.
Only known support in a newer catalogue justifies "update Open Forge first".

## Preview

Dry-run describes the channel, exact version where known, command vectors, files and additions.
Use the upstream dry run when its profile declares one and the installed tool is available.
Otherwise give a description.
Network access is allowed only when latest must be resolved, and the report discloses it.
No installation, config write, PATH change, shared deletion or Tool record write occurs.

## Setup

Run the documented upstream setup and uninstall argument vectors through the declared executable.
Keep arguments separate and use no shell strings.
Run both tools' own installers.
CodeGraph uses local Claude and Codex setup with its hook and permissions.
Codebase Memory uses its account-wide setup and default client discovery.
Disclose account scope and watcher defaults.
Open Forge writes no replacement MCP entries or Codex TOML sections.
It adds no tool Skill or managed-block line.

Back up declared changed files before invoking native setup.
Run native uninstall before removing Open Forge additions.
Guard any native PATH or shared-binary effects under the common consent rule.
Never request deletion of every project's indexes.

## Additions

Use only the additive rules in [Tool record behavior](../shared/tool-records/behavior.md).
The committed lock stores what Open Forge added.
Machine state stores native account-setting previous values, paths and PATH history.
There is no Tool folder, lease, journal or recovery dependency under `.open-forge-state/`.
Tool commands don't run concurrently by design and need no extra cross-process lock.
Keep existing destination permissions and expected-state checks.
Other commands' workspace-lock and recovery contracts stay unchanged.

## Verification

Verify the installed version, native command outcome and Open Forge additions before recording completion.
Publish verified additions so a later failure leaves a correct record of completed work.
Report partial native effects and retained sibling backups.
Don't invent prior values or delete an unrecorded addition after an interrupted write.
The [design](../../technical-designs/tool-setup.md) defines placement and interfaces.
