---
open-forge:
  description: "Inspect a change or design and return prioritized, evidence-backed findings"
  tags: [Extension, Workflow, Quality, Review]
---

# Review

## Goal

Produce actionable findings in priority order, or a clear no-findings result, without making conclusions stronger than the evidence. The default is read-only review.

## Steps

1. **Fix the review target.** Identify the intended behavior, scope, applicable rules, exact baseline, changed and untracked artifacts, and claimed evidence. Bind the review to that state so later changes can be distinguished.
2. **Read the relevant context.** Inspect the target and enough of its neighbors to understand responsibilities, interactions, requirements, and existing safeguards.
3. **Trace consequential failures.** Prioritize correctness, security, data loss, regressions, compatibility, and broken contracts over stylistic preference. Examine high-risk paths, state transitions, and evidence gaps.
4. **Check knowledge changes.** When durable sources changed, reconcile accepted outcomes with their resulting records. Look for omissions, stale candidates, unjustified acceptance, duplication, and incorrect role, scope, or lifetime.
5. **Test candidate findings.** Use only checks permitted by the review boundary. A read-only request does not authorize changes to snapshots, generated files, dependencies, caches, or external state. Challenge each finding against counterevidence and possible false positives.
6. **Report what matters.** Give each material finding a stable ID, priority, location, evidence, consequence, and smallest credible correction. Label uncertainty. Put findings before questions, verification gaps, and residual risk.
7. **Account for changes.** Compare the final state with the reviewed baseline before claiming the review stayed read-only. When corrections are separately authorized, revalidate findings, record their dispositions, and obtain fresh affected evidence. Repeat the whole review only if its scope was materially invalidated.

## Completion

- Each finding has evidence, consequence, and useful correction guidance; no-findings results state the inspected scope and evidence limits.
- Relevant durable knowledge transitions and counterevidence were considered.
- The target remained unchanged unless corrections were authorized. Any correction has a disposition and fresh affected evidence.
- Uncertainty, verification gaps, and residual risk are explicit.
