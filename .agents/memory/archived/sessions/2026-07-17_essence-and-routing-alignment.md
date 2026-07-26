---
open-forge:
  description: Completed alignment of route-scoped directives, phase-aware workflows, extension lifecycle, binding continuity, archives, and tiered tests
  tags: [Memory, Session, Contextual, Historical, Archived, Framework, Directive, Workflow, Extension, Testing]
---

# Essence And Routing Alignment Session

Status: complete. Final phase: verification. Closed: 2026-07-18.

## Goal

Restore one top-to-bottom routed decision model while keeping Open Forge minimal, flexible, memory-backed, human-governed, and honest about increasing the odds of good non-deterministic agent behavior rather than guaranteeing it.

## User-Stated Direction

- Directive applicability belongs in routing; once a directive file is loaded it should bind without an internal `Applies To` decision.
- Workflow selection should infer the current development phase, recommend the closest workflow, and suggest an earlier prerequisite only when warranted.
- Extensions need a removable, extension-owned way to augment shared axioms or other sections without unsafe shared-file collisions.
- #KeepInMind remains baseline-important and binding, must be complete through the CLI, and must support long-running sessions and context restoration rather than closeout only.
- Superseded `.agents` material and other non-current docs should move to scoped archives so first review starts from clean current truth.
- Tests should share one cwd-independent utility layer, keep fast development feedback as the default, and reserve subprocess, Git, and OS-temporary-directory coverage for explicit closure/CI runs.
- Test value should come from behavior, invariants, safety, ownership, and rollback evidence; incidental file-existence snapshots should not make ordinary framework evolution expensive.

## Established Before Implementation

- Binding directives at the time included the former Framework Essence directive and the current [Deliberate Framework Change directive](../../../directives/deliberate-framework-change.md). Identity-level meaning from the former directive now belongs to [Open Forge Principles](../../crystallized/documents/principles.md).
- Patterns: route-scoped directives, phase-aware workflow routing, and continuity checkpoints under `.agents/patterns/open-forge/`.

## Accepted Implementation Direction

- Shared-file extension behavior uses explicit owned augmentation slots inside already-loaded files. Extension blocks are deterministic and removable; workspace `.overwrite.md` companions remain the final local-precedence layer.
- A transparent Git-visible `open-forge.extensions.json` receipt records CLI ownership, hashes, dependency roots, and augmentation blocks for safe update/removal. It is CLI state, not agent runtime truth; materialized Markdown remains complete agent truth.
- Removal currently acts only on explicitly named installed ids and blocks retained reverse dependents. Automatic orphan pruning remains deliberately unimplemented until an explicit previewable policy can distinguish roots and legacy/local dependency packs safely.
- Workflow phases are metadata tags on existing routes, not physical phase folders. They orient selection without adding route depth or imposing a waterfall.
- Scoped archives separate completed handoffs, sessions, analysis, observations, planning, and historical run plans after current truth is extracted and links are repaired.
- Tests use one cwd-independent utility boundary and explicit `*.unit.test.ts` / `*.closure.test.ts` tiers. Fast tests are the default; OS temporary directories, Git, subprocesses, packaging, and benchmark lifecycles are closure/CI evidence.
- Local sources that declare persistent dependencies or augmentations require a stable id. Core and extension plans preserve receipt-owned files and retained blocks, reject manager collisions and linked roots/catalogue packages, and validate cross-platform paths and managed marker ownership.
- The first-party Vision loader augmentation was removed because its rule duplicated Goal-based workflow routing; explicit augmentation remains available and is proven through synthetic lifecycle and packaged-layout fixtures without adding unwarranted baseline context.

## Current Evidence

- Route-scoped directive source, dogfood, payloads, doctor validation, and governing docs are in migration.
- All first-party workflow recipes and harness worker workflows have primary phase tags; doctor validation is in migration.
- Loader and routing governance now treat the complete routed #KeepInMind catalogue as binding continuity context.
- Scoped archival is complete: completed handoffs and sessions, dated analyses, applied ideas, resolved observations, planning snapshots, and frozen gen10/gen11 run plans moved to explicit archives after open work was extracted; repaired links resolve, run-plan bytes match their original Git blobs, and `open-forge doctor .` reports no problems.
- Test architecture is implemented: the final fast tier passes 25/25 from the repository root and outside its cwd; final closure passes 162/162. All filesystem/process suites use `tests/support/index.ts`, and no private temp/spawn helper remains.
- Lifecycle regressions cover stale file/block reconciliation, shared-owner consent and release, unowned-path rejection, strict receipt integrity, unrelated modified-block refusal, cross-extension target-delete refusal, unmanaged/managed collisions, retained-slot protection, linked removal/catalogue roots, Core receipt safety, fragment exclusion, portable paths, marker ownership, stable dependency identity, and byte-for-byte rollback at three injected transaction stages.
- Final lifecycle review added and verified authored-versus-generated receipt digests, standalone index and Core regeneration safety, retained route-host protection, self-validating portable receipt paths, and literal-backslash rejection.
- Final `bun run test:ci` passed 187 tests with 0 failures and 1,192 assertions. Build and npm package dry-run passed; dogfood and installable Core doctors report zero findings; staged and unstaged patch-integrity checks pass.
- Workflow-first behavioral scenarios are structurally prepared and remain explicitly **UNRUN** against models; no behavioral-compliance claim is promoted from preparation.
- No continuation work has been staged; the pre-existing staged baseline is preserved.

## Closeout

No new observation was warranted: verified lifecycle and testing findings were incorporated directly into current decisions, directives, patterns, documentation, implementation, and tests. Naming remains an existing emerging observation rather than accepted truth.
