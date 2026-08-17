---
tags:
  - Memory
  - Working
  - Handoff
  - KeepInMind
description: Sealed Gate 4 closeout and Gate 5 wake-up summary for the replacement Open Forge CLI release program.
---

# CLI Release Gate 4 Complete Handoff

## Seal

Sealed on 2026-08-17 after Gate 4 review and validation completed. Do not edit
this record. The mutable resumption state remains in the [CLI Release
Checkpoint](../checkpoints/cli-release.md).

This handoff supersedes the next actions in the earlier sealed Gate 2 Queue 31–33
and Gate 3 handoffs. Those records remain immutable history. This record is a
contextual transfer snapshot, not command or Architecture authority. It records
no current commit hash and does not assert a commit that does not yet exist.

## Current state

- Gates 1, 2, 3, and 4 are complete. Gate 2 remains non-shipping.
- Gate 3 Architecture was accepted with revisions. Gate 4 crystallized one
  accepted source set, finalized the physical `contracts/index/` and
  `contracts/references/` paths, archived history, reconciled the applicable
  Directives, Patterns, Templates, maps, and public documents, and completed
  review and bounded validation. The known legacy Doctor limitation remains
  preserved Gate 5 evidence and is not claimed clean.
- Gate 5 is the active next gate, but it is not accepted or started. The
  maintainer will review this wake-up summary and may authorize Gate 5.
- The replacement remains non-shipping. No replacement implementation source,
  `.slnx`, project, dependency lock, lifecycle file, workspace lock, package,
  Native AOT artifact, or release exists.
- Full retained delivery remains the complete accepted command surface.
  Completion is rejected, and no partial publication is accepted.

## Authority map

The current replacement CLI authority is:

- [CLI Architecture](../../crystallized/documents/cli/architecture.md), which
  defines the accepted implementation and release boundaries;
- the four overview Documents, [Command Contract Set](../../crystallized/documents/cli/command-contract-set.md),
  [Command Interface Contract](../../crystallized/documents/cli/command-interface-contract.md),
  [Command Behavior Contract](../../crystallized/documents/cli/command-behavior-contract.md),
  and [Command Technical Design](../../crystallized/documents/cli/command-technical-design.md);
- the [Shared CLI Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md);
- the detailed contracts under [`contracts/**`](../../crystallized/documents/cli/contracts/_contracts.md); and
- the scoped [CLI Directives](../../../directives/open-forge/cli/_cli.md) and
  [CLI Patterns](../../../patterns/open-forge/cli/_cli.md) for applicable work.

The mutable Working route contains only program state: the [CLI Release
Program](../cli-release/_cli-release.md), [Release Plan](../cli-release/release-plan.md),
[Decision Agenda](../cli-release/decision-agenda.md), and Checkpoint. Sealed
handoffs preserve transfer state but do not replace those authorities. Archived
audits, migration records, and reviews are historical evidence only.

## Accepted Architecture summary

- The replacement is an optional deterministic Framework accelerator with a
  predictable human maintenance surface. It is separate from the frozen
  `open-forge-old` executable and has no legacy compatibility or fallback path.
- The retained command tree is `find`, `index`, `status`, `context`,
  `references`, `doctor`, `repair`, `install`, `update`, and `cleanup`; the
  `route` group contains `inspect`, `list`, `init`, `create`, `update`, `move`,
  and `remove`; the `extension` group contains `list`, `inspect`, `create`,
  `install`, `update`, and `remove`. Completion is not a command.
- The accepted implementation is one C#/.NET 10 or newer production executable
  in one `OpenForge.Cli` project, targeting `net10.0` with SDK `10.0.101`,
  `rollForward=latestPatch`, and C# `14.0`. It uses the future
  `OpenForge.slnx`, mirrored managed tests, and a separate system-test project.
  The future source and test topology is accepted, but it does not exist yet.
- The accepted runtime boundaries are System.CommandLine 2.0.11, Markdig 1.3.2
  through one fixed CommonMark pipeline, YamlDotNet 18.1.0 through its
  source-generated semantic path, source-generated System.Text.Json metadata
  with reflection disabled, and the conservative
  `open-forge-markdown-v1` semantic identity. Unsupported equivalence fails
  closed.
- The replacement uses real cross-platform `System.IO`, real isolated operating-
  system temporary resources in tests, `.agents/open-forge.lifecycle.json` for
  schema-version-1 lifecycle state, and the actual OS-level
  `.agents/open-forge.lock` for workspace mutations. `extension create` is the
  only current no-workspace mutation exception.
