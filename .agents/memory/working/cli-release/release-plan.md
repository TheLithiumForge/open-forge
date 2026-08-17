---
open-forge:
  description: Current gates, tasks, dependencies, validation, and stop states for the new CLI release program
  tags: [Memory, Working, CLI, Release, Program, Gate, Plan, Contextual]
---

# CLI Release Plan

## Authority and use

The maintainer's accepted direction and the current authoritative sources define
the replacement CLI's meaning. This file records program status, execution order,
validation, and stop states. It does not define command behavior or make the
replacement shipped.

The current authority set is the accepted [CLI Architecture](../../crystallized/documents/cli/architecture.md),
the consolidated [Command Contract Set](../../crystallized/documents/cli/command-contract-set.md),
the [Shared CLI Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md),
and the detailed contracts under [`contracts/`](../../crystallized/documents/cli/contracts/_contracts.md).
The scoped [CLI Directives](../../../directives/open-forge/cli/_cli.md) and [CLI
Patterns](../../../patterns/open-forge/cli/_cli.md) govern applicable work.
Mutable Working retains only the program, Decision Agenda, Release Plan, active
Task, and Checkpoint; sealed handoffs are transfer snapshots, not command
authority.
The [archived Gate 1 records](../../archived/cli-release/gate-1-audit.md),
[migration ledger](../../archived/cli-release/contract-migration-ledger.md), and
[review history](../../archived/cli-release/review/gate-2-review-overview.md)
preserve history and evidence only. They never replace current authority.

## Current status

Gates 1, 2, 3, and 4 are complete. Gate 2 remains non-shipping. Gate 3
Architecture was accepted by the maintainer with all revised decisions. Gate 4
crystallized one accepted source set, finalized the `contracts/index/` and
`contracts/references/` paths, archived history, reconciled the applicable
Directives, Patterns, Templates, maps, and public documents, and completed its
review and bounded validation. The known legacy Doctor limitation remains
preserved Gate 5 evidence and is not claimed clean.

Gate 5 is authorized and active through the [Foundation And Native AOT Spike
Task](task-foundation-aot-spike.md). The `.slnx`, foundation source, production
project, two test projects, flexible stable .NET 10 policy, exact central direct
dependency versions, and local `win-x64` Native AOT evidence now exist. The
foundation Task is accepted for local integration. The replacement remains
non-shipping. No retained command, accepted shipping executable, lifecycle file,
workspace lock, package, release artifact, or release exists. No staging or
candidate path is current.

| Gate | Purpose                                                           | State                               | Exit authority                                  |
| ---- | ----------------------------------------------------------------- | ----------------------------------- | ----------------------------------------------- |
| 1    | Evidence and current-truth audit                                  | Complete; non-shipping              | Maintainer approval                             |
| 2    | Product and command contract decisions                            | Complete; non-shipping              | Maintainer acceptance                           |
| 3    | Architecture and foundation design                                | Complete; accepted                  | Maintainer acceptance                           |
| 4    | Crystallization, final indexing, review, and readiness validation | Complete; accepted/current          | Maintainer acceptance                           |
| 5    | Complete implementation sequence and release proof                | Active; foundation locally accepted | Reproducible evidence and maintainer acceptance |
| 6    | Post-implementation documentation, history, and release closeout  | Blocked until Gate 5                | Maintainer release and closeout acceptance      |

## Completed Gate 1 and Gate 2 foundations

- The replacement is an optional agent-first Framework accelerator with a
  predictable human maintenance surface. Operations are stateless and
  deterministic. Plain Markdown remains complete without the CLI.
- The retained delivery surface is all current contracted commands. Completion
  is rejected, and no partial `context` plus `find` publication or retained-later
  release slice exists.
- `src/cli-mvp/` contains frozen source for `open-forge-old`; neither
  `src/cli-mvp/` nor `open-forge-old` is replacement implementation or contract
  authority. `open-forge-old` remains the separate frozen executable. Rune
  remains outside this program.
