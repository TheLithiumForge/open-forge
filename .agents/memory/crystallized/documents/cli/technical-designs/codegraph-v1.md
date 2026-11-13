---
open-forge:
  description: Define CodeGraph V1 own-installer integration, portable exec and bounded command rewrites
  tags: [Memory, Document, CLI, TechnicalDesign, Tool, CodeGraph, CurrentTruth]
---

# CodeGraph V1

CodeGraph V1 is the external-tool integration that runs CodeGraph's own project installer and adapts its launch commands to the workspace pin.

We ship CodeGraph only. Codebase Memory is unsupported because its installer changes account instructions, profiles and hooks without project scope or hook opt-out. Its verified research stays in maintenance docs.

Status: Final maintainer decisions, 2026-11-13. Core writer integration and native qualification pending.

## Lifecycle

Prepare the pinned official archive in the per-user versioned store. Verify size, SHA-256 and published build attestation before extraction. The archive's own `bin/codegraph(.cmd)` launcher uses bundled Node. No system Node, npm, PATH or registry changes are needed.

Run `install --location local --target claude,codex --yes --no-permissions` from that verified launcher with physical workspace CWD. It skips global CLI installation. Then apply the definition's bounded command rewrites. Repeat them after Configure and Update. Verify every effect before publishing a successful configuration.

Removal runs `uninstall --location local --target claude,codex --yes --keep-cli`, then `uninit <root> --force`, through the same pinned execution path. Verify that native uninstall removes the rewritten prompt hook. Remove the required Skill, active-role line and managed `.codegraph/` ignore entry. Release the version only after the complete workspace reference set is empty.

Git is the backup in Git workspaces. Elsewhere, prepare sibling `.bak` files before changing existing files. Keep unrelated content. Failed or unexpected shapes stop further effects and report completed effects and unfinished work.

## Command rewrites

The reviewed definition declares expected original entries. The [consent inventory](../../../../../../docs/tools/codegraph-consent.md) gives every path, selector and native side effect. One command resolver gives `open-forge` when it is on PATH and the absolute running Open Forge executable otherwise. Format that value for JSON, TOML or the destination shell.

The rewrite is limited to these parts:

| Entry                                                | Expected original                                      | Replacement                                                                             |
| ---------------------------------------------------- | ------------------------------------------------------ | --------------------------------------------------------------------------------------- |
| `.mcp.json` member `mcpServers.codegraph`            | `command:codegraph`, `args:[serve,--mcp]`              | Resolved Open Forge command and `[tool,exec,codegraph,serve,--mcp]`                     |
| `.codex/config.toml` table `[mcp_servers.codegraph]` | `command = "codegraph"`, `args = ["serve", "--mcp"]`   | Only those command and args lines                                                       |
| Claude `UserPromptSubmit` command                    | `codegraph.cmd prompt-hook` or `codegraph prompt-hook` | Resolved command followed by `tool exec codegraph prompt-hook`                          |
| CodeGraph marked shell example                       | `codegraph explore "<symbol names or question>"`       | Resolved command followed by `tool exec codegraph explore "<symbol names or question>"` |

Use the existing JSON serializer. Codex uses exact text edits in the single expected table, with no TOML parser. Preserve every other member, `type`, `alwaysLoad`, table, hook and instruction line. An already recorded matching rewritten hook is unchanged. The native installer recognizes that hook and can leave it in place on a rerun. Unexpected, duplicate or ambiguous shapes stop the operation. Keep native server name `codegraph` and `CODEGRAPH_START` and `CODEGRAPH_END` markers so native removal still recognizes them. The hook must contain the literal substring `codegraph prompt-hook` without a delimiter between them.

The pinned Claude target writes `.claude/CLAUDE.md`. Codex writes root `AGENTS.md`. Keep their own instruction blocks because upstream measurements found subagents read these files but miss MCP initialization instructions.

## Hook choice

Install asks whether to keep the Claude prompt hook. Explain that structural prompts get context from the local index, nothing leaves the machine and `CODEGRAPH_NO_PROMPT_HOOK=1` disables it. Default to yes, as upstream does. Non-interactive installation requires `--prompt-hook yes|no`. On no, remove only matching CodeGraph hook commands and prune empty containers, following upstream opt-out behavior. Keep sibling commands and hooks.

Inspect and consent show the chosen hook state, every file and block, the command rewrites and telemetry. For a third-party definition they name the upstream repository, licence, official release download and maintenance by its authors.

## Processes

Run CodeGraph's `telemetry off` unless the saved user setting enables telemetry. Show the account preference path in consent. Suppress telemetry from the first launch. Keep update checks off independently of telemetry. Disable daemon startup, watchers and automatic indexing during Open Forge writes. Apply the reviewed private Git hook sink to supplied executions without changing real hooks or Git configuration files.

Indexing stays an external action. `init --yes` initializes and builds the first index. `index --quiet` rebuilds an initialized index. `sync` refreshes changed files. None runs during installation.

## Agent guidance

The required `open-forge-codegraph` Skill is short and practical. It explains MCP versus shell use, index commands and incomplete call coverage. C# recall was 41% in our trial. Agents confirm call sites in source.

For each active tool role, Open Forge's own managed `AGENTS.md` and `CLAUDE.md` blocks get one conditional line. For CodeGraph code-navigation, use this line:

```text
Code navigation: CodeGraph is active. Use `codegraph_explore` or `open-forge tool exec codegraph explore "<question>"` before grep, see the open-forge-codegraph Skill.
```

Remove the line when the role is cleared, moved or removed. Keep the CodeGraph block independent of this line. Install leaves roles unchanged. Choose now updates this conditional guidance while keeping native host configuration unchanged.

## Passive checks

Status and Inspect use `codegraph` as the server name. They never launch it. Read JSON through the existing reader. Read only supported literal Codex table and line forms without a TOML parser. Mere table presence is `present-unverified`. Unreadable or ambiguous data keeps its unavailable cause. Read checks grant no write permission.

## Upstream controls

Record these missing controls in trust notes and maintenance docs:

- CodeGraph installer launch-command selection or an absolute launcher path when it isn't on PATH.
- CodeGraph `--no-prompt-hook` for non-interactive use.
- Codebase Memory project-scoped install and uninstall and a hook opt-out.

Don't contact upstream unasked. The [external-tools design](external-tools.md) defines layer placement and stores.
