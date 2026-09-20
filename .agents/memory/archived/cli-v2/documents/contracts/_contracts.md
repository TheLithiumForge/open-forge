---
open-forge:
  description: Historical CLI-v2 operation contracts kept only as raw input for the new CLI
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Contracts

This scope contains accepted behavioral, protocol, lifecycle, and compatibility
guarantees for replacement CLI operations and their shared boundaries.

A document belongs here only when a named implementation, provider, or
consumer can conform to or violate it. Other accepted material remains in the
authoritative current source for the question it answers.

Exact TypeScript declarations, named values, and schemas belong to production source once implemented. These documents define meaning, invariants, and compatibility expectations rather than maintaining a second source copy.

## Review Structure

Every direct Contract uses the same top-level review surface:

- `Scope` identifies the boundary and related authority.
- `Guarantees` contains domain-specific observable and testable obligations.
- `Boundaries` states important exclusions and behavior consumers must not infer.
- `Verification` defines the evidence required to prove conformance.
- `Related Current Sources` links only the authoritative sources that complete the boundary.

Domain-specific headings remain nested beneath those responsibilities. A
Contract adds `Compatibility And Evolution` only when persisted data, external
consumers, versioned protocols, or an explicit compatibility promise makes
change semantics independently material.

## Historical Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Historical CLI-v2 source: Explicit per-user shell-completion targets, selection, profile ownership, safe mutation, interruption recovery, and Framework-install handoff](completion-lifecycle.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Static and bounded dynamic shell-completion candidates, sources, workspace resolution, safety, cost limits, and fallback behavior](completion-protocol.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Explicit domain diagnosis, typed findings, completeness, stable ordering, safe repair projection, conflict handling, and rediagnosis semantics](diagnosis-and-repair.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Shared containment, identity, dispatch, persistence, verification, concurrency, and recovery boundaries for CLI filesystem effects](filesystem-effects.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Shared inventory, parsing, containment, traversal, fragment, diagnosis, and repair semantics for local Markdown references](local-references.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: One reviewable workspace configuration and lifecycle record for whole-Framework reconciliation, deliberate route exclusions, Extension ownership, exact-id dependencies, and advisory checksums](managed-lifecycle.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Shared request, planning, preflight, effect, application, verification, recovery, and evidence semantics for every replacement CLI mutation](mutation-execution.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Direct typed prerequisites, result behavior, and exact capability gates for every replacement CLI operation](operation-prerequisites.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Exact help, version, parse-failure, diagnostic, ordering, stream, and exit behavior at the replacement CLI parser boundary](parser-results.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: One typed request-construction path for explicit arguments, guided choices, defaults, confirmation, preview, and structured automation](request-construction.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Shared authored route topology, natural identity, metadata, generated-navigation, context projection, and invalid-state semantics for replacement CLI consumers](route-inventory.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: One-artifact Node.js, Bun, and Deno runtime floors, invocation, behavioral parity, capability gating, and release evidence](runtime-compatibility.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Source-visible safety review for external Extension payloads, concealed Markdown constructs, terminal-safe evidence, and authority independent from selection](source-review.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Exact shallow status facts, typed data, semantic outcomes, advisory suggestions, and exhaustive share-safe redaction behavior](status-results.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Workspace-aligned formatter discovery, trust classification, exact-file post-processing, checksum refresh, and manual follow-up](workspace-formatting.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
- [Historical CLI-v2 source: Git-first mutation recovery, Gitless sibling backups, in-process reversal, hard-stop evidence, and residual-state behavior](workspace-recovery.md) - #Memory #Archived #Contextual #Historical #CLI #CLIv2 #RawData
