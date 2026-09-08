---
open-forge:
  description: Reproduce and isolate a defect, identify its root cause, and verify an authorized minimal fix
  tags: [Extension, Workflow, Quality, Debugging]
---

# Debugging

## Goal

Connect an observed symptom to a reproducible root cause and, when changes are authorized, a minimal verified fix.

## Steps

1. Capture the symptom, expected behavior, environment, inputs, frequency, and recent relevant changes without altering the failing evidence.
2. Reproduce the failure with the smallest reliable case, or explain why reproduction is unavailable.
3. Trace the failing path and observe boundaries where actual behavior diverges from expected behavior.
4. Form competing hypotheses and test the cheapest check that distinguishes among them. Do not use speculative product changes as the primary investigation method.
5. Identify the root cause and explain how it produces the symptom, which conditions contribute, and why existing safeguards missed it.
6. If changes are authorized, define regression evidence and apply the smallest cause-level correction. Do not mask the symptom or weaken a valid expectation.
7. Rerun the reproduction, regression evidence, and nearby checks justified by the risk.

## Completion

- The symptom, trigger, contributing conditions, and root cause are distinguished.
- Conclusions are tied to observed evidence and rejected hypotheses.
- Files or behavior changed only when authorized.
- Any fix addresses the cause and passes regression evidence.
- Remaining uncertainty and the next discriminating check are explicit.
