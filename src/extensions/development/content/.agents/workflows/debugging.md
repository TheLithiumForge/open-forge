---
open-forge:
  description: Reproduce and isolate a defect, identify its root cause, and verify an authorized minimal fix
  tags: [Extension, Workflow, Quality, Debugging]
---

# Debugging

## Goal

Connect an observed symptom to a reproducible root cause and, when changes are authorized, a minimal verified fix.

## Steps

1. Load the applicable project scopes and identify the source that defines expected behavior. Capture the symptom, expectation, environment, inputs, frequency, and recent relevant changes without altering the failing evidence. If the expectation is disputed, keep that disagreement visible while investigating.
2. Use the project's available tools and verification procedures to reproduce the failure with the smallest reliable case, or explain why reproduction is unavailable.
3. Trace the failing path and observe boundaries where actual behavior diverges from expected behavior.
4. Form competing hypotheses and run the cheapest check that distinguishes among them. Do not use speculative product changes as the primary investigation method.
5. Identify the root cause and explain how it produces the symptom, which conditions contribute, and why existing safeguards missed it.
6. If changes are authorized, define regression evidence and apply the smallest correction that addresses the cause. Do not mask the symptom or weaken a valid expectation.
7. Rerun the reproduction, the checks that provide regression evidence, and nearby checks justified by the risk.

If evidence cannot establish the cause, return an inconclusive diagnosis with the checks performed, remaining hypotheses, and next useful check. Investigation may stop there. A cause or fix must not be reported as established.

## Completion

- The result distinguishes a verified diagnosis or fix from an inconclusive investigation.
- Conclusions are tied to observed evidence and rejected hypotheses.
- Files or behavior changed only when authorized.
- Any fix addresses the cause and passes regression evidence.
- Remaining uncertainty and the next discriminating check are explicit.
