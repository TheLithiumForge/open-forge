---
open-forge:
  description: Review a bounded change or design with stable finding IDs, evidence-backed consequences, and no automatic duplicate review
  tags: [Extension, Workflow, Quality, Review, Evidence, Efficiency]
---

# Review

## Goal

Produce prioritized actionable findings, or a clear no-findings result, without modifying the target or making conclusions stronger than the evidence.

## Steps

1. Establish the accepted outcome, review horizon, baseline, exact changed and untracked artifacts, protected surfaces, direct integration neighborhood, workspace rules, claimed evidence, maximum review budget, and already consumed review IDs. Stop before invocation when no unit remains.
2. Consume one review-budget unit with a stable invocation ID. For an independent first pass, withhold earlier reviewer conclusions. Inspect the target and only enough surrounding context to understand responsibilities and interactions.
3. Trace high-risk paths, contracts, consumers, failure modes, compatibility, source and test locality, durable-source transitions, and evidence gaps.
4. Run or inspect verification matched to a suspected issue and known to be read-only.
5. Challenge each candidate finding against existing safeguards and plausible false positives.
6. Give every blocking or material finding a stable ID, severity, category, location, evidence, consequence, smallest credible correction, and earliest invalidated boundary.
7. Separate residual risk, optional material improvements, preference, duplicate findings, and missing verification.
8. The owning context groups accepted findings into one correction packet and records accepted, rejected, duplicate, preference-only, and false-positive disposition when useful.
9. After correction, recheck changed finding IDs and affected context. Rerun the complete review only when the correction materially changes the whole artifact or review horizon.

## Completion

- Every material finding has stable identity, evidence, consequence, and correction guidance.
- False positives and existing safeguards were considered.
- The target remained unchanged.
- Verification gaps, residual risk, and uncertainty are explicit.
- Duplicate review and repeated rationale were avoided.
