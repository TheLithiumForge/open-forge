---
open-forge:
  description: Reproduce and isolate a defect, test competing hypotheses efficiently, and verify an authorized cause-level fix
  tags: [Workflow, Quality, Debugging, Efficiency]
---

# Debugging

## Goal

Connect an observed symptom to a reproducible root cause and, when authorized, a minimal verified fix without multiplying owners or masking evidence.

## Steps

1. Capture the symptom, expected behavior, environment, inputs, frequency, recent changes, and external-effect boundaries without altering failing evidence.
2. Reproduce the failure with the smallest reliable case, or state why reproduction is unavailable.
3. Keep one investigation owner responsible for the hypothesis set and root-cause synthesis.
4. Trace the failing path and identify the earliest boundary where actual behavior diverges from expected behavior.
5. Form competing hypotheses and run the cheapest discriminating checks. Parallelize only independent read-only checks with compact returns.
6. Identify the root cause, contributing conditions, affected scope, and why safeguards missed it.
7. When changes are authorized, add regression evidence and apply the smallest cause-level correction. Do not weaken a valid expectation or mask the symptom.
8. Rerun reproduction, regression evidence, direct integration checks, and broader gates justified by risk.
9. Use an independent review only for safety-critical, cross-cutting, or persistently unexplained defects.

If evidence cannot establish the cause, return an inconclusive diagnosis with the checks performed, remaining hypotheses, and next useful check. Investigation may stop there; a cause or fix must not be reported as established.

## Completion

- The result distinguishes a verified diagnosis or fix from an inconclusive investigation.
- Rejected hypotheses and decisive evidence are explicit.
- Any fix addresses the cause and passes regression evidence.
- Remaining uncertainty and the next discriminating check are visible.
