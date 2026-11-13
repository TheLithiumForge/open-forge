---
title: Codebase Memory
description: Review the Codebase Memory draft, account scope and source checks.
---

# Codebase Memory

Codebase Memory is a code index that finds symbols, callers and structural relationships.

Use it to find candidate call sites, then read them in source. In the frozen C# trial, it found 45 of 75 invocation sites, or 60%. Changed C# sources need a full project rebuild because an incremental deletion update lost surviving calls.

## Availability

The `0.12.1` definition is a draft. All six selected official archives pass size, SHA-256 and Sigstore signature checks. Linux uses static portable assets. Installation is blocked on R1 and native qualification. The production catalogue stays empty.

The [draft definition](../../../../src/tools/drafts/codebase-memory-0.12.1.json) records URLs, sizes, digests, bundles and signer identity. A blob signature binds archive bytes to that identity. It doesn't supply source-build provenance.

## Account scope

The installer configures account-level Claude Code and Codex clients. It writes MCP entries, Skills, profiles, Codex instructions and hooks. It has no `--no-hooks`, instruction opt-out or project-scope flag. `--clients=claude,codex` limits the client set. `--skip-binary` skips binary placement and installation's PATH change. Uninstall still sweeps detected clients and can change the Windows user PATH.

The [consent inventory](../../../../docs/tools/codebase-memory-consent.md) names those files and effects. R1 must resolve the scope before installation. MCP entries use `codebase-memory-mcp`.

## Index use

Indexes live in the verified user cache, so no workspace ignore entry is added by default. Use only the physical workspace root. Set automatic indexing, watching and the UI off before use. One-shot CLI calls also join the account's session daemon. Its build and cache need separate consent.

The [required Skill draft](../../../../src/tools/skills/open-forge-codebase-memory/SKILL.md) gives the exact build, exact-project rebuild and query forms. `uninstall --delete-indexes` deletes every project index in the selected cache. This pin has no telemetry or background update checks.

## Language limits

C# precision was 100% and recall was 60% in the trial. TypeScript recall was 48.7%. Python recall was 85.5%. Confirm overloads, partial types, extension methods and interface dispatch in source. Markdown mentions aren't call evidence. JavaScript and PowerShell accuracy are unmeasured.

The [external tools page](tools.md) explains the common command and consent model.
