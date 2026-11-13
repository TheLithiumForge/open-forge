---
title: CodeGraph
description: Review the pinned CodeGraph draft and its source navigation limits.
---

# CodeGraph

CodeGraph is a code index that finds symbols and likely callers in your project.

Use it to find files before an edit, then confirm the reported calls in source. In the frozen C# trial, it found 31 of 75 invocation sites, or 41.3%.

## Availability

The `1.6.2` definition is a draft. All six official release archives pass size, SHA-256 and GitHub build-attestation checks. Installation is blocked while R2 resolves the Claude prompt hook, the private launcher and native qualification. The production catalogue stays empty.

The [draft definition](../../../../src/tools/drafts/codegraph-1.6.2.json) records every URL, size, digest and signer identity. It keeps the release-tag commit separate from the attested workflow commit. The workflow makes commits before publishing. The draft doesn't assert that the two commits are equivalent.

## Project setup

The pinned source supports local Claude Code and Codex setup with an explicit target list. Noninteractive mode also writes a Claude `UserPromptSubmit` hook. `--no-permissions` doesn't disable it. MCP entries use `codegraph`, not `open-forge-codegraph`.

The [consent inventory](../../../../docs/tools/codegraph-consent.md) lists project files, legacy cleanup and account state that the source can touch. No installer was run for this draft.

## Index use

After a qualified setup, build and refresh the index through CodeGraph. First initialization still needs a reviewed path that keeps real Git hooks unchanged. Open Forge doesn't build or schedule indexes.

The [required Skill draft](../../../../src/tools/skills/open-forge-codegraph/SKILL.md) gives the exact build, sync and query forms, child environment and source checks. The managed ignore entry is `.codegraph/`. Telemetry defaults to off. Update checks, watchers and daemon startup stay off.

## Language limits

The trial measured C# precision at 88.6% and recall at 41.3%. TypeScript recall was 56.4%. Python recall was 82.3%. Check overloads, partial types, extension methods, interface dispatch, property reads and repeated calls in source. JavaScript accuracy is unmeasured. PowerShell graph support isn't reported.

The [external tools page](tools.md) explains command availability and separate consent.