- The split contract migration is complete. The old mixed sources were removed
  after lossless reconciliation, and the final shared and command-local contract
  topology is now under the Crystallized CLI route.
- No command review is active. Settled, rejected, and superseded review material
  remains archived historical context.

## Current source set

The accepted Crystallized source set is:

- [CLI Architecture](../../crystallized/documents/cli/architecture.md), which defines the accepted implementation and
  release boundaries.
- [Command Contract Set](../../crystallized/documents/cli/command-contract-set.md), which provides the concise overview of command-contract roles, topology, and authority boundaries.
- [Shared CLI Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md), which defines cross-command conventions.
- [Detailed command contracts](../../crystallized/documents/cli/contracts/_contracts.md), which define command-local behavior and interfaces.

These sources are current authority without competing Working copies. The final
contract paths for the `index` and `references` commands are `contracts/index/`
and `contracts/references/`. Physical-identity regressions are retained as Gate 5
implementation evidence rather than as a current staging obligation.

## Gate 3 — Accepted Architecture result

Gate 3 accepted the Architecture and all revised decisions. The result is
summarized here; the [Architecture](../../crystallized/documents/cli/architecture.md) defines the exact detail.

- The accepted design specifies `OpenForge.slnx` with one production project,
  `OpenForge.Cli`, and one future production executable. The source project is
  physically rooted at `src/open-forge-cli/OpenForge.Cli`. Managed tests mirror
  production paths under
  `tests/open-forge-cli/OpenForge.Cli.Tests`, with a separate
  `OpenForge.Cli.SystemTests` project for complete system and end-to-end
  boundaries. Solution folders cannot hide physical folders.
- The composition root explicitly builds the command tree, binds requests,
  composes capabilities, selects rendering, and maps results to process exits.
  Direct construction and pure capability functions come first. Dependency
  injection is earned only by demonstrated composition or lifecycle needs, must
  be source-generated and Native-AOT-safe, and is not a default container
  boundary.
- The accepted dependencies are System.CommandLine 2.0.11, Markdig 1.3.2 through
  one fixed CommonMark pipeline, YamlDotNet 18.1.0 through source-generated
  semantic models, and source-generated System.Text.Json metadata with reflection
  disabled. The conservative `open-forge-markdown-v1` fingerprint is the accepted
  semantic identity. Unsupported equivalence fails closed.
- The only replacement lifecycle document is
  `.agents/open-forge.lifecycle.json`, schema version 1, with isolated
  `framework` and `extensions` sections in one common envelope. The replacement
  does not read, recognize, migrate, alias, or fall back to old lifecycle or
  Extension files. Existing old-format files remain ordinary untouched content
  outside replacement authority.
- The BCL, especially real `System.IO`, is the first filesystem boundary. Tests
  use real isolated operating-system temporary resources. Operations that mutate
  the selected workspace acquire the actual OS-level lock at
  `.agents/open-forge.lock`; `extension create` has no workspace subject and is
  the only current no-workspace mutation exception. File existence is not lock
  ownership. No fake or virtual filesystem is part of the design.
- Every operation forms one concrete typed result. The structured envelope is
  schema version 1 with `schemaVersion`, `command`, `status`, `workspace`,
  `result`, and `next`. The seven semantic statuses have fixed exits, and
  recovery provenance binds workspace, operation, target, artifact, expected
  identity, and recovery state. Gate 5 must prove these boundaries.
- Tests use xUnit v3 through Microsoft Testing Platform. Every test has an
  explicit `DisplayName`, one durable `Feature` trait, and exactly one
  `Evidence` trait from `Unit`, `Integration`, `EndToEnd`, or
  `PackageEndToEnd`.
- The release publishes exactly six RIDs: `win-x64`, `win-arm64`, `linux-x64`,
  `linux-arm64`, `osx-x64`, and `osx-arm64`. The package graph uses the launcher
  `@thelithiumforge/open-forge` and the six matching
  `@thelithiumforge` platform packages. Packages contain no download,
  postinstall, compilation, or behavioral wrapper. Checksums, signatures, SBOM,
  provenance, OIDC attestation, support-floor execution, and main-only
  publication are release requirements.

