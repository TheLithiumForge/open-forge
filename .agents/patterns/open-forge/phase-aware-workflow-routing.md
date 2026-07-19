---
open-forge:
  description: Infer the current development phase and recommend the closest workflow plus an earlier prerequisite only when current truth is insufficient
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
  -> use phase tags to orient the candidate scan
  -> select the workflow whose Goal best covers the transition
  -> detect a material missing prerequisite
  -> when warranted, recommend the earlier workflow first
  -> when no Goal matches exactly, offer closest installed route(s) or direct execution
  -> otherwise begin at the current phase
```

Phases are wayfinding, not a waterfall. Work may start in any phase, loop within one, move backward when evidence changes the understanding, or hand off forward when the current Goal is accepted.

An explicit workflow choice wins. An explicit no-workflow or direct-execution request proceeds without another prompt.

## Review Checks

- Every workflow recipe declares exactly one stable phase tag.
- Phase narrows attention; the selected workflow Goal, request, and routed state justify the recommendation.
- An earlier workflow is suggested only for a concrete missing prerequisite, never as mandatory ceremony.
- One workflow remains primary; additional workflows are ordered handoffs.
- No exact match presents the established closest-route/direct-execution choice, while an explicit opt-out is honored immediately.
