---
open-forge:
  description: Completed redesign of benchmark seeds and harness into compact reusable dual-review scenarios
  tags: [Memory, Session, Archived, Contextual, Historical, Benchmark]
---

# Benchmark Redesign Session

Date: 2026-07-21.

## Goal

Replace the stale seed and P0 evidence system with a small dogfood loop in which an orchestrator runs a worker through a frozen scenario, the worker reviews its own work, and the orchestrator independently reviews behavior and actual outcome before comparing the accounts.

## Completed Result

- Replaced the former forensic runner with `list`, `prepare`, and `finish` over conventional scenario folders and explicit string ids.
- Replaced benchmark-owned directives, workflows, overlays, schemas, fixed ratings, eligibility machinery, and seed packages with five compact scenarios using current Core and actual bundled extensions.
- Added explicit coverage for routing, Memory, workspace-wide directives, patterns, guidance, Workspace routes, conflict escalation, skills, and workflows. Narrower scoped directive inheritance remains explicitly uncovered because no current extension supplies it.
- Restricted scenario payloads to task/product context, including Memory and Workspace orientation, while rejecting substitute Open Forge owners, nested `AGENTS.md`, and scenario-owned `Axioms`.
- Froze the standard orchestrator prompt for every run and added an optional external orchestrator addendum for run-specific observations, checks, and records without changing worker-visible inputs.
- Kept `doctor` and `find --follow-required` as paired complete-workspace validators at preparation and finish.
- Preserved worker self-review verbatim and required the orchestrator to form Behavior, Outcome, and Limits independently before comparison.
- Retained earlier reports as explicitly historical raw context and retired obsolete active benchmark surfaces.

## Dogfood

- The planning-conflict run found incompatible accepted truth, paused for the owner, reconciled the losing record, stayed planning-only, and disclosed unsupported implementation assumptions in self-review. Its original prompt and missing product source exposed two scenario biases, which were removed from the checked-in version.
- The broad-context recall run used a frozen external orchestrator addendum, correctly treated `rune-bridge` as guidance rather than proof of Rune availability, returned every accepted archive-export fact, disclosed exact-search limits, and left the workspace unchanged.
- In both runs, the worker and independent outcome accounts materially agreed. Exact worker route reads and tool sequence remained unknown because the runtime exposed no independent low-level trace.

## Verification

- Benchmark runner tests: 9 passed.
- Full fast suite: 22 passed.
- Full closure suite: 146 passed.
- `open-forge doctor`: clean.
- `open-forge find --follow-required --json`: clean.
- Every checked-in scenario prepared through the real CLI with current Core/extensions and both complete-workspace validators.

The accepted ongoing contract is [Benchmark Design](../../crystallized/decisions/benchmark-design.md).