Architecture acceptance does not provide source, Native AOT, package, CI, or
release evidence. Those are Gate 5 proof obligations.

## Gate 4 — Completed closeout

Gate 4 is complete. It established one accepted Crystallized source set for the
replacement CLI:

- The CLI Architecture, consolidated Command Contract Set, Shared CLI Operation
  Contract, and detailed `contracts/**` are the current command source set.
- The final physical contract paths are `contracts/index/` and
  `contracts/references/`. No staging or candidate path is current.
- Gate 1 audits, migration records, and review records were archived as
  historical evidence. Directives, Patterns, Templates, maps, and public
  documents were reconciled with the accepted source set.
- Generated `Entries` were regenerated through the repository's navigation
  process. Their interiors remain generated navigation, not authored authority.

The final validation evidence is:

- Fresh targeted semantic and writing review findings were corrected, and
  focused rereviews passed.
- The targeted legacy `open-forge-old index` check succeeded during temporary
  compatibility staging; the final paths were restored afterward.
- `open-forge-old load --bodies` succeeded during staging.
- The final legacy Doctor remains unable to traverse the final `_index.md` and
  `_references.md` physical-identity compatibility names. This preserved
  new-CLI regression is not claimed clean.
- Custom final validation covered 142 scoped Markdown files, 2,276 local links,
  and 277 anchor references with zero broken.
- Candidate paths: 0. Temporary Find/Index IDs: 0. Old Working contract paths: 0. Current old lifecycle claims: 0. Bad Crystallized frontmatter tags: 0.
- Authored current files were formatted, `git diff --check` was clean, and
  sealed earlier handoffs were unchanged.

Gate 4 did not implement a command, create a package, run a release, or accept
Gate 5. The replacement remains non-shipping.

## Gate 5 — Active implementation plan

Gate 5 is authorized and active. The first bounded Task established the stable
.NET 10 foundation and local Windows `win-x64` Native AOT evidence without
implementing a retained command. It prepared the six-RID workflow; WSL, macOS,
the native runner jobs, and support-floor execution remain later Gate 5 evidence.
Packaging, publication, and release remain outside this Task, and the replacement
remains non-shipping.

Use this order:

1. **Foundation and tools:** locally accepted through the
   [Foundation And Native AOT Spike Task](task-foundation-aot-spike.md). It proves
   the stable .NET 10 SDK baseline, exact direct package inputs, accepted parser,
   serialization, filesystem, result, test, build, and Windows Native AOT
   boundaries. Cross-platform execution remains later evidence.
2. **All retained commands:** implement and verify every accepted command in
   dependency order. Completion remains rejected. Internal Tasks may move after
   deep Preflight, but they do not create publication slices.
3. **Thin npm packages:** package the canonical executable through the launcher
   and six platform packages. Wrappers never implement command behavior.
4. **CI and main-only release proof:** prove build, test, Native AOT, package,
   support-floor, supply-chain, and release journeys. Publication runs only from
   `main`.

Return to Architecture if the foundation or implementation materially fails an
accepted assumption. The replacement cannot ship until the complete retained
command surface and its complete release proof are accepted. There is no
partial publication.

## Historical validation

The archived [Gate 1 audit and registers](../../archived/cli-release/gate-1-audit.md), [contract migration ledger](../../archived/cli-release/contract-migration-ledger.md), and [settled review records](../../archived/cli-release/review/gate-2-review-overview.md) retain prior validation detail. Historical checks established prose, routing, links, generated-navigation, body-loading, and diff consistency for their scopes. They did not establish implementation, dependency, parser, filesystem, Native AOT, package, CI, or release evidence.

## Stop conditions

Stop for maintainer judgment when work encounters an unaccepted product or
Framework-semantic boundary, a Rune expansion, a safety bypass, a new package
channel, a platform promise outside the Architecture, evidence that invalidates
an accepted decision, generated changes outside scope, or an inseparable
pre-existing change.
