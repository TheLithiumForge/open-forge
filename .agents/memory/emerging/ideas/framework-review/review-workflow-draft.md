---
open-forge:
  description: Retained evidence-focused review recipe draft for the user to refine later
  tags: [Memory, Idea, Contextual, Candidate, Framework, Review]
---

# Evidence-Focused Review Workflow Draft

This candidate was retained at the user's request for later refinement. Parts of the idea have since informed the shipped Review Workflow. This exact draft remains proposed content and does not define the installed method. Extension refinement is deferred; [Task 28](../../../archived/cli-development/tasks/source-framework-review.md) records the accepted boundary.

The [evidence assessment](../../../archived/framework-review/analysis/e01-dogfood-evidence.md) preserves its motivation and limits. The [original patch](review-workflow-proposal.diff) is supporting draft text.

## Candidate File

```markdown
---
open-forge:
  description: Review a change, design, or repository state and report prioritized evidence-backed findings without modifying it by default
  tags: [Extension, Workflow, Quality, Review]
---

# Review

## Goal

Produce prioritized actionable findings, or a clear no-findings result, without making conclusions stronger than the evidence.

## Steps

1. Establish the review target, intended behavior, scope, workspace rules, and relevant starting state. Record the baseline, any changed or untracked material included in the target, and the claimed verification.
2. Inspect the target and enough surrounding context to understand responsibilities, interactions, and accepted constraints.
3. When durable knowledge changed, compare what was accepted with every resulting durable source. Report missing outcomes, stale candidates, excessive promotion, duplication, and content placed in the wrong role, scope, authority state, or lifetime.
4. Trace high-risk paths, boundaries, failure modes, state transitions, compatibility concerns, and evidence gaps. Prioritize correctness, security, data loss, regressions, and broken contracts over style preference.
5. Run or inspect verification matched to the suspected issue. In read-only work, use only checks known not to change snapshots, generated files, dependencies, caches, or external state.
6. Give each finding a location, evidence, consequence, and smallest credible correction. Assign each material finding a stable identifier. Label uncertainty that could change the finding.
7. Challenge every candidate finding against existing safeguards, context, and possible false positives.
8. Compare the final state with the baseline and account for every change before claiming the review remained read-only.
9. Report findings in priority order, followed by questions, verification gaps, and residual risk. Make fixes only when explicitly requested, and keep them distinct from the findings they address.
10. If authorized corrections follow, record the disposition of each material finding and revalidate it against the changed target. Recheck affected findings and their relevant context. Repeat the full review only when the changes materially alter what must be reviewed.

## Completion

- Every reported finding has evidence, consequence, and correction guidance. Material findings retain stable identifiers through any correction and recheck.
- Durable knowledge transitions were reconciled when relevant.
- False positives and existing safeguards were considered.
- The final state remained unchanged unless fixes were explicitly requested.
- Verification gaps, uncertainty, and residual risk are stated.
```
