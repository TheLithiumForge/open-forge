---
title: CodeGraph
description: Use the pinned CodeGraph index for project code navigation.
---

# CodeGraph

CodeGraph is a local code index that finds symbols and traces call paths in your project.

Use it to find code before an edit, then confirm call sites in source. Our C# trial found 31 of 75 invocation sites, or 41.3% recall. A missing graph result doesn't rule out a caller.

## Availability

CodeGraph `1.6.2` is the V1 tool. All six official release archives pass byte-size, published SHA-256 and GitHub build-attestation checks. The definition stays a blocked draft until the core writers and native journeys are ready. The [draft record](../../../../src/tools/drafts/codegraph-1.6.2.json) gives all pins, exact installer originals and rewrites.

CodeGraph is third-party software from `colbymchenry/codegraph`, under MIT, downloaded from its official release and maintained by its authors. Its archive launcher uses bundled Node. System Node, npm and PATH changes aren't needed.

## Project setup

CodeGraph's own installer configures Claude Code and Codex locally. It writes MCP server `codegraph` in `.mcp.json` and `.codex/config.toml`, instruction blocks in `.claude/CLAUDE.md` and `AGENTS.md` and an optional Claude prompt hook. Open Forge rewrites only the command parts to `open-forge tool run codegraph`. It keeps names and markers so CodeGraph's own uninstall recognizes them.

If Open Forge isn't on PATH, generated commands use its running executable's absolute path. An unexpected original entry stops the rewrite and reports its file. The [consent inventory](../../../../docs/tools/codegraph-consent.md) lists every output and rewrite. It also describes legacy cleanup and account telemetry state.

## Teammate setup

Bare `open-forge tool install` installs every tool declared by the workspace. After cloning, a teammate can run it to prepare the local pins. If CodeGraph is declared but missing locally, `tool run` tells you to run that command. It doesn't install silently. Normal consent and hook choice still apply.

## Prompt hook

The Claude `UserPromptSubmit` hook adds context from the local index on structural prompts. Nothing leaves the machine. Open Forge asks whether to keep it. Defaults to yes. Non-interactive installs take `--prompt-hook yes|no`. Choosing no removes only that hook entry. `CODEGRAPH_NO_PROMPT_HOOK=1` disables it at runtime.

Telemetry defaults to off through CodeGraph's own command and is a visible setting. Update checks stay off. Watchers and automatic indexing don't run during Open Forge writes.

## Index use

After a qualified install and your agreement to indexing, run from the workspace root. These commands wait for core Run integration:

```sh
open-forge tool run codegraph init --yes
open-forge tool run codegraph index --quiet
open-forge tool run codegraph sync
open-forge tool run codegraph explore "Where is the catalogue loaded?"
```

`init --yes` builds the first index. `index --quiet` rebuilds an initialized index. `sync` refreshes changed files. The final command queries the index. With MCP, use `codegraph_explore` for the same purpose. Installation doesn't build an index.

After indexing, choose the code-navigation role. The required `open-forge-codegraph` Skill gives practical usage. Open Forge's managed agent block adds one conditional CodeGraph line while the role is active. CodeGraph's own instruction block stays too. The managed ignore entry is `.codegraph/`.

## Remove

Removal runs CodeGraph's local uninstall with `--keep-cli`, then `uninit <root> --force`. It removes recognized project entries and the index. Open Forge removes its required Skill, role line and ignore entry and releases unused store references. Other workspace versions and unrelated host content stay in place.

## Language limits

C# precision was 88.6% and recall was 41.3%. TypeScript recall was 56.4%. Python recall was 82.3%. Confirm overloads, partial types, extension methods, dispatch and repeated calls in source. JavaScript accuracy wasn't measured. The trial has no CodeGraph PowerShell graph support.

The [tools page](tools.md) explains command availability and separate consent.
