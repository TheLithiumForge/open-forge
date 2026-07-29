---
open-forge:
  description: Review a change, design, or repository state and report prioritized evidence-backed findings without modifying it by default
  tags: [Extension, Workflow, Quality, Review]
---

# Review

## Goal

Produce prioritized actionable findings, or an explicit no-findings result, with conclusions no stronger than the available evidence.

## Steps

1. Establish the review target, intended behavior, scope, applicable workspace rules, and a relevant starting-state baseline.
2. Inspect the target and enough surrounding context to understand responsibilities, interactions, and accepted constraints.
3. Trace high-risk paths, boundaries, failure modes, state transitions, compatibility concerns, and evidence gaps. Prioritize correctness, security, data loss, regressions, and broken contracts over style preference.
4. Run or inspect proportionate verification when it can confirm or reject a suspected issue. In read-only work, use only checks known not to mutate snapshots, generated files, dependencies, caches, or external state.
5. Give each finding a location, evidence, consequence, and smallest credible correction. Label material uncertainty.
6. Challenge every candidate finding against existing safeguards, context, and possible false positives.
7. Compare the final state with the baseline and account for every change before claiming the review remained read-only.
8. Report findings in priority order, followed by questions, verification gaps, and residual risk. Make fixes only when explicitly requested, and keep them distinct from the findings they address.

## Completion

- Every reported finding has evidence, consequence, and correction guidance.
- False positives and existing safeguards were considered.
- The final state remained unchanged unless fixes were explicitly requested.
- Verification gaps, uncertainty, and residual risk are stated.
