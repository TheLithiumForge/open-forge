---
open-forge:
  description: Review a code change, technical design, or repository state and report prioritized evidence-backed findings. Use when correctness, regressions, safety, or maintainability need independent scrutiny
  tags: [Extension, Workflow, PhaseVerification, Quality, Review]
---

# Review

Review produces prioritized findings grounded in observable evidence and leaves the reviewed state unchanged by default.

## Mode

iterative

## Goal

- outcome: actionable findings ordered by impact, or an explicit no-findings result with residual risk
- acceptance: each finding identifies evidence, consequence, and a concrete correction or decision
- stop: the review target is unavailable, intended behavior cannot be established, or evidence needed to validate a suspected issue is inaccessible

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- [evidence standards, test judgment, behavior preservation, and debugging discipline](../../skills/quality/SKILL.md) - #Skill #Quality

## Constraints

- Review is read-only unless the user explicitly requests fixes.
- If fixes are requested, keep findings distinct from authorized changes and verify every change.
- For read-only review, run only checks known not to update snapshots, generated files, dependencies, workspace-visible caches, or external state.
- Prioritize correctness, behavior, security, data loss, and regressions over style preferences.
- Do not claim an issue without a reproducible path, direct evidence, or clearly labeled uncertainty.

## Steps

1. Establish the review target, intended behavior, scope, applicable project rules, and a relevant starting-state baseline.
2. Inspect the changed state and enough surrounding code or content to understand ownership and interactions.
3. Trace high-risk paths, boundaries, failure modes, state transitions, and compatibility concerns.
4. Run or inspect proportionate verification when it can confirm or reject a suspected issue. In read-only mode, use only checks known to preserve the captured state.
5. For each real issue, record location, severity, evidence, consequence, and the smallest credible correction.
6. Challenge each candidate finding for false positives, out-of-scope assumptions, and existing safeguards.
7. Compare final state with the baseline and account for every change before claiming the review remained read-only.
8. Report prioritized findings first, then questions, verification gaps, and residual risk.
9. If fixes were explicitly requested, make only the authorized corrections and rerun relevant verification.

## Loop

Iterative while evidence changes the assessment: repeat steps 2 through 7 for each affected path, and steps 5 through 9 for explicitly requested fixes. Stop when material risks are covered or a blocker prevents a defensible conclusion.

## Outputs

- prioritized evidence-backed findings, or an explicit no-findings result
- questions and assumptions affecting confidence
- verification performed and gaps
- authorized fixes and their verification, when explicitly requested

## Completion

- [ ] every finding has evidence, consequence, and correction guidance
- [ ] false positives and existing safeguards were considered
- [ ] final state was compared with the starting baseline and remained unchanged unless fixes were explicitly requested
- [ ] verification gaps and residual risk are stated
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
