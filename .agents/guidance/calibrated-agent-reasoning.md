---
open-forge:
  description: Calibrate delegated reasoning, ownership continuity, review depth, and parallelism to bounded work and concrete risk
  tags: [Core, Guidance, Collaboration, Orchestration, Delegation, Reasoning, Review, Risk, Efficiency]
---

# Calibrated Agent Reasoning

## Scenario

Use this Guidance when delegated work can run at different reasoning levels and the primary owner must balance confidence, wall-clock time, context, and token cost.

## Preferred Approach

Bound the work before choosing reasoning. State the outcome, accepted meaning, authority, expected paths, protected paths, direct integration neighborhood, non-goals, evidence, return shape, and stop conditions.

| Default  | Use when                                                                                                                                                                     |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `medium` | Exact operations, mechanical inspection, state maintenance, formatting, or a tightly specified transformation with local failure effects.                                    |
| `high`   | Bounded exploration, external research, ordinary advice, substantial prose, focused review, or implementation decisions inside a well-established pattern.                   |
| `xhigh`  | Cross-surface architecture, difficult contract interpretation, broad integration review, or a bounded audit whose value depends on finding interactions.                     |
| `max`    | One coherent implementation or evidence-authoring owner faces difficult behavior, expensive rework, or many interacting local decisions inside an already accepted boundary. |

Raise or lower the level only for concrete complexity, risk, reversibility, evidence cost, or latency. Maximum reasoning is most valuable when one owner can carry a hard slice to completion. It is usually wasteful when multiplied across explorers, councils, routine reviewers, mechanical operators, or repeated confirmation passes.

## Ownership And Context

- Keep one implementation owner through tests, production, local refactoring, and correction whenever the boundaries allow it.
- Use a separate context for independent evidence, architecture compaction, or review only when isolation adds value greater than packet and reintegration cost.
- Do not ask a helper to rediscover accepted architecture or history. Provide a compact execution capsule and link the decisive sources.
- Expected paths are a forecast. Protected paths are hard boundaries. Directly required neighboring paths may be added only inside accepted meaning and must be reported.
- Require compact returns. Raw searches, full logs, and long transcripts should remain in artifacts.

## Parallelism And Review

- Parallelize non-overlapping actions or independent read-only perspectives with a named integration point.
- Do not parallelize several agents over the same mutable responsibility unless comparison is the explicit deliverable.
- Use one independent review by default only when a named risk justifies it. Add a second reviewer only for a distinct risk or genuinely independent provider perspective.
- Give findings stable identifiers, group corrections, and recheck only changed findings and their neighborhood.
- Measure review value by accepted blocking or material findings, prevented rework, and decision impact, not by finding count.

## Tradeoffs

Higher reasoning can improve difficult cross-surface work, but it costs time and can overvalue speculative concerns. Lower reasoning is efficient for closed work, but it can miss interactions when the packet is incomplete. Better boundaries and ownership continuity often save more than changing reasoning level.
