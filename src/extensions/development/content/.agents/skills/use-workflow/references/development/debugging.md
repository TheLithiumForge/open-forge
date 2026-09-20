---
open-forge:
  description: "Reproduce a defect, distinguish causes, and verify an authorized correction"
  tags: [Extension, Workflow, Quality, Debugging]
---

# Debugging

## Goal

Connect an observed symptom to an evidenced cause and, when changes are authorized, a minimal verified fix. An inconclusive diagnosis is valid when the evidence does not establish the cause.

## Steps

1. **Capture the discrepancy.** Load applicable scopes and the source defining expected behavior. Preserve the symptom, inputs, relevant environment, frequency, and recent changes. Keep a disputed expectation visible.
2. **Reproduce reliably.** Use the project's available tools to find the smallest reliable failing case. Preserve the failing evidence. State any limit that prevents reproduction.
3. **Locate the divergence.** Trace the failing path and inspect boundaries where actual behavior first differs from the expectation.
4. **Distinguish hypotheses.** Form plausible competing explanations and run the cheapest permitted check that separates them. Do not use speculative product changes as the primary investigation method.
5. **Explain the cause.** Tie the cause to the symptom, contributing conditions, and any missed safeguard. Distinguish established facts from the leading hypothesis.
6. **Correct when authorized.** Define regression evidence, then make the smallest change that addresses the cause. Do not weaken a valid expectation or merely hide the symptom.
7. **Verify the changed state.** Rerun the reproduction, regression checks, and neighboring checks warranted by the risk. Report failures and gaps rather than inferring success from an attempted run.

If the cause remains unsettled, stop the diagnosis at that boundary and give the checks performed, remaining hypotheses, and next discriminating check. Do not call a cause or fix established.

## Completion

- The result is identified as a verified diagnosis, verified fix, or inconclusive investigation.
- Conclusions are tied to evidence and competing explanations were considered.
- Any changes were authorized, address the cause, and have fresh regression evidence.
- Remaining uncertainty and the next useful check are explicit.
