---
open-forge:
  description: Planned Task 30 follow-through for interaction, content taxonomy, and scenario evidence
  tags: [Memory, Working, CLI, Task, Subtask, Contextual]
---

# Task 30 — Phases 5–8

## Status

**Mostly absorbed. Only phase 6 remains distinct, and it is narrower than it
looks.** This file was always a planning packet that pointed elsewhere; the
things it pointed at have since been done or given their own owners.

## What this packet still owns, measured 2026-09-17

| Its phase | Where it went |
| --- | --- |
| **5** — interaction, help and error paths | Delivered by G4's [04 interaction system](../../../../archived/cli-development/tasks/task30-g4/04-interaction-system.md) and the accepted C1–C18 decisions. Its error-path half continues in [phase 5-D](phase-5-diagnosis-and-interoperability.md). |
| **6** — content and taxonomy ownership | **Still open. The only distinct remainder.** |
| **7** — scenario coverage | Owned by [phase 7 scenarios](phase-7-scenarios.md), now specified. |
| **7/8** — test project architecture and parallel execution | [Complete](phase-7-8-test-architecture.md). |

### Phase 6 — what is actually left

Shared wording is **already largely single-sourced**: 52 shared message families
are recorded in the G4 conventions, `CliFindingWording` holds 60 shared members,
and 32 per-command wording files hold the command-local remainder. So the "one
source" half is done in code.

What is not established is **document ownership**: which record owns a given
statement, and whether dependent documents link to it rather than restating it.
The candidates that can disagree are the G4 conventions, the 28 command
catalogues, the public contracts, and `docs/`.

Before planning work here, measure whether any statement is genuinely duplicated
across those surfaces and drifting. **If nothing is drifting, the honest outcome
is to record that phase 6 is satisfied and close it** — this packet should not
manufacture a restructure to justify its own existence.

Note the overlap: [Task 37](../task37-wording-review-against-proposals.md) owns
sentence-level wording decisions, and G4's
[41 documentation propagation](../../../../archived/cli-development/tasks/task30-g4/41-documentation-propagation.md)
already propagated the accepted changes across 120 files. Phase 6 owns only the
question of who owns what, not the wording itself.

## Evidence source

- [Interaction layer](../../../../emerging/analysis/cli-experience-audit/interaction-layer.md)
- [Taxonomy and adoption](../../../../emerging/analysis/cli-experience-audit/taxonomy-and-adoption.md)
- [Test strategy and scenarios](../../../../emerging/analysis/cli-experience-audit/test-strategy-and-scenarios.md)
- [Layers and sequencing](../../../../emerging/analysis/cli-experience-audit/layers-and-sequencing.md)

## Actionable packet

- Phase 5 should make guided interaction, help, and error paths answer the
  user's next question without changing command semantics silently.
- The diagnosis and interoperability boundary is now explicit in [Phase 5
  diagnosis and interoperability](phase-5-diagnosis-and-interoperability.md).
- Phase 6 should assign content and taxonomy decisions to the narrowest owner;
  shared wording belongs in one source and dependent documents link to it.
- Phase 7 should define executable scenario coverage at the module and process
  boundaries after the view contract is stable.
- The in-process journey, model-snapshot, and coverage packet is now explicit
  in [Phase 7 scenarios](phase-7-scenarios.md); the linked test-architecture
  subtask remains the owner of project shape, usefulness dispositions, and
  parallel qualification.
- Phase 7's folder and test-layer work is defined in [Test Project Architecture
  And Parallel Execution](phase-7-8-test-architecture.md): isolate every E2E
  test under an OS-temp child, review every test for usefulness, and qualify
  parallel execution before changing the delivery default.
- Any future folder or project restructure remains a separate accepted boundary,
  not a mechanical consequence of the audit. Its project graph, layer map, and
  test dispositions must be accepted through the linked subtask first.

## Update rule

This subtask is a planning packet only. When implementation starts, replace
recommendations with accepted requirements and put discoveries in the active
Task. Do not rewrite the Emerging evidence sources to follow implementation
drift.
