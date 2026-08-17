---
open-forge:
  description: Active CLI program state, settled command dispositions, accepted direction, and gate state for the new Open Forge CLI
  tags: [Memory, Working, CLI, Release, Program, Gate, Contextual, Active, KeepInMind]
---

# CLI Release Program

This Working route records the active program state for implementing, verifying,
and eventually releasing the replacement Open Forge CLI. It remains
`#Contextual` Memory. The maintainer's accepted direction and the current linked
sources define accepted meaning; this route does not define command behavior.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Current status

- Gates 1, 2, 3, and 4 are complete. Gate 2 remains non-shipping. Shell
  completion is rejected, so the product has no completion command,
  responsibility, or implementation target.
- Gate 3 Architecture was accepted by the maintainer with all revised
  decisions. Gate 4 crystallized one accepted source set, finalized the
  `contracts/index/` and `contracts/references/` paths, archived history,
  reconciled the applicable Directives, Patterns, Templates, maps, and public
  documents, and completed its review and bounded validation. The known legacy
  Doctor limitation remains preserved Gate 5 evidence and is not claimed clean.
- Gate 5 is the active next gate, but it is not accepted or started. The
  maintainer will review the sealed wake-up summary and may authorize Gate 5.
- The replacement remains non-shipping. `src/cli-mvp/` contains frozen source
  for `open-forge-old`; neither `src/cli-mvp/` nor `open-forge-old` is replacement
  implementation or contract authority. `open-forge-old` remains the separate
  frozen executable.
- No replacement implementation source, `.slnx`, project, dependency lock,
  lifecycle file, workspace lock, package, Native AOT artifact, or release
  exists.
- The final contract paths are `contracts/index/` and `contracts/references/`.
  No staging or candidate path is current. The physical-identity regressions
  remain implementation evidence for Gate 5.
- No command review is active. Gate 1 audits and registers, the contract
  migration ledger, and settled or rejected reviews are archived historical
  evidence only. Queue 33's completion rejection and the full retained-command
  delivery boundary remain current program decisions.

## Gate 4 outcome and validation

Gate 4 is closed. It leaves one accepted Crystallized source set for the
replacement CLI. The CLI Architecture, consolidated Command Contract Set,
Shared CLI Operation Contract, and `contracts/**` are current command
authority. Scoped CLI Directives and Patterns govern applicable work. Working
contains only mutable program state, while archived audits, migration records,
and reviews remain historical.

The final validation evidence is:

- Fresh targeted semantic and writing review findings were corrected, and
  focused rereviews passed.
- The targeted legacy `open-forge-old index` check succeeded during temporary
  compatibility staging; the final paths were restored afterward.
- `open-forge-old load --bodies` succeeded during staging.
- The final legacy Doctor remains unable to traverse the final `_index.md` and
  `_references.md` physical-identity compatibility names. This is the preserved
  new-CLI regression and is not claimed clean.
- Custom final validation covered 142 scoped Markdown files, 2,276 local links,
  and 277 anchor references with zero broken.
- Candidate paths: 0. Temporary Find/Index IDs: 0. Old Working contract paths: 0. Current old lifecycle claims: 0. Bad Crystallized frontmatter tags: 0.
- Authored current files were formatted, generated `Entries` were regenerated,
  `git diff --check` was clean, and sealed earlier handoffs were unchanged.

## Next actions

1. The maintainer reviews the [sealed Gate 4 wake-up summary](../handoffs/2026-08-17_cli-release-gate-4-complete.md)
   and decides whether to authorize Gate 5. Until then, do not implement,
   package, publish, or claim the replacement is shipping.
2. If Gate 5 is authorized, begin with the foundation, exact tools, and a real
   six-RID Native AOT spike. Then implement all retained commands in dependency
   order, create thin npm packages, and finish CI and main-only release proof.
   Return to Architecture on a material failure of an accepted assumption.

## Current authorities and record responsibilities

The accepted current sources are Crystallized. The [CLI Architecture](../../crystallized/documents/cli/architecture.md), the consolidated
[Command Contract Set](../../crystallized/documents/cli/command-contract-set.md),
the [Shared CLI Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md), and the detailed contracts under
[`contracts/`](../../crystallized/documents/cli/contracts/_contracts.md) define
the replacement's current meaning. The Command Contract Set is the concise
`#Evergreen` overview of command-contract roles, topology, and authority
boundaries, not a Pattern or a replacement for detailed contracts. The scoped
[CLI Directives](../../../directives/open-forge/cli/_cli.md) and [CLI Patterns](../../../patterns/open-forge/cli/_cli.md)
govern their applicable work without replacing those command authorities.

The Command Contract Set links to the detailed contracts under
[`contracts/`](../../crystallized/documents/cli/contracts/_contracts.md).
The Shared CLI Operation Contract defines cross-command conventions and does not
replace command-local contracts. Mutable Working retains only this program
route, the Decision Agenda, the Release Plan, and the Checkpoint. Sealed handoffs
are transfer snapshots, not command authority. Archived audits, migration
records, and reviews are historical evidence only.

The remaining program records have narrower roles:

- `release-plan.md` records gates, tasks, dependencies, and validation.
- `decision-agenda.md` records accepted direction and explicit deferrals.
- The [archived migration ledger](../../archived/cli-release/contract-migration-ledger.md) records completed lossless
  migration evidence.
- `../checkpoints/cli-release.md` records concise resumption state.
- Archived Gate 1 records and review files preserve history and evidence. They
  never replace the current Architecture, Command Contract Set, or contracts.

Guidance and Templates were updated and reconciled with the accepted source set.
Templates remain optional copy-ready starters. D018 in the Decision Agenda keeps
completion rejected; create a separate Decision later only when rationale is
worth preserving independently.

## Boundaries

- Do not describe the new CLI as shipped or Gate 5 as accepted or started.
- Do not reopen accepted Architecture as a design question. Gate 5 must supply
  the executable evidence named by the Architecture after maintainer
  authorization.
- Keep `src/cli-mvp/`, its tests, build, and verification frozen and outside
  active implementation work.

## Entries

<!-- open-forge:generated-index:start -->

- [Accepted CLI direction, Gate 3 Architecture decisions, and Gate 5 evidence boundary](decision-agenda.md) - #Memory #Working #CLI #Release #Gate #Decision #Discussion #Contextual
- [Current gates, tasks, dependencies, validation, and stop states for the new CLI release program](release-plan.md) - #Memory #Working #CLI #Release #Program #Gate #Plan #Contextual

<!-- open-forge:generated-index:end -->
