---
open-forge:
  description: Completed benchmark composition redesign into agnostic scenarios, atomic primitives, exact meta-scenarios, and trace-reviewed run sets
  tags: [Memory, Session, Archived, Contextual, Historical, Benchmark, Dogfood]
---

# Benchmark Composition Redesign Session

Date: 2026-07-21.

## Goal

Turn the compact scenario harness into reusable building blocks: framework-agnostic tasks, selectable primitive treatments, exact stable recipes, reproducible on-demand additions, and direct orchestrator prompts for model and treatment comparisons.

## Completed Result

- Added five pure scenarios, eight atomic primitive blocks, and six exact meta-scenarios under a grouped hierarchy whose manifests own string identity.
- Kept each stable base or trap as its own complete recipe. The ledger control and trap share identical task bytes; the trap adds one independently coherent but contradictory Memory block.
- Split review ownership: scenarios assess the task, primitives assess their own semantic effect, and meta-scenarios assess routing, relationships, traps, escalation, and handoffs.
- Removed prompt, persona, and review leaks that could bypass selected Memory, manufacture a control conflict, or directly command the directive and pattern under test.
- Refactored the runner to discover and compose current Core, actual extensions, pure scenario material, ordered primitives, and frozen external variants from plain manifests.
- Required a non-overlapping external runs root, collision-free and link-free inputs, a completed observable-trace manifest, dual reviews, and both complete-workspace validators.
- Added worker-prompt SHA-256 and baseline Git tree identity so equal model or replicate cells compare bytes rather than ids alone.
- Added direct orchestrator templates for one run, parallel models or replicates, stable base versus trap, and on-demand treatment versus control.
- Kept provider-native tools as honestly recorded runtime bindings while allowing local tool primitives to ship ordinary files plus an optional Workspace discovery route.
- Preserved the manual plain-file protocol as complete; the CLI remains an optional preparation and capture helper.

## Superseded Assumptions

- Extensions no longer belong to scenarios.
- Routed Memory and Workspace material no longer belongs to scenario payloads.
- Benchmark-authored primitives are allowed as explicit reusable treatments instead of being prohibited as simulated framework behavior.
- Worker-visible variation no longer requires rewriting a stable scenario; exact meta-scenarios and frozen additive variants own it.

## Verification

- Full fast suite: 24 passed.
- Full closure suite: 146 passed.
- Runner composition suite: 11 passed, including every checked-in meta-scenario, external variants, added extensions, control/trap identity, trace closeout, payload boundaries, and external run-root containment.
- All current benchmark Markdown links resolve; every direct template placeholder has a matching assignment; no malformed flat parent-list shapes remain.
- All scenario and primitive references resolve, and scenario files contain no Open Forge-specific vocabulary.
- `open-forge doctor --json`: 0 errors and 0 warnings.
- `open-forge find --follow-required --json`: clean.

The accepted ongoing contract is [Benchmark Design](../../crystallized/decisions/benchmark-design.md).
