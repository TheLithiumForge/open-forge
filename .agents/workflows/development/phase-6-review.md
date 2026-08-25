---
open-forge:
  description: Provide one bounded independent review and route material findings by stable ID to the earliest invalidated boundary
  tags: [Workflow, Development, Phase, Review, Quality, Evidence]
---

# Phase 6 - Whole-Task Review

## Goal

Give the primary owner one independent read-only assessment of the accepted outcome, changed artifacts, integration, and evidence before final acceptance.

## Steps

1. Supply a clean packet with accepted outcome, invariants, architecture and placement map, baseline, exact changed and untracked artifacts, direct integration neighborhood, claimed evidence, public scenario, and full-gate result. Withhold prior reviewer conclusions during the independent first pass.
2. Review correctness, contracts, safety, error behavior, source and test locality, integration, maintainability, and evidence credibility at the assigned horizon.
3. Inspect or reproduce only read-only evidence that materially tests a concern. Do not fix findings.
4. Give every blocking or material finding a stable ID, severity, category, exact location, evidence, consequence, smallest correction, and earliest invalidated boundary.
5. Separate residual risk, optional material improvements, and preference-only observations.
6. The primary owner groups accepted findings into one correction packet. Recheck only changed finding IDs and their affected neighborhood unless the correction invalidates the whole task.
7. Add a second reviewer only when one named distinct risk remains outside this review horizon.

## Completion

- The review was independent, bounded, read-only, and evidence-backed.
- Material findings have stable identity and an explicit correction route.
- Preference and speculative alternatives do not trigger a correction cycle.
- The primary owner can accept, perform one grouped correction, or return one exact unresolved decision.
