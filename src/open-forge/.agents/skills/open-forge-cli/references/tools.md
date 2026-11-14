---
open-forge:
  description: Install published tools, check native effects and remove recorded project additions
  tags: [Skill, CLI, Reference]
---

# Tool setup

Tool setup is the CLI command group that installs published tools and records its project additions around native setup.

For example, a teammate restores the exact locked CodeGraph version, then runs CodeGraph's native setup.
Use Open Forge `0.9.0-beta.19` or newer.

## Install

Read the entry and preview before applying:

```sh
open-forge tool list
open-forge tool inspect codegraph
open-forge tool install codegraph@1.6.2 --channel github-release --dry-run
open-forge tool install codegraph@1.6.2 --channel github-release --yes
open-forge tool status codegraph
```

Inspect and the plan show channels, requirements, native outputs, Open Forge additions and account effects.
Use real output to check the exact version and machine facts.
Run native commands only within the task's authorized account and project.

## Commands

The command family is:

| Command                       | Job                                                 |
| ----------------------------- | --------------------------------------------------- |
| `tool list`                   | List catalogue entries                              |
| `tool inspect <id>`           | Read channels, native effects and requirements      |
| `tool status [id]`            | Check versions, additions, native settings and PATH |
| `tool install [id[@version]]` | Install and run native setup                        |
| `tool update [id[@version]]`  | Update declared tools within support                |
| `tool remove <id>`            | Remove project additions and offer native uninstall |

On named Install and Update, a bare ID chooses latest support.
Selectors accept `id@major.minor` or `id@exact stable SemVer`.
They don't accept `.x`, dist-tags or npm ranges.
Named Install and Update accept `--channel`.
Bare Install restores exact lock versions with normal channel choice.
Bare Update chooses newest support for declared tools.
Commit `.agents/open-forge.json` and `.agents/open-forge.lock.json` for teammates.
These paths stay fixed when you use a custom context root.

## Native effects

Keep upstream MCP names, hooks, permissions, instruction blocks, Skills and default client discovery.
Open Forge adds no per-tool usage Skill or managed guidance line.
CodeGraph uses local Claude and Codex setup, turns stored account telemetry off, unions the archive exclude and adds Gitignore coverage only when needed.
Codebase Memory uses account-wide native setup with `--skip-binary` after verified placement.
Its watcher defaults stay `auto_index=false`, `auto_watch=true` and `watcher_enabled=true`.
Open Forge adds a marked archive block to `.cbmignore`.

Direct commands are `codegraph` and `codebase-memory-mcp`.
CodeGraph needs `codegraph init` to build a project index after setup.
Status and Inspect show bin and manager PATH readiness.
Follow printed manual PATH instructions and restart terminals and agent apps after a PATH change.

## Consent and removal

Tool writers use `--yes` and `--dry-run`.
`--yes` accepts the shown plan and downloads.
PATH edits and shared deletion need separate live choices.
Dry-run reports network use when it resolves latest.

Codebase Memory native uninstall removes the account binary and all agent entries.
It requires live interactive confirmation naming those effects.
With `--yes`, no terminal or a declined account confirmation, remove only Open Forge project additions and print the native command.
Every project Remove keeps account-wide settings, reports them and prints manual change commands.
Kept account settings alone don't make removal incomplete.

## Results

Read status, findings and exit code.
JSON Tool results use schema 3 and `data.tools`.
Write results include the checked plan and completed or unfinished steps.
Tool commands run one at a time and add no state under `.open-forge-state/`.
On failure, use reported backups and verified progress. Changes aren't rolled back automatically.

Full options, native effects, records and result examples are in the [command reference](https://thelithiumforge.github.io/open-forge/guides/cli#tools).
