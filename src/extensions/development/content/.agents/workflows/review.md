---
open-forge:
  description: Review a change, design, or repository state and report prioritized evidence-backed findings without modifying it by default
  tags: [Extension, Workflow, Quality, Review]
---

# Review

## Goal

Produce prioritized actionable findings, or a clear no-findings result, without making conclusions stronger than the evidence.

## Steps

1. Establish the review target, intended behavior, scope, applicable project rules, baseline, exact changed and untracked artifacts, and claimed evidence. Resolve review criteria and acceptance requirements from the relevant project scopes and defining sources. Bind conclusions to that target so later changes can be distinguished.
2. Inspect the target and enough surrounding context to understand responsibilities, interactions, and accepted constraints.
3. When durable knowledge changed, compare what was accepted with every resulting durable source. Report missing outcomes, stale candidates, excessive promotion, duplication, and content placed in the wrong role, scope, authority state, or lifetime.
4. Trace high-risk paths, boundaries, failure modes, state transitions, compatibility concerns, and evidence gaps. Prioritize correctness, security, data loss, regressions, and broken contracts over style preference.
5. Use the project's verification procedures where they apply, and run or inspect checks matched to the suspected issue. In read-only work, use only checks known not to change snapshots, generated files, dependencies, caches, or external state.
6. Give each material finding a stable ID, priority, location, evidence, consequence, and smallest credible correction. Label uncertainty that could change the finding.
7. Challenge every candidate finding against existing safeguards, context, and possible false positives.
8. Compare the final state with the baseline and account for every change before claiming the review remained read-only.
9. Report findings in priority order, followed by questions, verification gaps, and residual risk.

   When corrections are authorized, revalidate each finding against current content and record its disposition. Keep corrections tied to finding IDs, run the affected checks to obtain fresh evidence, and recheck the affected findings and context. Repeat a complete review only when the changes materially expand or invalidate its scope.

## Completion

- Every reported finding has evidence, consequence, and correction guidance.
- Durable knowledge transitions were reconciled when relevant.
- False positives and existing safeguards were considered.
- The review target remained unchanged unless corrections were authorized. Any correction has a finding disposition and fresh affected evidence.
- Verification gaps, uncertainty, and residual risk are stated.
