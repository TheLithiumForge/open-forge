---
open-forge:
  description: Reviewed CLI flows and scenarios, their selection decisions, and the boundary before new tests
  tags: [Memory, Document, CLI, Experience, Evergreen]
---

# CLI Experience

## Scope And Acceptance

The maintainer requested a forgiving, helpful CLI on 2026-09-19 and delegated assessment and preservation of useful scenarios. This collection records that reviewed selection. The flow list and detailed target changes remain subject to maintainer validation before new tests are implemented.

A selected scenario is an intended user outcome, not evidence that the CLI passes it. Current [command contracts](../contracts/_contracts.md) remain the source for the existing command interface and precise output. Where a target differs, the scenario marks it explicitly. This collection does not import the supplied pack's proposed contracts, message catalogue, or generated renderer.

## Experience Direction

Continue useful work when its required inputs and intended effects are known. Missing optional frontmatter should allow indexing with a warning and preserved authored bytes. Prefer clear no-ops for already absent requested items. Keep unrelated damage from blocking independent work. Preserve ambiguous or user-changed content rather than guessing destructive actions. Report partial results honestly and explain the shortest useful next action.

Write expectations before observing a run. Check actual state independently of the output. Separate outcome, preservation, and communication verdicts; warning exit codes do not by themselves prove a failure.

## Sources

Adapted from the maintainer-supplied Open Forge experience v2 pack: 24 flows and 438 scenario cases. Its own README states that all cases were unrun proposals. This repository's assessment replaces any blanket adoption of that pack. Stable source IDs remain available for traceability.

Actual observations belong to the separate [2026-09-19 run record](../../../../archived/beta-preparation/cli-experience-run.md). That record does not redefine expected behavior.

## Entries

- [Connected CLI user flows selected for validation before automated tests](flows/_flows.md) - #Memory #Document #CLI #UserFlow
- [Reviewed scenario collections with explicit selection and target changes](scenarios/_scenarios.md) - #Memory #Document #CLI #Scenario
- [Disposition of every supplied flow and scenario](assessment.md) - #Memory #Document #CLI #Review
- [Portable fixture recipes for reproducing the selected user outcomes](fixtures.md) - #Memory #Document #CLI #Fixture
- [Compact flow list for validation before new tests](flow-review.md) - #Memory #Document #CLI #Review
