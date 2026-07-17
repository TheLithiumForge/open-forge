---
open-forge:
  description: Active transfer note for dependency-safe extensions and reproducible benchmark harness hardening
  tags: [Memory, Handoff, AgentCommunication, Contextual, Extension, CLI, Benchmark, Reliability]
---

# Handoff: Harness And Extensions

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
