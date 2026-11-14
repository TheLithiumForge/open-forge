---
open-forge:
  description: Define tool specs, yes consent and shared result coordinates
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool interface

Tool is a command group that installs published tools and adds configuration.
For example, a teammate runs bare Install to get the exact versions in the committed lock.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Inputs

The six leaves are defined by the [group](_tool.md).
All leaves accept the existing [global flags](../shared/global-flags/interface.md).
Writer leaves accept `--yes`, `--dry-run` and repeated `--allow-path <path>`.
Install and Update also accept `--channel <channel>` for a named tool spec.
There is no Tool `--automatic`, `--allow-download` or value-taking `--version`.
The global version flag keeps its existing meaning.

A tool spec is `id` or `id@version`.
IDs are exact catalogue keys, not paths or source IDs.
The V1 version selector accepts `major.minor` or an exact stable SemVer.
Settings and catalogue profiles keep the `major.minor.x` range notation.
A bare ID selects the latest supported version.
An explicit selector must match an available supported profile and its minimum tool version.
Unknown IDs or channels, empty selectors, prereleases, extra operands and repeated singleton values are invalid input.
Registry tags and other npm range operators aren't part of this V1 selector grammar.
Repeated Boolean flags are idempotent.

Bare Install and Update select declared IDs in ordinal order.
Bare Install uses the exact version in each lock entry.
If a declared tool has no lock entry, it selects the latest version supported by its declared range.
Bare Update selects the newest supported version for each declaration.
A channel override requires a named spec.

## Consent

Show the plan before asking or applying.
`--yes` accepts that plan, including downloads, native setup and declared account-wide effects.
It chooses npm when manager selection is unresolved and npm is usable, then the first usable client in the fixed npm, pnpm, Yarn Classic, bun order.
A single usable project-lockfile preference wins before that fallback.
Without `--yes`, ask when several compatible managers are possible.
If neither a live terminal nor `--yes` is available, apply reports missing consent and performs no effects.

Even with `--yes`, unattended execution never changes PATH or deletes shared tool files or runtimes.
Print exact instructions and keep those optional actions unselected.
Codebase Memory uninstall removes its account-wide binary and all agent entries.
Run it only after live interactive confirmation that names those effects, and never when `--yes` is present.
With `--yes`, without a terminal or after declining, remove only Open Forge's project additions and print the native uninstall command.
Keep account-wide native settings on every project Remove and print the native command to change them back.
The [recorded choices](../../technical-designs/tool-setup-lanes.md#recorded-choices) explain this guarded native-uninstall case.

## Reports

Use the existing schema-3 envelope, statuses, exits, detail levels and streams from [Result coordinates](../shared/result-coordinates/interface.md).
The command identity is `tool <leaf>`.
Every leaf has `data.tools`, an array ordered by tool ID, even for one tool.
Each entry carries the leaf's tool facts, including Inspect facts.
Writers add `preview`, `plan` and `outcomes`.
Plans show exact versions where known, channels, managers and requirements, commands, scopes, native installer writes, Open Forge additions, telemetry, downloads, launchers, PATH choices, backups and unfinished work.
Say when dry-run needs network access to resolve latest.
Text and JSON consume the same facts.

Status and Inspect always show bin and current PATH membership.
Show the selected manager's global-bin path and readiness separately.
An unchecked restarted terminal or agent app isn't reported as verified.
Absolute machine paths appear only in reports and machine state.

## Results

The aggregate gives cancellation priority once invocation starts.
Otherwise precedence is invalid input, blocked, failed, incomplete, completed with warnings and completed.
Use the exact shared status spellings and exits.
Before a bare writer starts effects, preflight every tool.
If one is blocked, perform no effects.
Once effects start, stop at the first failure and report completed and unstarted work separately.
No result promises package-manager rollback.

## Limits

There is one active version per tool per machine.
Before replacing a shared installation, disclose the account-wide effect.
Tools run through direct commands.
V1 adds no Open Forge tool Skill, per-tool managed-block line, Run, Choose, Configure or role assignment.
Upstream MCP names, hooks, permissions, instruction blocks and Skills stay as the upstream installer writes them.
