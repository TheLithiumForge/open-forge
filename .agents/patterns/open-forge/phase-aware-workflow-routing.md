---
open-forge:
  description: Select workflows from visible routing signals and recommend helpful prior work once without blocking progress
  tags: [Pattern, Workflow, Routing, DevelopmentPhase, Minimalism]
---

# Phase-Aware Workflow Routing

## Shape

Use five ordered but non-linear phases:

1. `discovery` - the problem, opportunity, or options are still unclear.
2. `definition` - intent is becoming an accepted vision, architecture, experience, or boundary.
3. `planning` - an accepted direction is being decomposed into executable work.
4. `delivery` - the system or artifact is being created, changed, repaired, or improved.
5. `verification` - claims, behavior, quality, or readiness are being tested or reviewed.

At workflow selection:

```text
user request + routed current truth
  -> infer current development state and requested transition
  -> use generated descriptions and phase tags to orient the candidate scan
  -> select a workflow before opening its body
  -> confirm that its Goal covers the transition
  -> read any optional Goal `- helpful before: ...` item
  -> when useful and available, recommend one matching earlier workflow once
  -> if it is skipped or unavailable, proceed with explicit assumptions
  -> when no workflow matches exactly, offer the closest installed route or direct execution
  -> otherwise begin at the current phase
```

Phases are wayfinding, not a waterfall. Work may start in any phase, loop within one, move backward when evidence changes the understanding, or hand off forward when the current Goal is accepted.

An explicit workflow choice wins. An explicit no-workflow or direct-execution request proceeds without another prompt.

## Review Checks

- Every workflow recipe declares exactly one stable phase tag.
- Phase narrows attention; the selected workflow Goal, request, and routed state justify the recommendation.
- Helpful prior work may prompt one available earlier-workflow recommendation, but it never blocks the selected workflow.
- When prior work is skipped or unavailable, the selected workflow states its assumptions and proceeds.
- One workflow remains primary; additional workflows are ordered handoffs.
- No exact match presents the established closest-route/direct-execution choice, while an explicit opt-out is honored immediately.
