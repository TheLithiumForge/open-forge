---
open-forge:
  description: Historical transfer note for completed extension, workflow-routing, installation, and harness hardening passes
  tags: [Memory, Archived, Handoff, AgentCommunication, Contextual, Historical, Extension, CLI, Benchmark, Reliability]
---

# Handoff: Harness And Extensions

Status: archived 2026-07-18.  
Original route: `.agents/memory/working/handoffs/2026-07-15_harness-and-extensions.md`.  
Archived because: the delivered passes completed and remaining gates were extracted into maintained decisions and the live backlog.  
Current owner or replacement: `docs/cli.md`, `docs/extensions.md`, `benchmarks/harness/README.md`, crystallized extension decisions, and `.agents/memory/working/backlog.md`.

Date: 2026-07-15. Status: completed.

## Goal

Improve the optional extension system and developer benchmark harness from the accepted routed ideas while keeping Core small, Markdown-first, runtime-agnostic, and on demand. Do not adopt a lab or category name; CRD remains only a candidate under evaluation.

## Change Boundary

- All user-owned changes present at task start were staged before implementation.
- Every change produced by this task must remain unstaged.
- Do not commit or stage task work.

## Delivered

1. `extend` now resolves offline bundled dependencies, validates strict manifests, plans portable path identities and target topology before writing, previews create/update/unchanged and scope effects, preserves local Markdown blocks, and rolls payload plus generated indexes back on process-level failure.
2. Eight bounded first-party packs now cover shared implementation capability, workflow essentials, development, planning, quality, design, reliability defaults, and an optional route-only Rune bridge.
3. The developer-only harness now provides atomic external prepare/finalize/validate/render flows, authenticated prepared/final evidence roots, exact CLI/Git/input provenance, prepared-workspace drift checks, external evaluation/trace boundaries, immutable finalization, full regular-file final snapshots, canonical results, and deterministic reports.
4. Operator docs distinguish engineering evidence from causal claims, identify the two external trust inputs, document the worker-tool provisioning boundary and snapshot disclosure risk, and treat prior reports as raw legacy engineering evidence.

## Constraints

- Installed routed files remain runtime truth; extension manifests are install-time metadata only.
- Dependencies resolve only from explicitly available bundled packages and never fetch from a network.
- Rune remains optional and one-directional; no private command or configuration contract is invented.
- The benchmark runner prepares and captures evidence around a worker; it does not become an agent runtime or scheduler.
- Existing reports are legacy engineering evidence, not causal proof.

## Verification

- Full `bun test`: 98 tests and 507 assertions passed. `bun run build` passed.
- The built Node CLI advertised all eight packs, resolved the development dependency closure, installed all packs over Core, traversed Required Routes, and returned `doctor` with zero errors and zero warnings.
- Six new or moved native skills passed the skill package validator.
- A real external prepare/prepared-validate/finalize/final-validate/render cycle passed with authenticated validation, a canonical path-bound claim, an exact harness-producer snapshot, a final-workspace snapshot, deterministic rendering, engineering eligibility true, and causal/public eligibility false. Six generated/example documents also passed the five registered Draft 2020-12 schemas.
- Five JSON schemas parsed, source-workspace `doctor` returned zero findings, and the #KeepInMind closeout route was read.
- The cached baseline remained 156 paths with binary diff hash `0e98326c9dec9b0fe3f68df30844fe4d146cc89d`; task work remained unstaged.

## Residual Boundaries

- Extensions remain install-only: there is no persistent ownership/lock state, version solver, update/remove/migration lifecycle, crash journal, or generated-index body diff in dry-run.
- Harness P0 does not schedule or isolate a worker, independently provision worker tools, randomize experiments, enforce evidence-class-specific controls, verify causality/publication readiness, or perform corpus-level statistics.
- Authenticated validation intentionally requires the intact run workspace and `.git`, the owner token, the prepared-root digest, and a compatible Git/runtime environment. Final workspace snapshots can be large and can disclose any secrets the worker leaves behind.

## 2026-07-17 Follow-On

Status: completed.

### Delivered

- The universal catalogue now has 18 composable units: skill-only, workflow-only, directive-only, pattern-only, mixed, and dependency-only extensions all use one dependency graph and one full-catalogue selector with transitive required selections. Native and APM-installed skills remain ordinary additive packages under `.agents/skills/`; Open Forge does not replace their bytes or require a private wrapper.
- Workflow recipes now put Mode first, use only linear or iterative modes, always declare Goal and Constraints, and distinguish organizational workflow categories from complete recipes. Directive files declare explicit Applies To scope; hybrid category inheritance remains the narrow exception. `chain` explains inherited context for any heading.
- The loader defaults non-trivial work to a matching workflow, offers the nearest route or ordered handoffs when no exact match exists, and honors explicit direct execution. Its CLI section covers deterministic find, chain, doctor, index, and catalogue operations. KeepInMind search is correctly documented as global discovery, not an active-context receipt.
- Normal installation is a reviewable Git lifecycle: base payload first, review and commit, then one extension dependency closure per review unit. Recognizable tracked Core anchors, target-scoped cleanliness, Git-visible outputs, portable aliases, Git-control paths, links, hard links, collisions, source separation, index preflight, and in-process rollback are enforced. `--pro` bypasses lifecycle gates only.
- Architecture and vision capabilities/workflows now support greenfield and rescue use, elicit missing intent, and derive warranted directives, patterns, guidance, workspace routes, and memory with provenance and user accord; unaccepted inference stays emerging/proposed.
- A reusable CLI-testing pattern requires OS-temporary folders, real subprocess CLI/Git commands, process/filesystem/Git/no-mutation assertions, and cleanup. Five workflow-first behavior scenarios are frozen and explicitly UNRUN; their real-CLI preparation test proves composition only and never fabricates agent-compliance evidence.

### Verification

- `bun test`: 163 passed, 0 failed, 1,155 assertions across CLI, all 18 first-party extensions, packaged layouts, and the benchmark harness.
- Focused CLI suite: 102 passed, 0 failed, 612 assertions. Focused harness suite: 31 passed, 0 failed, 194 assertions.
- `bun run build` passed. `npm pack --dry-run --json` reported 116 package entries and included every first-party extension payload.
- Six first-party native skills passed the official skill-package validator.
- Source-workspace and packaged-base `doctor --json` each reported 0 errors and 0 warnings after index regeneration.

### Git Boundary

- The 156-path user baseline recorded at task start was committed externally during this work as `d6c4922` (`minimalizing losslesly our docs`) and `20d1f5f` (`improved extensions`). The index is now empty.
- All 2026-07-17 follow-on work remains unstaged as requested. Do not reconstruct or restage the former baseline.

### Remaining S++ Gates

- No persistent extension ownership lock/update/remove/migration lifecycle or crash-recovery journal exists yet.
- No deterministic active-context receipt exists yet; KeepInMind closeout still requires comparing global discovery with the actually loaded chain.
- The new workflow-first scenarios are evidence-ready but UNRUN. S++ behavior claims still need crossed model/runtime trials, repeated runs, independent reproduction, and claim-scoped promotion gates.