- Every operation forms one concrete typed result. The structured envelope has
  schema version 1 with `schemaVersion`, `command`, `status`, `workspace`,
  `result`, and `next`. Recovery provenance binds the workspace, operation,
  target, artifact, expected identity, and recovery state.
- Evidence uses xUnit v3 through Microsoft Testing Platform. Each test has an
  explicit `DisplayName`, one durable `Feature` trait, and exactly one
  `Evidence` value: `Unit`, `Integration`, `EndToEnd`, or `PackageEndToEnd`.
- Release is exactly six RIDs: `win-x64`, `win-arm64`, `linux-x64`,
  `linux-arm64`, `osx-x64`, and `osx-arm64`. The launcher is
  `@thelithiumforge/open-forge`, with platform packages
  `@thelithiumforge/open-forge-win32-x64`,
  `@thelithiumforge/open-forge-win32-arm64`,
  `@thelithiumforge/open-forge-linux-x64`,
  `@thelithiumforge/open-forge-linux-arm64`,
  `@thelithiumforge/open-forge-darwin-x64`, and
  `@thelithiumforge/open-forge-darwin-arm64`. Packages are thin. Checksums,
  signatures, SBOM, provenance, OIDC attestation, support-floor execution, and
  main-only publication are required.

Architecture acceptance is not implementation, Native AOT, package, CI, or
release evidence.

## Gate 4 outcome

Gate 4 completed the accepted-knowledge and readiness boundary without changing
production behavior or starting implementation. The final source set has no
competing Working contract copies. The physical-identity regressions for the
new CLI remain implementation evidence for Gate 5.

## Validation boundary

Exact final validation evidence:

- Fresh targeted semantic and writing review findings were corrected, and
  focused rereviews passed.
- The targeted legacy `open-forge-old index` check succeeded during temporary
  compatibility staging; final paths were restored afterward.
- `open-forge-old load --bodies` succeeded during staging.

### Known legacy indexer limitation

- The final legacy Doctor remains unable to traverse the final `_index.md` and
  `_references.md` physical-identity compatibility names. This is the preserved
  new-CLI regression and is not claimed clean.

### Remaining final checks

- Custom final validation covered 142 scoped Markdown files, 2,276 local links,
  and 277 anchor references with zero broken.
- Candidate paths: 0. Temporary Find/Index IDs: 0. Old Working contract paths: 0. Current old lifecycle claims: 0. Bad Crystallized frontmatter tags: 0.
- Authored current files were formatted. Generated `Entries` were regenerated
  through the navigation process. `git diff --check` was clean. Sealed earlier
  handoffs were unchanged.

These checks establish the final prose, authority, routing, source identity,
generated-navigation, body-loading, link, anchor, and diff boundary. They do not
establish implementation, dependency, parser, filesystem, Native AOT, package,
CI, or release evidence.

## Clean implementation blocker

The replacement implementation boundary is clean: no replacement source,
`.slnx`, project, dependency lock, lifecycle file, workspace lock, package,
Native AOT artifact, or release exists. This is not a claim that the entire
worktree is clean; the unrelated worktree items below remain outside this
handoff.

## Gate 5 boundary and next decision

The maintainer's next decision is whether to authorize Gate 5 after reviewing
this summary. Do not mark Gate 5 decisions or evidence accepted before that
decision. If authorized, use this order:

1. Build the foundation with the exact tools and a real six-RID Native AOT
   spike. Prove the accepted dependencies and cross-cutting boundaries.
2. Implement and verify all retained commands in dependency order. Internal
   Tasks may move after deep Preflight, but they do not create publication
   slices.
3. Create thin npm packages. Wrappers install or invoke the canonical
   executable and never implement Framework behavior.
4. Complete CI and main-only release proof, including package journeys,
   support-floor execution, and supply-chain evidence.

Return to Architecture if the foundation or implementation materially fails an
accepted assumption. The replacement cannot ship until the complete retained
command surface and complete release proof are accepted.

## Key files and unrelated worktree items

- [CLI Release Program](../cli-release/_cli-release.md)
- [Release Plan](../cli-release/release-plan.md)
- [Decision Agenda](../cli-release/decision-agenda.md)
- [CLI Release Checkpoint](../checkpoints/cli-release.md)
- [CLI Architecture](../../crystallized/documents/cli/architecture.md)
- [Sources Of Truth map](../../../maps/sources-of-truth.md)

The pre-existing unrelated modification to `apm.lock.yaml` and the untracked
`nul` were excluded from Gate 4 and remain untouched. They are not Gate 4
evidence and are not part of the Gate 5 decision.
