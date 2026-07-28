---
open-forge:
  description: Reproduce and isolate a defect, identify its root cause, and verify an authorized minimal fix. Use for failures, regressions, flaky behavior, or unexplained state
  tags: [Extension, Workflow, PhaseDelivery, Quality, Debugging]
---

# Debugging

Debugging turns a symptom into a reproducible root cause and, when authorized, a verified minimal fix.

## Mode

iterative

## Goal

- outcome: a root-cause explanation supported by evidence, plus an authorized fix and regression proof when requested
- acceptance: the failure is reproduced or bounded, competing hypotheses are tested, and conclusions explain the observed evidence
- stop: reproduction requires unavailable inputs, observation would exceed authority, or the remaining hypotheses need a user or environment decision

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- [systematic reproduction, hypothesis testing, minimal fixes, and regression evidence](../../skills/quality/SKILL.md) - #Skill #Quality

## Constraints

- Diagnose before changing behavior. Do not use speculative edits as the primary investigation method.
- Preserve original failure evidence and distinguish symptom, trigger, contributing condition, and root cause.
- Make a fix only when the task authorizes mutation.
- Keep the fix minimal and add or identify regression evidence tied to the root cause.

## Steps

1. Capture the observed symptom, expected behavior, environment, frequency, inputs, and recent relevant changes.
2. Reproduce the failure with the smallest reliable case, or bound why reproduction is unavailable.
3. Trace the failing path and collect observations at boundaries where actual behavior diverges from expected behavior.
4. Form competing hypotheses and test the cheapest discriminating hypothesis first.
5. Identify the root cause and explain how it produces the symptom and why existing safeguards missed it.
6. If a fix is authorized, define a regression case, apply the smallest cause-level correction, and avoid masking the symptom.
7. Rerun the reproduction, regression check, and proportionate neighboring verification.
8. Report evidence, rejected hypotheses, fix status, verification, and remaining uncertainty.

## Loop

Repeat steps 2 through 5 until one hypothesis explains the evidence or investigation is blocked. For an authorized fix, repeat steps 6 and 7 until the regression evidence passes or the proposed cause is disproven.

## Outputs

- reproduction or bounded reproduction gap
- observations, hypotheses, and rejected alternatives
- root-cause explanation
- authorized minimal fix and regression evidence, when requested
- residual uncertainty and next discriminating check

## Completion

- [ ] symptom, trigger, and root cause are distinguished
- [ ] conclusions are tied to observed evidence
- [ ] mutation occurred only when authorized
- [ ] any fix addresses the cause and has regression evidence
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
